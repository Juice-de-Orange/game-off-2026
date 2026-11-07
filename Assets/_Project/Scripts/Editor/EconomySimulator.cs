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
    /// schedule loop over the story days until the pools of P73 exist. Used by the Economy Report and the balance tests.
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
            public bool GameOver => Evenings.Any(e => e.GameOver);
        }

        private readonly EconomyTuning _economy;
        private readonly HouseholdTuning _household;
        private readonly DaySchedule _schedule;
        private readonly VendorStock _kiosk, _dealer;
        private readonly int _freeWarnings;

        public EconomySimulator(EconomyTuning economy, HouseholdTuning household, DaySchedule schedule, VendorStock kiosk, VendorStock dealer,
            int freeWarnings = 2)
        {
            _economy = economy;
            _household = household;
            _schedule = schedule;
            _kiosk = kiosk;
            _dealer = dealer;
            _freeWarnings = freeWarnings;
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
        public static int MeetCharacters(int day, GameOff.Characters.CharacterMemory memory)
        {
            var due = GameOff.Characters.CharacterPlanner.Due(day, Story, memory, 3, () => 0f);
            foreach (var p in due)
            {
                var a = p.character.Appearances[p.appearance];
                var outcome = a.OutcomeOf(p.traveler.IntendedVerdict);
                memory.Remember(p.character, p.appearance, day, outcome);
                foreach (var c in a.consequences)
                    if (c.when == outcome && c.marketEvent != null) Market.Schedule(day + c.eventInDays, c.marketEvent);
            }
            return due.Count;
        }

        /// <summary>Forget the cached characters (after the story builder ran).</summary>
        public static void ResetCharacters() => _story = null;

        private DayEntry Entry(int day) => _schedule.Get((day - 1) % _schedule.Count + 1);
        private int ScheduleDay(int day) => (day - 1) % _schedule.Count + 1;

        /// <summary>Plays `days` days. `wrongPerDay` travelers each day are decided wrongly.</summary>
        public Run Play(Style style, int wrongPerDay, int days)
        {
            var run = new Run { Style = style };
            int balance = _economy.startCredits;
            var family = new Household(_household.members.Select(m => m.feelsCold).ToList());
            var held = new Dictionary<ItemData, int>(); // speculation stock carried to the next evening
            int sulksFrom = 0; // P66: the barkeeper sulks the day a bribe is turned down and the next
            var memory = new GameOff.Characters.CharacterMemory(); // P76: who was met and how it went
            try
            {
                for (int day = 1; day <= days; day++)
                {
                    var entry = Entry(day);
                    Market.Begin(ScheduleDay(day), entry.marketEvents);
                    // P76: the recurring characters come on top of the day's own travelers – on the path of an officer who
                    // decides every one of them right (their outcomes set off tomorrow's events, e.g. Quill arrested → smuggler raid).
                    int travelers = (entry.rules != null ? entry.rules.Travelers.Count : 0) + MeetCharacters(ScheduleDay(day), memory);
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
                    var covered = new HashSet<FamilyNeed>();
                    bool Needed(FamilyNeed n) => n != FamilyNeed.Medicine || family.AnySick;

                    if (style == Style.Clever || style == Style.Rumour || style == Style.Corrupt)
                        foreach (FamilyNeed need in System.Enum.GetValues(typeof(FamilyNeed)))
                        {
                            if (!Needed(need)) continue;
                            var tag = _household.TagOf(need);
                            var cheapest = kiosk.Items.Where(i => i.HasTag(tag) && kiosk.Count(i) > 0)
                                .OrderBy(i => _kiosk.PriceFor(i)).FirstOrDefault();
                            if (cheapest == null) continue;
                            int price = _kiosk.PriceFor(cheapest);
                            if (price > balance || price > Reserve()) continue;
                            kiosk.Take(cheapest);
                            balance -= price;
                            spent += price;
                            covered.Add(need);
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
                    if (style != Style.Skip)
                        foreach (FamilyNeed need in System.Enum.GetValues(typeof(FamilyNeed)))
                        {
                            if (covered.Contains(need) || !Needed(need)) continue;
                            int cost = _household.MoneyCost(need);
                            if (cost > balance) { evening.SkippedNeed = true; continue; }
                            balance -= cost;
                            spent += cost;
                            covered.Add(need);
                        }
                    if (style == Style.Skip) evening.SkippedNeed = true;
                    family.Settle(covered.Contains(FamilyNeed.Food), covered.Contains(FamilyNeed.Heat), covered.Contains(FamilyNeed.Medicine));

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
