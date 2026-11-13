using System.Collections.Generic;
using GameOff.Documents;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>
    /// The counter rules (DESIGN.md "Regeln am Schalter") as tests: each error type, the priority, the judgement table,
    /// and all content in the project through the validator. Run: Window/General/Test Runner (EditMode) or the unity CLI.
    /// </summary>
    public class RuleEvaluatorTests
    {
        private const string Today = "2290-11-05";
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void CleanUp()
        {
            foreach (var o in _created) Object.DestroyImmediate(o);
            _created.Clear();
        }

        // ---------- helpers: a clean traveler that each test breaks in one way ----------

        private DocumentData Doc(DocumentType type, bool forged = false, params (string key, string value)[] fields)
        {
            var list = new List<DocumentField>();
            foreach (var (k, v) in fields) list.Add(new DocumentField(k, v));
            var d = ScriptableObject.CreateInstance<DocumentData>();
            d.EditorSet(type, null, list, forged);
            _created.Add(d);
            return d;
        }

        private DocumentData Id(string name = "VESNA OKARU", string id = "KX-1", string origin = "TORVIK", string expires = "2291-01-01") =>
            Doc(DocumentType.IdCard, false, ("name", name), ("species", "HUMAN"), ("origin", origin), ("id", id), ("expires", expires));

        private DocumentData Permit(string name = "VESNA OKARU", string id = "KX-1", string expires = "2290-12-01",
            string route = "TORVIK > ASKA-3 > KESTREL-9", bool forged = false) =>
            Doc(DocumentType.EntryPermit, forged, ("name", name), ("id", id), ("purpose", "WORK"), ("duration", "2 CYCLES"),
                ("expires", expires), ("issued", "ASKA-3"), ("route", route));

        private DocumentData Manifest(string cargo = "4x WATER FILTER") =>
            Doc(DocumentType.CargoManifest, false, ("name", "VESNA OKARU"), ("ship", "MV HALDE"), ("origin", "TORVIK"),
                ("destination", "KESTREL-9"), ("cargo", cargo));

        private TravelerData Traveler(Verdict intended, params DocumentData[] docs)
        {
            var t = ScriptableObject.CreateInstance<TravelerData>();
            t.EditorSet("Test", "Human", new List<DocumentData>(docs), intended, "");
            t.name = "T_Test";
            _created.Add(t);
            return t;
        }

        [Test]
        public void Validator_RejectsForgedSealsBeforeTheLampIsOnSale()
        {
            var early = Day(null, null, Traveler(Verdict.Arrest, Id(), Permit(forged: true)));
            early.EditorSet("2290-11-08", new List<DayRule>(), new List<WantedEntry>(), new List<TravelerData>(early.Travelers));
            Assert.That(TravelerValidator.Validate(early), Has.Some.Contains("forged seal"));
        }

        private DayRules Day(List<DayRule> rules = null, List<WantedEntry> wanted = null, params TravelerData[] travelers)
        {
            var d = ScriptableObject.CreateInstance<DayRules>();
            d.EditorSet(Today, rules ?? new List<DayRule>(), wanted ?? new List<WantedEntry>(), new List<TravelerData>(travelers));
            d.name = "Day_Test";
            _created.Add(d);
            return d;
        }

        private Verdict Eval(TravelerData t, DayRules day) => RuleEvaluator.Evaluate(t, day).Correct;

        // ---------- base rules ----------

        [Test] public void CleanTraveler_IsAdmitted() => Assert.AreEqual(Verdict.Admit, Eval(Traveler(Verdict.Admit, Id(), Permit(), Manifest()), Day()));

        [Test] public void NameMismatch_IsDenied() =>
            Assert.AreEqual(Verdict.Deny, Eval(Traveler(Verdict.Deny, Id(), Permit(name: "VESNA OKAROO")), Day()));

        [Test] public void IdNumberMismatch_IsDenied() =>
            Assert.AreEqual(Verdict.Deny, Eval(Traveler(Verdict.Deny, Id(), Permit(id: "KX-2")), Day()));

        [Test] public void NameComparison_IgnoresCaseAndSpaces() =>
            Assert.AreEqual(Verdict.Admit, Eval(Traveler(Verdict.Admit, Id(), Permit(name: " vesna okaru ")), Day()));

        [Test] public void ExpiredPermit_IsDenied() =>
            Assert.AreEqual(Verdict.Deny, Eval(Traveler(Verdict.Deny, Id(), Permit(expires: "2290-11-04")), Day()));

        [Test] public void ExpiringToday_IsStillValid() =>
            Assert.AreEqual(Verdict.Admit, Eval(Traveler(Verdict.Admit, Id(), Permit(expires: Today)), Day()));

        [Test] public void MissingPermit_IsDenied() => Assert.AreEqual(Verdict.Deny, Eval(Traveler(Verdict.Deny, Id()), Day()));

        [Test] public void MissingId_IsDenied() => Assert.AreEqual(Verdict.Deny, Eval(Traveler(Verdict.Deny, Permit()), Day()));

        [Test] public void ForgedSeal_IsArrested() =>
            Assert.AreEqual(Verdict.Arrest, Eval(Traveler(Verdict.Arrest, Id(), Permit(forged: true)), Day()));

        // ---------- day rules ----------

        [Test] public void BannedOrigin_IsDenied() =>
            Assert.AreEqual(Verdict.Deny, Eval(Traveler(Verdict.Deny, Id(origin: "ORVAN"), Permit()), Day(new List<DayRule> { new BannedOriginRule("Orvan") })));

        [Test] public void BannedOrigin_DoesNotHitSimilarNames() =>
            Assert.AreEqual(Verdict.Admit, Eval(Traveler(Verdict.Admit, Id(origin: "TORVAN"), Permit()), Day(new List<DayRule> { new BannedOriginRule("Orvan") })));

        [Test] public void RequiredDocument_Missing_IsDenied() =>
            Assert.AreEqual(Verdict.Deny, Eval(Traveler(Verdict.Deny, Id(), Permit()), Day(new List<DayRule> { new RequireDocumentRule(DocumentType.CargoManifest) })));

        [Test] public void RequiredDocument_OnlyForOtherSpecies_IsIgnored() =>
            Assert.AreEqual(Verdict.Admit, Eval(Traveler(Verdict.Admit, Id(), Permit()), Day(new List<DayRule> { new RequireDocumentRule(DocumentType.CargoManifest, "Quobb") })));

        [Test] public void ForbiddenGoods_AreArrested() =>
            Assert.AreEqual(Verdict.Arrest, Eval(Traveler(Verdict.Arrest, Id(), Permit(), Manifest("6x CHEM STIMULANT VIALS")),
                Day(new List<DayRule> { new ForbiddenGoodsRule("stimulant") })));

        [Test] public void ForbiddenGoods_MatchWholeWordsOnly() =>
            Assert.AreEqual(Verdict.Admit, Eval(Traveler(Verdict.Admit, Id(), Permit(), Manifest("2x FARMSTUFF")),
                Day(new List<DayRule> { new ForbiddenGoodsRule("ARMS") })));

        [Test] public void Quarantine_RequiresAirlock() =>
            Assert.AreEqual(Verdict.Airlock, Eval(Traveler(Verdict.Airlock, Id(), Permit(route: "ORVA > KETRA STATION > KESTREL-9")),
                Day(new List<DayRule> { new QuarantineRule("Ketra Station") })));

        [Test] public void Quarantine_NeedsExactStop() =>
            Assert.AreEqual(Verdict.Admit, Eval(Traveler(Verdict.Admit, Id(), Permit(route: "ORVA > KETRA STATION NORTH > KESTREL-9")),
                Day(new List<DayRule> { new QuarantineRule("Ketra Station") })));

        [Test] public void Wanted_Arrest() =>
            Assert.AreEqual(Verdict.Arrest, Eval(Traveler(Verdict.Arrest, Id(), Permit()),
                Day(wanted: new List<WantedEntry> { new WantedEntry("VESNA OKARU", "", WantedOrder.Arrest) })));

        [Test] public void Wanted_TerminateMatchesById() =>
            Assert.AreEqual(Verdict.Airlock, Eval(Traveler(Verdict.Airlock, Id(), Permit()),
                Day(wanted: new List<WantedEntry> { new WantedEntry("SOMEONE ELSE", "KX-1", WantedOrder.Terminate) })));

        // ---------- priority: mandatory airlock > arrest > deny > admit ----------

        [Test]
        public void Priority_MostSevereDecides()
        {
            var day = Day(new List<DayRule> { new ForbiddenGoodsRule("STIMULANT"), new QuarantineRule("KETRA STATION") });
            var t = Traveler(Verdict.Airlock, Id(), Permit(expires: "2290-01-01", route: "KETRA STATION > KESTREL-9"), Manifest("1x STIMULANT"));
            var r = RuleEvaluator.Evaluate(t, day);
            Assert.AreEqual(Verdict.Airlock, r.Correct);
            Assert.AreEqual(3, r.Violations.Count);
            Assert.AreEqual(Verdict.Airlock, r.Violations[0].Requires);
            Assert.AreEqual(Verdict.Arrest, r.Violations[1].Requires);
            Assert.AreEqual(Verdict.Deny, r.Violations[2].Requires);
        }

        [Test]
        public void Priority_ArrestBeatsDeny()
        {
            var t = Traveler(Verdict.Arrest, Id(), Permit(name: "OTHER", forged: true));
            Assert.AreEqual(Verdict.Arrest, Eval(t, Day()));
        }

        // ---------- judgement table ----------

        private static EvaluationResult Result(Verdict correct) =>
            new EvaluationResult(correct, correct == Verdict.Admit ? new List<Violation>() : new List<Violation> { new Violation(correct, "reason") });

        [TestCase(Verdict.Admit), TestCase(Verdict.Deny), TestCase(Verdict.Arrest), TestCase(Verdict.Airlock)]
        public void Judge_SameDecision_IsCorrect(Verdict v) => Assert.IsTrue(DecisionJudge.Judge(Result(v), v).Correct);

        [Test]
        public void Judge_AirlockForCriminal_IsCorrectWithMoraleCost()
        {
            var j = DecisionJudge.Judge(Result(Verdict.Arrest), Verdict.Airlock);
            Assert.IsTrue(j.Correct);
            Assert.IsTrue(j.MoraleCost);
        }

        [Test]
        public void Judge_ArrestWhenAirlockOrdered_IsDisobeyed()
        {
            var j = DecisionJudge.Judge(Result(Verdict.Airlock), Verdict.Arrest);
            Assert.IsFalse(j.Correct);
            Assert.IsTrue(j.Disobeyed);
            StringAssert.Contains("Order disobeyed", j.Reason);
        }

        [Test]
        public void Judge_OrderedAirlock_StillCostsMorale() => Assert.IsTrue(DecisionJudge.Judge(Result(Verdict.Airlock), Verdict.Airlock).MoraleCost);

        [TestCase(Verdict.Admit, Verdict.Deny, "Valid traveler denied")]
        [TestCase(Verdict.Admit, Verdict.Arrest, "Innocent traveler arrested")]
        [TestCase(Verdict.Admit, Verdict.Airlock, "Innocent traveler airlocked")]
        [TestCase(Verdict.Deny, Verdict.Admit, "Admitted")]
        [TestCase(Verdict.Deny, Verdict.Arrest, "Wrongful arrest")]
        [TestCase(Verdict.Arrest, Verdict.Admit, "Criminal released")]
        [TestCase(Verdict.Arrest, Verdict.Deny, "Criminal released")]
        [TestCase(Verdict.Airlock, Verdict.Admit, "Ordered airlock ignored")]
        public void Judge_WrongDecision_HasReason(Verdict correct, Verdict chosen, string reasonStart)
        {
            var j = DecisionJudge.Judge(Result(correct), chosen);
            Assert.IsFalse(j.Correct);
            StringAssert.StartsWith(reasonStart, j.Reason);
        }

        // ---------- the validator itself ----------

        [Test]
        public void Validator_FlagsWrongIntention()
        {
            var t = Traveler(Verdict.Admit, Id(), Permit(expires: "2290-01-01")); // expired, but marked Admit
            var problems = TravelerValidator.Validate(Day(null, null, t));
            Assert.IsTrue(problems.Exists(p => p.Contains("rules say Deny")), string.Join("\n", problems));
        }

        [Test]
        public void Validator_FlagsNearMiss()
        {
            var t = Traveler(Verdict.Admit, Id(), Permit(), Manifest("3x STIMULANTS"));
            var problems = TravelerValidator.Validate(Day(new List<DayRule> { new ForbiddenGoodsRule("STIMULANT") }, null, t));
            Assert.IsTrue(problems.Exists(p => p.Contains("nearly matches")), string.Join("\n", problems));
        }

        [Test]
        public void Validator_AcceptsCleanDay() =>
            CollectionAssert.IsEmpty(TravelerValidator.Validate(Day(null, null, Traveler(Verdict.Admit, Id(), Permit(), Manifest()))));

        // ---------- all real content ----------

        [Test]
        public void DocumentPrefabs_ShowEveryRequiredField()
        {
            var problems = GameOff.EditorTools.TravelerValidatorMenu.ValidatePrefabFields();
            CollectionAssert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void AllDaysInProject_AreConsistent()
        {
            var problems = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:DayRules", new[] { "Assets/_Project/Data" }))
                problems.AddRange(TravelerValidator.Validate(AssetDatabase.LoadAssetAtPath<DayRules>(AssetDatabase.GUIDToAssetPath(guid))));
            CollectionAssert.IsEmpty(problems, string.Join("\n", problems));
        }
    }
}
