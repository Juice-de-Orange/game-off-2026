using System.Collections.Generic;
using GameOff.Characters;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Rest;
using UnityEngine;

namespace GameOff.Core
{
    /// <summary>
    /// P128: the morning save. Capture reads a Run (the live objects of one run) into a SaveData, Apply writes one back; both are
    /// Unity-light and EditMode-tested. CaptureRunning / ApplyRunning connect them to the scene's singletons (PlayerEconomy,
    /// Confiscation, CharacterDirector, StationMood, the static Market; P131: the family's stock at home). DayManager saves at the
    /// very start of every morning, before DayStarted: so a continued run replays that morning exactly (spoiling, refills,
    /// character plan, letter).
    /// Saving mid-day is deliberately not possible (DESIGN.md).
    /// </summary>
    public static class SaveGame
    {
        /// <summary>The live state of one run that a save reads and restores. Objects are changed in place; the plain fields are set.</summary>
        public class Run
        {
            public int day;
            public int seed;
            public Wallet wallet;
            public Inventory inventory;
            public Inventory locker;
            /// <summary>P131: the family's stock at home.</summary>
            public HomeDelivery home;
            public Household household;
            public Conduct conduct;
            public CharacterMemory memory;
            /// <summary>The characters of this run: Capture finds the owner of a pending consequence among them.</summary>
            public IEnumerable<RecurringCharacter> characters = new RecurringCharacter[0];
            public ItemData wearing;
            public bool hasTicket;
            public int barkeeperSulksFrom;
            public float stationMood;
            public List<string> letterLines = new List<string>();
            /// <summary>Vendor extras by stock asset name.</summary>
            public Dictionary<string, List<(ItemData item, int count)>> vendorExtras = new Dictionary<string, List<(ItemData, int)>>();
            /// <summary>Market events scheduled for later days (Market.Schedule).</summary>
            public Dictionary<int, List<MarketEvent>> scheduledEvents = new Dictionary<int, List<MarketEvent>>();
            /// <summary>P132: the cassette in each slot of each tape deck (by deck id, slot 1 first, null = empty).</summary>
            public Dictionary<string, List<CassetteData>> decks = new Dictionary<string, List<CassetteData>>();
            /// <summary>P132: ids of the cassettes already picked up on the station.</summary>
            public List<string> pickedUp = new List<string>();
        }

        /// <summary>Finds items, market events and characters by their saved id.</summary>
        public class Lookup
        {
            private readonly Dictionary<string, ItemData> _items = new Dictionary<string, ItemData>();
            private readonly Dictionary<string, MarketEvent> _events = new Dictionary<string, MarketEvent>();
            private readonly Dictionary<string, RecurringCharacter> _characters = new Dictionary<string, RecurringCharacter>();

            public Lookup(IEnumerable<ItemData> items, IEnumerable<MarketEvent> events, IEnumerable<RecurringCharacter> characters)
            {
                if (items != null) foreach (var i in items) if (i != null && !_items.ContainsKey(i.name)) _items[i.name] = i;
                if (events != null) foreach (var e in events) if (e != null && !_events.ContainsKey(e.name)) _events[e.name] = e;
                if (characters != null) foreach (var c in characters) if (c != null && !_characters.ContainsKey(CharacterId(c))) _characters[CharacterId(c)] = c;
            }

            /// <summary>Everything of these types that is loaded: in the game all of it hangs on Main (tunings, stocks, characters).</summary>
            public static Lookup FromLoadedAssets(IEnumerable<RecurringCharacter> characters) =>
                new Lookup(Resources.FindObjectsOfTypeAll<ItemData>(), Resources.FindObjectsOfTypeAll<MarketEvent>(),
                    characters ?? Resources.FindObjectsOfTypeAll<RecurringCharacter>());

            public ItemData Item(string id) => Find(_items, id, "item");
            public MarketEvent Event(string id) => Find(_events, id, "market event");
            public RecurringCharacter Character(string id) => Find(_characters, id, "character");

            private static T Find<T>(Dictionary<string, T> map, string id, string what) where T : class
            {
                if (string.IsNullOrEmpty(id)) return null;
                if (map.TryGetValue(id, out var found)) return found;
                Debug.LogWarning($"[Save] unknown {what} '{id}' in the save, skipped");
                return null;
            }
        }

        /// <summary>The character's id field, or its asset name if the id is empty.</summary>
        public static string CharacterId(RecurringCharacter c) => c == null ? "" : string.IsNullOrEmpty(c.Id) ? c.name : c.Id;

        // ---------- Capture ----------

        public static SaveData Capture(Run run)
        {
            var d = new SaveData { day = run.day, seed = run.seed };
            if (run.wallet != null) d.credits = run.wallet.Balance;
            d.inventory = Stacks(run.inventory);
            d.wearing = run.wearing != null ? run.wearing.name : "";
            d.locker = Stacks(run.locker);
            if (run.home != null) d.home = Stacks(run.home.Items);
            foreach (var pair in run.vendorExtras)
            {
                var shelf = new SaveData.VendorShelf { stock = pair.Key };
                foreach (var (item, count) in pair.Value)
                    if (item != null && count > 0) shelf.extras.Add(new SaveData.ItemStack { item = item.name, count = count });
                if (shelf.extras.Count > 0) d.vendors.Add(shelf);
            }
            if (run.household != null)
            {
                for (int i = 0; i < run.household.Count; i++)
                {
                    d.family.Add(run.household.Condition(i));
                    d.criticalNights.Add(run.household.CriticalNights(i));
                }
                d.diedTonight.AddRange(run.household.DiedTonight);
            }
            if (run.conduct != null)
            {
                d.disciplinary = run.conduct.Disciplinary;
                d.morale = run.conduct.Morale;
                d.airlockMorale = run.conduct.AirlockMorale;
                d.airlocks = run.conduct.Airlocks;
                d.innocentAirlocks = run.conduct.InnocentAirlocks;
            }
            d.hasTicket = run.hasTicket;
            d.barkeeperSulksFrom = run.barkeeperSulksFrom;
            d.letterLines.AddRange(run.letterLines);
            d.marketEvents = DayEventList(run.scheduledEvents.Keys, day => run.scheduledEvents[day], run.day);
            d.stationMood = run.stationMood;
            if (run.memory != null) d.characters = CaptureMemory(run.memory, run.characters);
            CaptureCassettes(run, d);
            return d;
        }

        /// <summary>P132: tape decks and picked-up cassettes, by name and id.</summary>
        private static void CaptureCassettes(Run run, SaveData d)
        {
            foreach (var pair in run.decks)
            {
                if (string.IsNullOrEmpty(pair.Key) || pair.Value == null) continue;
                var deck = new SaveData.DeckSlots { deck = pair.Key };
                foreach (var cassette in pair.Value) deck.slots.Add(cassette != null ? cassette.name : "");
                d.decks.Add(deck);
            }
            foreach (var id in run.pickedUp)
                if (!string.IsNullOrEmpty(id) && !d.pickedUp.Contains(id)) d.pickedUp.Add(id);
        }

        private static SaveData.CharacterState CaptureMemory(CharacterMemory m, IEnumerable<RecurringCharacter> characters)
        {
            var s = new SaveData.CharacterState();
            foreach (var r in m.Records)
                s.records.Add(new SaveData.Record { character = CharacterId(r.character), appearance = r.appearance, day = r.day, outcome = (int)r.outcome });
            s.flags.AddRange(m.Flags);
            foreach (var pair in m.EntrustedItems)
                s.entrusted.Add(new SaveData.Entrusted { character = CharacterId(pair.Key), item = pair.Value.item != null ? pair.Value.item.name : "", deposit = pair.Value.deposit });
            foreach (var (day, effect) in m.Effects)
            {
                if (!FindConsequence(effect, characters, out var character, out int appearance, out int index))
                {
                    Debug.LogWarning("[Save] a pending character effect belongs to no character, not saved");
                    continue;
                }
                s.effects.Add(new SaveData.Effect { day = day, character = CharacterId(character), appearance = appearance, consequence = index });
            }
            foreach (var w in m.Wanted)
                s.wanted.Add(new SaveData.Wanted { character = CharacterId(w.character), entry = w.entry, fromDay = w.fromDay, toDay = w.toDay });
            s.events = DayEventList(m.EventDays, day => m.EventsOn(day), int.MinValue);
            foreach (var a in m.Arrests) s.arrests.Add(new SaveData.Arrest { name = a.name, species = a.species, day = a.day });
            foreach (var pair in m.KnownKin)
                s.kin.Add(new SaveData.Kin { character = CharacterId(pair.Key), name = pair.Value.name, byYou = pair.Value.byYou });
            return s;
        }

        /// <summary>Consequences are plain objects inside a character's appearances: they are saved by character, appearance and index.</summary>
        private static bool FindConsequence(Consequence effect, IEnumerable<RecurringCharacter> characters,
            out RecurringCharacter character, out int appearance, out int index)
        {
            character = null;
            appearance = index = -1;
            if (effect == null || characters == null) return false;
            foreach (var c in characters)
            {
                if (c == null) continue;
                for (int a = 0; a < c.Appearances.Count; a++)
                {
                    int i = c.Appearances[a].consequences.IndexOf(effect);
                    if (i < 0) continue;
                    character = c;
                    appearance = a;
                    index = i;
                    return true;
                }
            }
            return false;
        }

        private static List<SaveData.ItemStack> Stacks(Inventory inventory)
        {
            var list = new List<SaveData.ItemStack>();
            if (inventory == null) return list;
            foreach (var item in inventory.Items) list.Add(new SaveData.ItemStack { item = item.name, count = inventory.Count(item) });
            return list;
        }

        /// <summary>P131: the stock at home as stacks, each item once, in the order it first appears.</summary>
        private static List<SaveData.ItemStack> Stacks(IReadOnlyList<ItemData> items)
        {
            var list = new List<SaveData.ItemStack>();
            foreach (var item in items)
            {
                if (item == null) continue;
                var stack = list.Find(s => s.item == item.name);
                if (stack != null) stack.count++;
                else list.Add(new SaveData.ItemStack { item = item.name, count = 1 });
            }
            return list;
        }

        private static List<SaveData.DayEvents> DayEventList(IEnumerable<int> days, System.Func<int, IReadOnlyList<MarketEvent>> on, int fromDay)
        {
            var list = new List<SaveData.DayEvents>();
            var sorted = new List<int>(days);
            sorted.Sort();
            foreach (int day in sorted)
            {
                if (day < fromDay) continue;
                var entry = new SaveData.DayEvents { day = day };
                foreach (var e in on(day)) if (e != null) entry.events.Add(e.name);
                if (entry.events.Count > 0) list.Add(entry);
            }
            return list;
        }

        // ---------- Apply ----------

        /// <summary>Writes the save into a fresh run (the objects as a new game creates them).</summary>
        public static void Apply(SaveData d, Run run, Lookup lookup)
        {
            run.day = d.day;
            run.seed = d.seed;
            if (run.wallet != null) run.wallet.Restore(d.credits);
            Fill(run.inventory, d.inventory, lookup);
            run.wearing = lookup.Item(d.wearing);
            Fill(run.locker, d.locker, lookup);
            if (run.home != null)
            {
                var stock = new List<ItemData>();
                if (d.home != null)
                    foreach (var s in d.home)
                    {
                        var item = lookup.Item(s.item);
                        for (int i = 0; item != null && i < s.count; i++) stock.Add(item);
                    }
                run.home.Restore(stock);
            }
            run.vendorExtras.Clear();
            foreach (var shelf in d.vendors)
            {
                var extras = new List<(ItemData, int)>();
                foreach (var s in shelf.extras)
                {
                    var item = lookup.Item(s.item);
                    if (item != null && s.count > 0) extras.Add((item, s.count));
                }
                if (extras.Count > 0 && !string.IsNullOrEmpty(shelf.stock)) run.vendorExtras[shelf.stock] = extras;
            }
            if (run.household != null) run.household.Restore(d.family, d.criticalNights, d.diedTonight);
            if (run.conduct != null) run.conduct.Restore(d.disciplinary, d.morale, d.airlockMorale, d.airlocks, d.innocentAirlocks);
            run.hasTicket = d.hasTicket;
            run.barkeeperSulksFrom = d.barkeeperSulksFrom;
            run.letterLines.Clear();
            run.letterLines.AddRange(d.letterLines);
            run.scheduledEvents.Clear();
            foreach (var day in d.marketEvents)
                foreach (var id in day.events)
                {
                    var e = lookup.Event(id);
                    if (e == null) continue;
                    if (!run.scheduledEvents.TryGetValue(day.day, out var list)) run.scheduledEvents[day.day] = list = new List<MarketEvent>();
                    if (!list.Contains(e)) list.Add(e);
                }
            run.stationMood = d.stationMood;
            if (run.memory != null && d.characters != null) ApplyMemory(d.characters, run.memory, lookup);
            ApplyCassettes(d, run, lookup);
        }

        /// <summary>P132: tape decks and picked-up cassettes; a save from before P132 has neither.</summary>
        private static void ApplyCassettes(SaveData d, Run run, Lookup lookup)
        {
            run.decks.Clear();
            if (d.decks != null)
                foreach (var deck in d.decks)
                {
                    if (deck == null || string.IsNullOrEmpty(deck.deck)) continue;
                    var slots = new List<CassetteData>();
                    foreach (var name in deck.slots) slots.Add(lookup.Item(name) as CassetteData);
                    run.decks[deck.deck] = slots;
                }
            run.pickedUp.Clear();
            if (d.pickedUp != null) run.pickedUp.AddRange(d.pickedUp);
        }

        private static void ApplyMemory(SaveData.CharacterState s, CharacterMemory m, Lookup lookup)
        {
            foreach (var r in s.records)
            {
                var c = lookup.Character(r.character);
                if (c != null) m.Remember(c, r.appearance, r.day, (Outcome)r.outcome);
            }
            foreach (var flag in s.flags) m.SetFlag(flag);
            foreach (var e in s.entrusted)
            {
                var c = lookup.Character(e.character);
                var item = lookup.Item(e.item);
                if (c != null && item != null) m.Entrust(c, item, e.deposit);
            }
            foreach (var e in s.effects)
            {
                var c = lookup.Character(e.character);
                if (c == null || e.appearance < 0 || e.appearance >= c.Appearances.Count) continue;
                var list = c.Appearances[e.appearance].consequences;
                if (e.consequence >= 0 && e.consequence < list.Count) m.ScheduleEffect(e.day, list[e.consequence]);
            }
            foreach (var w in s.wanted)
            {
                var c = lookup.Character(w.character);
                if (c != null) m.RestoreWanted(new CharacterMemory.PendingWanted { character = c, entry = w.entry, fromDay = w.fromDay, toDay = w.toDay });
            }
            foreach (var day in s.events)
                foreach (var id in day.events) m.ScheduleEvent(day.day, lookup.Event(id));
            foreach (var a in s.arrests) m.RememberArrest(a.name, a.species, a.day);
            foreach (var k in s.kin) m.RestoreKin(lookup.Character(k.character), k.name, k.byYou);
        }

        private static void Fill(Inventory inventory, List<SaveData.ItemStack> stacks, Lookup lookup)
        {
            if (inventory == null) return;
            inventory.Clear();
            foreach (var s in stacks)
            {
                var item = lookup.Item(s.item);
                if (item != null && s.count > 0) inventory.Add(item, s.count);
            }
        }

        // ---------- The running game ----------

        /// <summary>Reads the scene's run on the morning of <paramref name="day"/> (before DayStarted).</summary>
        public static SaveData CaptureRunning(int day, int seed)
        {
            var economy = PlayerEconomy.Instance;
            var confiscation = Confiscation.Instance;
            var director = CharacterDirector.Instance;
            var run = new Run
            {
                day = day,
                seed = seed,
                wallet = economy != null ? economy.Wallet : null,
                inventory = economy != null ? economy.Inventory : null,
                home = economy != null ? economy.Delivery : null,
                household = economy != null ? economy.Household : null,
                locker = confiscation != null ? confiscation.Locker : null,
                conduct = confiscation != null ? confiscation.Conduct : null,
                memory = director != null ? director.Memory : null,
                stationMood = StationMood.Instance != null ? StationMood.Instance.Situation : 0f,
            };
            if (economy != null)
            {
                run.wearing = economy.Wearing;
                run.hasTicket = economy.HasTicket;
                run.barkeeperSulksFrom = economy.BarkeeperSulksFrom;
                run.letterLines.AddRange(economy.LetterLines);
                run.vendorExtras = economy.VendorExtras(); // also what a continued run restored for shelves nobody opened yet
            }
            foreach (int d in Market.ScheduledDays)
                run.scheduledEvents[d] = new List<MarketEvent>(Market.ScheduledFor(d));
            run.characters = director != null ? director.Characters : (IEnumerable<RecurringCharacter>)Resources.FindObjectsOfTypeAll<RecurringCharacter>();
            foreach (var deck in CassetteDeck.All) // P132
                if (deck != null) run.decks[deck.Id] = new List<CassetteData>(deck.Slots);
            foreach (var pickup in CassettePickup.All)
                if (pickup != null && pickup.Taken) run.pickedUp.Add(pickup.Id);
            return Capture(run);
        }

        /// <summary>Writes a save into the scene's fresh run (after every Awake/Start, before the first morning).</summary>
        public static void ApplyRunning(SaveData d)
        {
            var economy = PlayerEconomy.Instance;
            var confiscation = Confiscation.Instance;
            var director = CharacterDirector.Instance;
            var run = new Run
            {
                wallet = economy != null ? economy.Wallet : null,
                inventory = economy != null ? economy.Inventory : null,
                home = economy != null ? economy.Delivery : null,
                household = economy != null ? economy.Household : null,
                locker = confiscation != null ? confiscation.Locker : null,
                conduct = confiscation != null ? confiscation.Conduct : null,
                memory = director != null ? director.Memory : null,
            };
            Apply(d, run, Lookup.FromLoadedAssets(director != null ? director.Characters : null));
            if (economy != null)
            {
                economy.Wearing = run.wearing;
                economy.HasTicket = run.hasTicket;
                economy.BarkeeperSulksFrom = run.barkeeperSulksFrom;
                foreach (var line in run.letterLines) economy.AddLetterLine(line);
                foreach (var pair in run.vendorExtras) economy.RestoreExtras(pair.Key, pair.Value);
            }
            foreach (var pair in run.scheduledEvents)
                foreach (var e in pair.Value) Market.Schedule(pair.Key, e);
            if (StationMood.Instance != null) StationMood.Instance.SetSituation(run.stationMood);
            foreach (var deck in CassetteDeck.All) // P132
            {
                if (deck == null) continue;
                run.decks.TryGetValue(deck.Id, out var slots);
                deck.Restore(slots);
            }
            foreach (var pickup in CassettePickup.All)
                if (pickup != null && run.pickedUp.Contains(pickup.Id)) pickup.SetTaken(true);
            Debug.Log($"[Save] continued on day {d.day}: {d.credits} credits, {d.inventory.Count} item stacks, {d.characters.records.Count} character records");
        }
    }
}
