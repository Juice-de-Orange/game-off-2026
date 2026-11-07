using System.Collections.Generic;
using GameOff.Characters;
using GameOff.Economy;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P75: conditions, windows, queue slots, memory and the consequences of recurring characters.</summary>
    public class CharacterTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void CleanUp()
        {
            foreach (var o in _created) Object.DestroyImmediate(o);
            _created.Clear();
            Market.Begin(1, null);
        }

        private T Make<T>(string name = "x") where T : ScriptableObject
        {
            var o = ScriptableObject.CreateInstance<T>();
            o.name = name;
            _created.Add(o);
            return o;
        }

        private TravelerData Traveler(string name) => Make<TravelerData>(name);

        /// <summary>The acceptance character: day 1 anywhere; day 2 third in the queue only if helped; else wanted on day 3.</summary>
        private RecurringCharacter TestCharacter(out TravelerData a, out TravelerData b, out TravelerData c, MarketEvent e)
        {
            a = Traveler("A"); b = Traveler("B"); c = Traveler("C");
            var ch = Make<RecurringCharacter>("Char_Test");
            ch.EditorSet("test", "Test Person", "Human", new List<Appearance>
            {
                new Appearance
                {
                    traveler = a, dayFrom = 1, dayTo = 1, slot = 0, condition = AppearanceCondition.NeverMet,
                    consequences = new List<Consequence>
                    {
                        new Consequence { when = Outcome.Betrayed, wanted = true, wantedOrder = WantedOrder.Arrest, wantedInDays = 2 },
                        new Consequence { when = Outcome.Betrayed, marketEvent = e, eventInDays = 1 },
                    },
                },
                new Appearance { traveler = b, dayFrom = 2, dayTo = 2, slot = 3, condition = AppearanceCondition.LastHelped, greetingAfterHelped = "Thanks again!" },
                new Appearance { traveler = c, dayFrom = 3, dayTo = 3, slot = 1, condition = AppearanceCondition.LastWronged },
            });
            return ch;
        }

        private static List<TravelerData> Regular(params TravelerData[] t) => new List<TravelerData>(t);

        [Test]
        public void HelpedOnDay1_ComesThirdOnDay2()
        {
            var ch = TestCharacter(out var a, out var b, out _, null);
            var memory = new CharacterMemory();
            var chars = new List<RecurringCharacter> { ch };
            var day1 = CharacterPlanner.Due(1, chars, memory, 3, () => 0f);
            Assert.AreEqual(1, day1.Count);
            Assert.AreSame(a, day1[0].traveler);
            memory.Remember(ch, 0, 1, Outcome.Helped);

            var day2 = CharacterPlanner.Due(2, chars, memory, 3, () => 0f);
            Assert.AreEqual(1, day2.Count);
            Assert.AreSame(b, day2[0].traveler);
            var r = Regular(Traveler("r1"), Traveler("r2"), Traveler("r3"), Traveler("r4"));
            var queue = CharacterPlanner.Queue(r, day2, () => 0f);
            Assert.AreSame(b, queue[2], "slot 3 = third in the queue");
            Assert.AreEqual(5, queue.Count);

            Assert.IsEmpty(CharacterPlanner.Due(3, chars, memory, 3, () => 0f), "helped: no revenge on day 3");
        }

        [Test]
        public void BetrayedOnDay1_SkipsDay2_AndComesBackOnDay3()
        {
            var ch = TestCharacter(out _, out _, out var c, null);
            var memory = new CharacterMemory();
            var chars = new List<RecurringCharacter> { ch };
            memory.Remember(ch, 0, 1, Outcome.Betrayed);
            Assert.IsEmpty(CharacterPlanner.Due(2, chars, memory, 3, () => 0f));
            var day3 = CharacterPlanner.Due(3, chars, memory, 3, () => 0f);
            Assert.AreEqual(1, day3.Count);
            Assert.AreSame(c, day3[0].traveler);
        }

        [Test]
        public void TheDead_NeverComeBack()
        {
            var ch = TestCharacter(out _, out _, out _, null);
            var memory = new CharacterMemory();
            memory.Remember(ch, 0, 1, Outcome.Dead);
            Assert.IsFalse(memory.Alive(ch));
            for (int day = 2; day <= 3; day++) Assert.IsEmpty(CharacterPlanner.Due(day, new List<RecurringCharacter> { ch }, memory, 3, () => 0f));
            Assert.AreEqual(Outcome.Dead, ch.Appearances[0].OutcomeOf(Verdict.Airlock), "the airlock is always final");
        }

        [Test]
        public void Window_PicksItsDay_AndAlwaysComesOnTheLastDay()
        {
            var t = Traveler("w");
            var ch = Make<RecurringCharacter>();
            ch.EditorSet("w", "W", "Human", new List<Appearance> { new Appearance { traveler = t, dayFrom = 5, dayTo = 7 } });
            var memory = new CharacterMemory();
            var chars = new List<RecurringCharacter> { ch };
            Assert.IsEmpty(CharacterPlanner.Due(4, chars, memory, 3, () => 0f), "before the window");
            Assert.IsEmpty(CharacterPlanner.Due(5, chars, memory, 3, () => 0.9f), "1/3 chance on the first day");
            Assert.AreEqual(1, CharacterPlanner.Due(5, chars, memory, 3, () => 0.1f).Count);
            Assert.AreEqual(1, CharacterPlanner.Due(7, chars, memory, 3, () => 0.99f).Count, "last day: always");
            memory.Remember(ch, 0, 6, Outcome.Neutral);
            Assert.IsEmpty(CharacterPlanner.Due(7, chars, memory, 3, () => 0f), "once only");
        }

        [Test]
        public void AtMostMaxPerDay_FixedSlotsFirst()
        {
            var chars = new List<RecurringCharacter>();
            for (int i = 0; i < 4; i++)
            {
                var ch = Make<RecurringCharacter>();
                ch.EditorSet("c" + i, "C" + i, "Human", new List<Appearance> { new Appearance { traveler = Traveler("t" + i), slot = i == 3 ? 1 : 0 } });
                chars.Add(ch);
            }
            var due = CharacterPlanner.Due(1, chars, new CharacterMemory(), 3, () => 0f);
            Assert.AreEqual(3, due.Count);
            Assert.AreEqual("C3", due[0].character.DisplayName, "the fixed slot stays");
        }

        [Test]
        public void Consequences_WantedForItsDays_AndScheduledEventsJoinTheMarket()
        {
            var ch = TestCharacter(out _, out _, out _, null);
            var memory = new CharacterMemory();
            memory.ScheduleWanted(ch, new WantedEntry("Test Person", "", WantedOrder.Arrest), 3, 1);
            Assert.IsEmpty(memory.WantedOn(2));
            Assert.AreEqual(1, memory.WantedOn(3).Count);
            Assert.IsEmpty(memory.WantedOn(4));

            var e = Make<MarketEvent>("Market_Test");
            Market.Begin(1, null);
            Market.Schedule(2, e);
            Market.Begin(2, null);
            CollectionAssert.Contains(Market.TodayEvents, e);
            Market.Begin(1, null);
            Market.Begin(2, null);
            CollectionAssert.DoesNotContain(Market.TodayEvents, e, "a new run forgets the old schedule");
        }

        /// <summary>
        /// Validator (P75): every appearance of every character in the project is judged right by the rules on its first day –
        /// with the wanted entries that the earlier appearances can put on that day when the condition says how it went.
        /// </summary>
        [Test]
        public void ProjectCharacters_EveryAppearanceIsJudgedAsIntended()
        {
            var schedule = UnityEditor.AssetDatabase.LoadAssetAtPath<GameOff.Day.DaySchedule>("Assets/_Project/Data/Days/DaySchedule.asset");
            Assert.NotNull(schedule);
            foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t:RecurringCharacter"))
            {
                var ch = UnityEditor.AssetDatabase.LoadAssetAtPath<RecurringCharacter>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
                for (int i = 0; i < ch.Appearances.Count; i++)
                {
                    var a = ch.Appearances[i];
                    Assert.NotNull(a.traveler, $"{ch.name} appearance {i + 1}: no traveler");
                    Assert.LessOrEqual(a.dayFrom, Mathf.Max(a.dayFrom, a.dayTo));
                    var rules = schedule.Get(a.dayFrom).rules;
                    var wanted = new List<WantedEntry>();
                    for (int j = 0; j < i; j++)
                        foreach (var c in ch.Appearances[j].consequences)
                        {
                            int on = ch.Appearances[j].dayFrom + c.wantedInDays;
                            bool lands = c.wanted && a.dayFrom >= on && a.dayFrom < on + c.wantedForDays;
                            bool fits = a.condition == AppearanceCondition.Always || a.condition == AppearanceCondition.Alive
                                || (a.condition == AppearanceCondition.LastBetrayed && c.when == Outcome.Betrayed)
                                || (a.condition == AppearanceCondition.LastConfiscated && c.when == Outcome.Confiscated)
                                || (a.condition == AppearanceCondition.LastWronged && (c.when == Outcome.Betrayed || c.when == Outcome.Confiscated))
                                || (a.condition == AppearanceCondition.LastHelped && c.when == Outcome.Helped);
                            if (lands && fits) wanted.Add(new WantedEntry(ch.DisplayName, "", c.wantedOrder, c.wantedNote));
                        }
                    var day = rules.RuntimeCopy(null, wanted);
                    try
                    {
                        var result = RuleEvaluator.Evaluate(a.traveler, day);
                        Assert.AreEqual(a.traveler.IntendedVerdict, result.Correct,
                            $"{ch.name} appearance {i + 1} (day {a.dayFrom}): rules say {result.Correct} ({result.MainReason}), intended {a.traveler.IntendedVerdict}");
                    }
                    finally { Object.DestroyImmediate(day); }
                }
            }
        }

        [Test]
        public void RuntimeCopy_LeavesTheAssetAlone()
        {
            var day = Make<DayRules>("Day_Test");
            var r1 = Traveler("r1");
            day.EditorSet("2290-11-05", new List<DayRule>(), new List<WantedEntry>(), new List<TravelerData> { r1 });
            var extra = Traveler("extra");
            var copy = day.RuntimeCopy(new List<TravelerData> { extra, r1 }, new List<WantedEntry> { new WantedEntry("X", "", WantedOrder.Arrest) });
            _created.Add(copy);
            Assert.AreEqual(2, copy.Travelers.Count);
            Assert.AreEqual(1, copy.Wanted.Count);
            Assert.AreEqual(1, day.Travelers.Count);
            Assert.AreEqual(0, day.Wanted.Count);
            Assert.AreEqual(day.Date, copy.Date);
        }
    }
}
