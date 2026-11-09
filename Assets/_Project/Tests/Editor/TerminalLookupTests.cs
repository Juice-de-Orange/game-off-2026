using System.Collections.Generic;
using System.Linq;
using GameOff.Documents;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>
    /// P34: the counter terminal (TerminalLookup) exposes wanted travelers by name or ID number, reads the entry record from the papers,
    /// and agrees with RuleEvaluator's wanted check on every day in the project.
    /// </summary>
    public class TerminalLookupTests
    {
        private const string Today = "2290-11-05";
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void CleanUp()
        {
            foreach (var o in _created) Object.DestroyImmediate(o);
            _created.Clear();
        }

        private DocumentData Doc(DocumentType type, params (string key, string value)[] fields)
        {
            var list = new List<DocumentField>();
            foreach (var (k, v) in fields) list.Add(new DocumentField(k, v));
            var d = ScriptableObject.CreateInstance<DocumentData>();
            d.EditorSet(type, null, list, false);
            _created.Add(d);
            return d;
        }

        private DocumentData Id(string name = "VESNA OKARU", string id = "KX-1", string expires = "2291-01-01") =>
            Doc(DocumentType.IdCard, ("name", name), ("species", "HUMAN"), ("origin", "TORVIK"), ("id", id), ("expires", expires));

        private DocumentData Permit(string name = "VESNA OKARU", string id = "KX-1", string expires = "2290-12-01") =>
            Doc(DocumentType.EntryPermit, ("name", name), ("id", id), ("purpose", "WORK"), ("expires", expires), ("route", "TORVIK > KESTREL-9"));

        private TravelerData Traveler(Verdict intended, params DocumentData[] docs)
        {
            var t = ScriptableObject.CreateInstance<TravelerData>();
            t.EditorSet("Test", "Human", new List<DocumentData>(docs), intended, "");
            t.name = "T_Test";
            _created.Add(t);
            return t;
        }

        private DayRules Day(List<WantedEntry> wanted, params TravelerData[] travelers)
        {
            var d = ScriptableObject.CreateInstance<DayRules>();
            d.EditorSet(Today, new List<DayRule>(), wanted ?? new List<WantedEntry>(), new List<TravelerData>(travelers));
            d.name = "Day_Test";
            _created.Add(d);
            return d;
        }

        private static TerminalQuery Query(TravelerData t, TerminalQuery.Kind kind) =>
            TerminalLookup.Queries(t).First(q => q.Field == kind);

        [Test]
        public void WantedTraveler_IsExposedByName()
        {
            var t = Traveler(Verdict.Arrest, Id(), Permit());
            var day = Day(new List<WantedEntry> { new WantedEntry("VESNA OKARU", "", WantedOrder.Arrest, "Smuggling") }, t);

            var result = TerminalLookup.Check(Query(t, TerminalQuery.Kind.Name), t, day);

            Assert.IsTrue(result.IsWanted);
            Assert.AreEqual(WantedOrder.Arrest, result.Wanted[0].order);
            StringAssert.Contains("MATCH", TerminalLookup.Render(result));
            StringAssert.Contains("ARREST", TerminalLookup.Render(result));
            Assert.AreEqual(Verdict.Arrest, RuleEvaluator.Evaluate(t, day).Correct, "the terminal and the rules agree");
        }

        [Test]
        public void WantedTraveler_UnderFalseName_IsExposedById()
        {
            var t = Traveler(Verdict.Airlock, Id(name: "IRA BELL"), Permit(name: "IRA BELL"));
            var day = Day(new List<WantedEntry> { new WantedEntry("SOMEONE ELSE", "kx-1 ", WantedOrder.Terminate) }, t);

            Assert.IsFalse(TerminalLookup.Check(Query(t, TerminalQuery.Kind.Name), t, day).IsWanted, "the false name is clean");
            var byId = TerminalLookup.Check(Query(t, TerminalQuery.Kind.Id), t, day);
            Assert.IsTrue(byId.IsWanted, "the ID number gives them away (case and spaces ignored)");
            StringAssert.Contains("TERMINATE", TerminalLookup.Render(byId));
            Assert.AreEqual(Verdict.Airlock, RuleEvaluator.Evaluate(t, day).Correct);
        }

        [Test]
        public void CleanTraveler_HasNoMatch_AndARecordPerPaper()
        {
            var t = Traveler(Verdict.Admit, Id(), Permit());
            var day = Day(new List<WantedEntry> { new WantedEntry("SOMEONE ELSE", "ZZ-9", WantedOrder.Arrest) }, t);

            var result = TerminalLookup.Check(Query(t, TerminalQuery.Kind.Name), t, day);

            Assert.IsFalse(result.IsWanted);
            Assert.AreEqual(2, result.Record.Count, "ID card and entry permit carry the name");
            StringAssert.Contains("NO MATCH", TerminalLookup.Render(result));
        }

        [Test]
        public void Queries_ListEachNameAndIdOnce()
        {
            var t = Traveler(Verdict.Deny, Id(), Permit(name: "vesna okaru ", id: "KX-2"));
            var queries = TerminalLookup.Queries(t);
            Assert.AreEqual(1, queries.Count(q => q.Field == TerminalQuery.Kind.Name), "same name, case and spaces ignored");
            Assert.AreEqual(2, queries.Count(q => q.Field == TerminalQuery.Kind.Id), "two different ID numbers");
            Assert.AreEqual(TerminalQuery.Kind.Name, queries[0].Field, "names first");
        }

        [Test]
        public void Record_MarksExpiredPapers()
        {
            var t = Traveler(Verdict.Deny, Id(), Permit(expires: "2290-11-01"));
            var result = TerminalLookup.Check(Query(t, TerminalQuery.Kind.Id), t, Day(null, t));
            Assert.AreEqual(2, result.Record.Count);
            Assert.IsFalse(result.Record[0].Contains("EXPIRED"), "the ID card is valid");
            StringAssert.Contains("EXPIRED", result.Record[1]);
        }

        [Test]
        public void StatusLine_ShowsLaneProgressAndClock()
        {
            Assert.AreEqual("LANE 1 CLOSED · 0/6 · 08:00", TerminalLookup.StatusLine(TerminalLookup.Lane.Closed, false, 0, 6, "08:00"));
            StringAssert.Contains("INSPECTION", TerminalLookup.StatusLine(TerminalLookup.Lane.Open, true, 2, 6, "09:10"));
            StringAssert.Contains("SHIFT OVER", TerminalLookup.StatusLine(TerminalLookup.Lane.Over, false, 6, 6, "17:00"));
        }

        /// <summary>Every traveler in the project: the terminal finds exactly the wanted entries the rules act on.</summary>
        [Test]
        public void ProjectContent_TerminalAgreesWithRuleEvaluator()
        {
            var problems = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:DayRules", new[] { "Assets/_Project/Data" }))
            {
                var day = AssetDatabase.LoadAssetAtPath<DayRules>(AssetDatabase.GUIDToAssetPath(guid));
                if (day == null) continue;
                foreach (var t in day.Travelers)
                {
                    if (t == null) continue;
                    var results = TerminalLookup.Queries(t).Select(q => TerminalLookup.Check(q, t, day)).ToList();
                    // Per entry of the list (like RuleEvaluator), so two equal entries count twice on both sides.
                    int hits = day.Wanted.Count(w => results.Any(r => r.Wanted.Contains(w)));
                    int ruled = RuleEvaluator.Evaluate(t, day).Violations.Count(v => v.Reason.StartsWith("Wanted"));
                    if (hits != ruled) problems.Add($"{day.name}/{t.name}: terminal {hits} wanted hits, rules {ruled}");
                }
            }
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }
    }
}
