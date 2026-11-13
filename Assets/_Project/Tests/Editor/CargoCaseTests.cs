using System.Linq;
using GameOff.Economy;
using GameOff.Hangar;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P35: a reported crate makes an arrest right without making anything else wrong; the shipped cases fit their days.</summary>
    public class CargoCaseTests
    {
        private static EvaluationResult Admit() => new EvaluationResult(Verdict.Admit, new System.Collections.Generic.List<Violation>());

        [Test]
        public void Arrest_WithEvidence_IsRight_AndAdmitStaysRight()
        {
            var withEvidence = DecisionJudge.Judge(Admit(), Verdict.Arrest, true);
            Assert.IsTrue(withEvidence.Correct);
            Assert.AreEqual(Verdict.Arrest, withEvidence.CorrectVerdict);
            Assert.IsFalse(withEvidence.MoraleCost);
            Assert.IsTrue(DecisionJudge.Judge(Admit(), Verdict.Admit, true).Correct, "the papers are still in order");
        }

        [Test]
        public void Arrest_WithoutEvidence_StaysWrong()
        {
            Assert.IsFalse(DecisionJudge.Judge(Admit(), Verdict.Arrest).Correct);
            Assert.IsFalse(DecisionJudge.Judge(Admit(), Verdict.Arrest, false).Correct);
        }

        [Test]
        public void Evidence_DoesNotExcuseOtherDecisions()
        {
            Assert.IsFalse(DecisionJudge.Judge(Admit(), Verdict.Deny, true).Correct);
            Assert.IsFalse(DecisionJudge.Judge(Admit(), Verdict.Airlock, true).Correct);
        }

        [Test]
        public void Report_IsPossibleFromTheLetterUntilTheCounterOpens()
        {
            Assert.IsTrue(CargoCrate.CanReport(GameOff.Day.DayManager.Phase.Shift, false), "phase Shift starts when the letter closes");
            Assert.IsFalse(CargoCrate.CanReport(GameOff.Day.DayManager.Phase.Shift, true), "the counter is open");
            Assert.IsFalse(CargoCrate.CanReport(GameOff.Day.DayManager.Phase.Evening, false));
        }

        [Test]
        public void ClosingLine_AfterTheftOnly_IsNotAboutTheAirlock()
        {
            var texts = ScriptableObject.CreateInstance<GameOff.Departure.DepartureTuning>();
            var conduct = new Conduct();
            StringAssert.AreEqualIgnoringCase(GameOff.Departure.TicketEnding.AirlockLine(0, 0, texts), GameOff.Departure.TicketEnding.ConductLine(conduct, texts), "a clean run");
            conduct.AddMorale(-1);
            Assert.AreEqual(texts.moraleThief, GameOff.Departure.TicketEnding.ConductLine(conduct, texts));
            Object.DestroyImmediate(texts);
        }

        [Test]
        public void Pocketing_LowersMorale_ButIsNoAirlock()
        {
            var conduct = new Conduct();
            var morale = ScriptableObject.CreateInstance<MoraleTuning>();
            conduct.AddMorale(morale.lingeringAtOrBelow - 1);
            Assert.AreEqual(morale.lingeringAtOrBelow - 1, conduct.Morale);
            Assert.IsNull(morale.Thought(conduct, 2), "the thoughts about the airlock need an airlock");
            Object.DestroyImmediate(morale);
        }

        /// <summary>Every shipped case names a traveler of that story day who is meant to pass, and has goods.</summary>
        [Test]
        public void ProjectContent_CasesFitTheirDays()
        {
            var tuning = AssetDatabase.LoadAssetAtPath<CargoCaseTuning>("Assets/_Project/Data/Economy/CargoCases.asset");
            Assert.IsNotNull(tuning, "run GameOff/Rebuild All After Deliveries");
            Assert.IsNotEmpty(tuning.cases);
            foreach (var c in tuning.cases)
            {
                var rules = AssetDatabase.LoadAssetAtPath<DayRules>($"Assets/_Project/Data/Days/Story/Day{c.day:00}.asset");
                Assert.IsNotNull(rules, $"day {c.day}");
                var traveler = rules.Travelers.FirstOrDefault(t => t != null && t.DisplayName == c.traveler);
                Assert.IsNotNull(traveler, $"{c.traveler} is not on day {c.day}");
                Assert.AreEqual(Verdict.Admit, RuleEvaluator.Evaluate(traveler, rules).Correct, $"{c.traveler}: the papers must stay in order");
                Assert.IsNotNull(c.item, $"{c.traveler}: no goods");
                Assert.Greater(c.count, 0);
            }
        }
    }
}
