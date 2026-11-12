using System.Collections.Generic;
using System.Linq;
using GameOff.Day;
using GameOff.Economy;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Plays the economy for N days without the game (P71), with the real rules: Market (prices, events, amounts), VendorSupply,
    /// VendorStock prices, the bed price (HouseholdTuning.MoneyCost), Payroll, Household and the barkeeper's rules. Days beyond the
    /// story are drawn from the loop pool (P73) with a fixed seed; without a pool they loop over the story days. Used by the Economy Report and the balance tests.
    /// P131: what goes home (kiosk goods, gifts, pilfered goods) lands in the family's stock (HomeDelivery) like in the game: each night
    /// uses one item per due need, perishable leftovers spoil, the rest stays. The styles still only send what tonight's needs lack.
    /// </summary>
    public class EconomySimulator
    {
        public enum Style
        {
            /// <summary>Buys the cheapest goods for food and heat (medicine when sick) at the kiosk and sends them home; the bed for the rest.</summary>
            Clever,
            /// <summary>Pays every need with money at the bed.</summary>
            Bed,
            /// <summary>Pays only the rent.</summary>
            Skip,
            /// <summary>Clever plus the barkeeper's wish and speculating on the rumour; turns bribes down (the barkeeper sulks).</summary>
            Rumour,
            /// <summary>Like Rumour, but takes every bribe (one wrong decision more, the envelope on top, no sulking).</summary>
            Corrupt,
            /// <summary>P125: Clever plus pocketing one piece per shift ("Missed that"); sends it home if it covers a need (customs), else sells it.</summary>
            Pilferer,
        }

        public class Evening
        {
            public int Day, Wage, Spent, Balance;
            public int[] Family;
            public bool GameOver, SkippedNeed;
        }

        public class Run
        {
            public Style Style;
            public int Correct, Travelers;
            public readonly List<Evening> Evenings = new List<Evening>();
            /// <summary>First day whose evening ends with enough for the ticket and nobody sick; -1 = never.</summary>
            public int TicketDay = -1;
            public bool SkippedAnyNeed => Evenings.Any(e => e.SkippedNeed);
            public int WorstCondition => Evenings.Count == 0 ? 0 : Evenings.Max(e => e.Family.Max());
            public int SpeculationProfit, SpeculatedUnits, WishCost, Bribes;
            /// <summary>P125: characters' bonuses minus reprimands; fines, goods sold, customs hits and penalties of the Pilferer.</summary>
            public int Bonus, Fines, LootSold, CustomsCaught, Disciplinary;
            public bool Discharged;
            public bool GameOver => Evenings.Any(e => e.GameOver);
        }

        private readonly EconomyTuning _economy;
        private readonly HouseholdTuning _household;
        private readonly DaySchedule _schedule;
        private readonly VendorStock _kiosk, _dealer;
        private readonly int _freeWarnings;
        private readonly ConfiscationTuning _confiscation;

        public EconomySimulator(EconomyTuning economy, HouseholdTuning household, DaySchedule schedule, VendorStock kiosk, VendorStock dealer,
            int freeWarnings = 2, ConfiscationTuning confiscation = null)
        {
            _economy = economy;
            _household = household;
            _schedule = schedule;
            _kiosk = kiosk;
            _dealer = dealer;
            _freeWarnings = freeWarnings;
            _confiscation = confiscation;
        }

        private static List<GameOff.Characters.RecurringCharacter> _story;

        private static List<GameOff.Characters.RecurringCharacter> Story
        {
            get
            {
                if (_story != null) return _story;
                _story = new List<GameOff.Characters.RecurringCharacter>();
                foreach (var g in UnityEditor.AssetDatabase.FindAssets("t:RecurringCharacter", new[] { "Assets/_Project/Data/Characters/Story" }))
                    _story.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<GameOff.Characters.RecurringCharacter>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)));
                return _story;
            }
        }

        /// <summary>
        /// P76: the story characters met on that day when every one is decided as intended; books their outcomes and schedules the
        /// market events they set off (Market.Schedule). Returns how many came.
        /// </summary>
        public static int MeetCharacters(int day, GameOff.Characters.CharacterMemory memory,
            List<(int day, GameOff.Characters.Consequence effect)> later = null)
        {
            var due = GameOff.Characters.CharacterPlanner.Due(day, Story, memory, 3, () => 0f);
            foreach (var p in due)
            {
                var a = p.character.Appearances[p.appearance];
                var outcome = a.OutcomeOf(p.traveler.IntendedVerdict);
                memory.Remember(p.character, p.appearance, day, outcome);
                foreach (var c in a.consequences)
                {
                    if (c.when != outcome) continue;
                    if (c.marketEvent != null) Market.Schedule(day + c.eventInDays, c.marketEvent);
                    if (!c.onlyIfArrested || p.traveler.IntendedVerdict == GameOff.Travelers.Verdict.Arrest) later?.Add((day + c.afterDays, c)); // P125
                }
            }
            return due.Count;
        }

        /// <summary>Forget the cached characters (after the story builder ran).</summary>
        public static void ResetCharacters() => _story = null;

        /// <summary>P73: the seed the simulator draws the endless days with (a fixed run, so the report is the same every time).</summary>
        public const int Seed = 7301;

        public int StoryDays => _schedule.Count;

        private DayEntry Entry(int day) => _schedule.Endless ? _schedule.Get(day, Seed) : _schedule.Get((day - 1) % _schedule.Count + 1);
        private int ScheduleDay(int day) => _schedule.Endless ? day : (day - 1) % _schedule.Count + 1;

        /// <summary>
        /// P125: one shift's confiscated goods for the Pilferer. False = discharged (too many penalties).
        /// <paramref name="open"/>: a need that is due tonight and not covered by the stock at home yet (P131).
        /// </summary>
        private bool Pilfer(DayEntry entry, ref int balance, HomeDelivery home, System.Func<FamilyNeed, bool> open, Run run, System.Random rolls)
        {
            var goods = new List<(ItemData item, int count)>();
            if (entry.rules != null)
                foreach (var t in entry.rules.Travelers)
                {
                    if (t.IntendedVerdict != GameOff.Travelers.Verdict.Arrest) continue;
                    var manifest = t.Document(GameOff.Documents.DocumentType.CargoManifest);
                    if (manifest != null) goods.AddRange(CargoReader.Read(manifest.Get("cargo"), _confiscation.goods, _confiscation.maxPerItem));
                }
            if (goods.Count == 0) return true;

            var picked = goods.OrderByDescending(g => _dealer.OfferFor(g.item)).First().item;
            var outcome = ShiftReport.Resolve(ShiftChoice.MissedThat, goods, picked, _confiscation, (float)rolls.NextDouble());
            run.Fines += outcome.Fine;
            balance = Mathf.Max(0, balance - outcome.Fine);
            run.Disciplinary += outcome.Disciplinary;
            foreach (var (item, count) in outcome.Kept)
                for (int i = 0; i < count; i++)
                {
                    bool forTheFamily = System.Enum.GetValues(typeof(FamilyNeed)).Cast<FamilyNeed>()
                        .Any(n => open(n) && item.HasTag(_household.TagOf(n)));
                    if (forTheFamily)
                    {
                        if (DroneCustoms.Resolve(new[] { item }, _confiscation, (float)rolls.NextDouble()).Caught)
                        {
                            run.CustomsCaught++;
                            run.Disciplinary++;
                        }
                        else home.Add(item);
                    }
                    else if (_dealer.Buys(item))
                    {
                        int offer = _dealer.OfferFor(item);
                        balance += offer;
                        run.LootSold += offer;
                    }
                }
            return run.Disciplinary < _confiscation.disciplinaryLimit;
        }

        /// <summary>Plays `days` days. `wrongPerDay` travelers each day are decided wrongly.</summary>
        public Run Play(Style style, int wrongPerDay, int days)
        {
            var run = new Run { Style = style };
            int balance = _economy.startCredits;
            var family = new Household(_household.members.Select(m => m.feelsCold).ToList());
            var held = new Dictionary<ItemData, int>(); // speculation stock carried to the next evening
            int sulksFrom = 0; // P66: the barkeeper sulks the day a bribe is turned down and the next
            var later = new List<(int day, GameOff.Characters.Consequence effect)>(); // P125: bonuses and gifts still to come
            var rolls = new System.Random(Seed); // P125: the Pilferer's luck, the same in every report
            var memory = new GameOff.Characters.CharacterMemory(); // P76: who was met and how it went
            var home = new HomeDelivery(); // P131: the family's stock, carried from night to night
            var needs = System.Enum.GetValues(typeof(FamilyNeed)).Cast<FamilyNeed>().ToArray();
            try
            {
                for (int day = 1; day <= days; day++)
                {
                    var entry = Entry(day);
                    Market.Begin(ScheduleDay(day), entry.marketEvents);
                    // P76: the recurring characters come on top of the day's own travelers – on the path of an officer who
                    // decides every one of them right (their outcomes set off tomorrow's events, e.g. Quill arrested → smuggler raid).
                    int travelers = (entry.rules != null ? entry.rules.Travelers.Count : 0) + MeetCharacters(ScheduleDay(day), memory, later);
                    int correct = Mathf.Max(0, travelers - wrongPerDay);
                    int bribeMoney = 0;
                    var bribe = entry.bribe;
                    if (bribe != null && bribe.Active)
                    {
                        if (style == Style.Corrupt)
                        {
                            bribeMoney = bribe.amount;
                            run.Bribes += bribe.amount;
                            if (bribe.traveler.IntendedVerdict != GameOff.Travelers.Verdict.Admit && correct > 0) correct--;
                        }
                        else sulksFrom = day;
                    }
                    run.Travelers = travelers;
                    run.Correct = correct;
                    int wage = Payroll.Wage(correct, _household) + bribeMoney;
                    int penalty = Payroll.Penalty(travelers - correct, _freeWarnings, _household);
                    int rent = _household.rent;
                    var kiosk = new VendorSupply(_kiosk);
                    var dealer = new VendorSupply(_dealer);
                    int spent = 0;
                    var evening = new Evening { Day = day, Wage = wage - penalty };

                    // Evening, before the bed: sell yesterday's speculation, then shopping.
                    foreach (var pair in held.ToList())
                    {
                        int offer = _dealer.OfferFor(pair.Key);
                        balance += offer * pair.Value;
                        run.SpeculationProfit += offer * pair.Value;
                    }
                    held.Clear();

                    // The money left after tonight's fixed bill must stay ≥ 0, so shopping keeps the rent safe.
                    int Reserve() => balance + wage - penalty - rent;
                    bool Needed(FamilyNeed n) => n != FamilyNeed.Medicine || family.AnySick;
                    // P131: the item at home for every need tonight (one item per need), like the settlement.
                    ItemData[] FromHome() => home.Covering(needs.Select(n => Needed(n) ? _household.TagOf(n) : null).ToArray(), _economy.perishableTag);
                    bool Open(FamilyNeed n) => Needed(n) && FromHome()[(int)n] == null;

                    // P125: what the characters' outcomes bring today: a bonus or reprimand, a gift for the family.
                    int dueDay = ScheduleDay(day);
                    foreach (var (_, effect) in later.Where(l => l.day == dueDay))
                    {
                        balance = Mathf.Max(0, balance + effect.wageChange);
                        run.Bonus += effect.wageChange;
                        if (effect.giftItem == null || (effect.giftOnlyIfFamilySick && !family.AnySick)) continue;
                        home.AddGift(effect.giftItem);
                    }
                    later.RemoveAll(l => l.day == dueDay);

                    if (style == Style.Pilferer && _confiscation != null && !Pilfer(entry, ref balance, home, Open, run, rolls))
                    {
                        run.Discharged = evening.GameOver = true;
                        evening.Balance = balance;
                        evening.Family = Enumerable.Range(0, family.Count).Select(family.Condition).ToArray();
                        run.Evenings.Add(evening);
                        break;
                    }

                    if (style == Style.Clever || style == Style.Rumour || style == Style.Corrupt || style == Style.Pilferer)
                        foreach (FamilyNeed need in System.Enum.GetValues(typeof(FamilyNeed)))
                        {
                            if (!Open(need)) continue;
                            var tag = _household.TagOf(need);
                            var cheapest = kiosk.Items.Where(i => i.HasTag(tag) && kiosk.Count(i) > 0)
                                .OrderBy(i => _kiosk.PriceFor(i)).FirstOrDefault();
                            if (cheapest == null) continue;
                            int price = _kiosk.PriceFor(cheapest);
                            if (price > balance || price > Reserve()) continue;
                            kiosk.Take(cheapest);
                            balance -= price;
                            spent += price;
                            home.Add(cheapest);
                        }

                    bool sulking = sulksFrom > 0 && day >= sulksFrom && day <= sulksFrom + 1;
                    if ((style == Style.Rumour || style == Style.Corrupt) && day < days && !sulking)
                    {
                        var wish = entry.barWish;
                        if (wish != null && wish.Active)
                        {
                            // Cheapest matching goods at kiosk or dealer, as many as wished.
                            var offers = kiosk.Items.Where(i => i.HasTag(wish.tag)).Select(i => (i, s: kiosk, stock: _kiosk))
                                .Concat(dealer.Items.Where(i => i.HasTag(wish.tag)).Select(i => (i, s: dealer, stock: _dealer)))
                                .OrderBy(o => o.stock.PriceFor(o.i)).ToList();
                            int need = wish.amount, cost = 0;
                            var bought = new List<(ItemData, VendorSupply)>();
                            foreach (var o in offers)
                                while (need > 0 && o.s.Count(o.i) > 0) { o.s.Take(o.i); cost += o.stock.PriceFor(o.i); bought.Add((o.i, o.s)); need--; }
                            if (need == 0 && cost <= Reserve() && cost <= balance)
                            {
                                balance -= cost;
                                spent += cost;
                                run.WishCost += cost;
                                // The rumour is true: buy what will sell higher tomorrow at the dealer.
                                var today = dealer.Items.ToDictionary(i => i, i => _dealer.PriceFor(i));
                                var tomorrow = Entry(day + 1);
                                Market.Begin(ScheduleDay(day + 1), tomorrow.marketEvents);
                                var offerTomorrow = dealer.Items.ToDictionary(i => i, i => _dealer.OfferFor(i));
                                Market.Begin(ScheduleDay(day), entry.marketEvents);
                                foreach (var item in dealer.Items.OrderByDescending(i => offerTomorrow[i] - today[i]))
                                {
                                    if (offerTomorrow[item] <= today[item]) break;
                                    while (dealer.Count(item) > 0 && today[item] <= balance && today[item] <= Reserve() - 30)
                                    {
                                        dealer.Take(item);
                                        balance -= today[item];
                                        run.SpeculationProfit -= today[item];
                                        run.SpeculatedUnits++;
                                        held[item] = held.TryGetValue(item, out int n) ? n + 1 : 1;
                                    }
                                }
                            }
                        }
                    }

                    // Night: wage, penalty, rent – then the needs that are still open, with money at the bed.
                    if (balance + wage - penalty < rent)
                    {
                        evening.GameOver = true;
                        evening.Balance = balance;
                        evening.Family = Enumerable.Range(0, family.Count).Select(family.Condition).ToArray();
                        run.Evenings.Add(evening);
                        break;
                    }
                    balance += wage - penalty - rent;
                    spent += rent;
                    var fromHome = FromHome();
                    var paid = new HashSet<FamilyNeed>();
                    if (style != Style.Skip)
                        foreach (var need in needs)
                        {
                            if (fromHome[(int)need] != null || !Needed(need)) continue;
                            int cost = _household.MoneyCost(need);
                            if (cost > balance) { evening.SkippedNeed = true; continue; }
                            balance -= cost;
                            spent += cost;
                            paid.Add(need);
                        }
                    if (style == Style.Skip) evening.SkippedNeed = true;
                    bool Met(FamilyNeed n) => fromHome[(int)n] != null || paid.Contains(n);
                    family.Settle(Met(FamilyNeed.Food), Met(FamilyNeed.Heat), Met(FamilyNeed.Medicine));
                    // P131: the night uses one item per covered need; perishable leftovers spoil, the rest stays for later nights (like SettlementWindow.Book).
                    home.Night(fromHome, needs.Select(n => _household.TagOf(n)).ToArray(), _economy.perishableTag);

                    evening.Balance = balance;
                    evening.Spent = spent;
                    evening.Family = Enumerable.Range(0, family.Count).Select(family.Condition).ToArray();
                    run.Evenings.Add(evening);
                    if (run.TicketDay < 0 && balance >= _household.ticketPrice && !family.AnySick) run.TicketDay = day;
                }
            }
            finally
            {
                Market.Begin(1, null);
            }
            return run;
        }
    }
}
