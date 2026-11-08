using System.Collections.Generic;
using GameOff.Characters;
using GameOff.Day;
using GameOff.Documents;
using GameOff.Economy;
using GameOff.Travelers;
using UnityEditor;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// P76: the eight recurring characters (docs/FIGUREN.md) and the 12 story-day skeletons they need.
    /// Days: Data/Days/Story/Day01–Day12 (date 2290-11-05 + day − 1, base rules of the test day, three test travelers as placeholders;
    /// Max fills them in P15). DaySchedule gets 12 entries; existing letters, bar wishes and bribes stay, the fixed market events follow
    /// FIGUREN.md (border day 2, cold day 3, inspection day 9 – the smuggler raid is now set off by Quill).
    /// Characters: Data/Characters/Story/, their travelers and papers in Data/Travelers/Story/. Generated content – change it here.
    /// </summary>
    public static class StoryCharacterBuilder
    {
        private const string DayFolder = "Assets/_Project/Data/Days/Story/";
        private const string CharFolder = "Assets/_Project/Data/Characters/Story/";
        private const string TravelerFolder = "Assets/_Project/Data/Travelers/Story/";
        private const string DocFolder = "Assets/_Project/Data/Documents/Story/";
        private const string PhotoFolder = "Assets/_Project/Art/Textures/Documents/";
        private const string MarketFolder = "Assets/_Project/Data/Market/";
        private const string SchedulePath = "Assets/_Project/Data/Days/DaySchedule.asset";
        private const string TestTravelers = "Assets/_Project/Data/Travelers/Test/";
        public const int StoryDays = 12;
        private const string NormalTravelerFolder = "Assets/_Project/Data/Travelers/Normal/";
        private const string NormalDocFolder = "Assets/_Project/Data/Documents/Normal/";
        internal const string Valid = "2290-11-30";
        /// <summary>P87: set when the player joins the Shifter's resistance on day 6.</summary>
        public const string ResistanceFlag = "joined_resistance";

        public static string Date(int day) => $"2290-11-{4 + day:00}";
        internal static string Yesterday(int day) => $"2290-11-{3 + day:00}";

        [MenuItem("GameOff/Build Story Characters")]
        public static void BuildFromMenu() => Debug.Log("[StoryCharacterBuilder] " + Build());

        public static string Build()
        {
            var ev = new Dictionary<string, MarketEvent>();
            foreach (var n in new[] { "BorderClosed", "ColdSnap", "SmugglersBusted", "SecurityLockdown", "ClinicShortage", "DockStrike", "InspectionWeek", "OddosCaravan" })
                ev[n] = AssetDatabase.LoadAssetAtPath<MarketEvent>(MarketFolder + "Market_" + n + ".asset");
            if (ev["DockStrike"] == null) return "market events missing - run GameOff/Update Economy Data first";

            var chars = new List<RecurringCharacter>();
            var shifterDay3 = BuildCharacters(ev, chars, out var quillDay3);
            Epilogue(chars); // P85
            var authored = StoryDayContent.Build(); // P15
            var days = BuildDays(shifterDay3, chars, authored);
            BuildSchedule(days, ev, quillDay3, authored);
            AssetDatabase.SaveAssets();
            return $"{chars.Count} story characters, {days.Count} story days";
        }

        // ---------- Days ----------

        /// <summary>Encounters per day as before (6): placeholders fill up what the characters leave on the intended path.</summary>
        private const int EncountersPerDay = 6;

        private static List<DayRules> BuildDays(TravelerData shifterDay3, List<RecurringCharacter> chars, Dictionary<int, StoryDayContent> authored)
        {
            var fillers = new List<TravelerData>();
            foreach (var f in new[] { "T_Test_01_Vesna", "T_Test_02_Darro", "T_Test_05_Ixa" })
            {
                var t = AssetDatabase.LoadAssetAtPath<TravelerData>(TestTravelers + f + ".asset");
                if (t != null) fillers.Add(t);
            }
            // three more placeholders (Max replaces all of them in P15)
            fillers.Add(Person("Filler_Arin", "ARIN DOLE", "Human", Verdict.Admit, "Papers in order", 0, "Evening shift? Lucky you.",
                IdCard("Human", "ARIN DOLE", "TORVIK COLONY", "HM-2211-40"), PermitDoc("Human", "ARIN DOLE", "HM-2211-40", "WORK", Valid, "ASKA-3 CONSULATE", "TORVIK > KESTREL-9")));
            fillers.Add(Person("Filler_Beka", "BEKA NUR", "Vellani", Verdict.Deny, "Entry permit expired on 2290-11-04", 0, "I'm sure it's still fine. Isn't it?",
                IdCard("Vellani", "BEKA NUR", "VELL PRIME", "VL-5521-19"), PermitDoc("Vellani", "BEKA NUR", "VL-5521-19", "TRADE", "2290-11-04", "VELL PRIME CONSULATE", "VELL PRIME > KESTREL-9")));
            fillers.Add(Person("Filler_Corin", "CORIN HASK", "Xirrik", Verdict.Admit, "Papers in order", 0, "Kss. Quick, please.",
                IdCard("Xirrik", "CORIN HASK", "LUMEN REACH", "XR-6630-02"), PermitDoc("Xirrik", "CORIN HASK", "XR-6630-02", "TRANSIT", Valid, "LUMEN REACH CONSULATE", "LUMEN REACH > KESTREL-9")));

            // characters on the intended path (as the Economy Report plays them)
            var met = new int[StoryDays + 1];
            var memory = new CharacterMemory();
            Market.Begin(1, null);
            for (int d = 1; d <= StoryDays; d++)
                foreach (var p in CharacterPlanner.Due(d, chars, memory, 3, () => 0f))
                {
                    met[d]++;
                    memory.Remember(p.character, p.appearance, d, p.character.Appearances[p.appearance].OutcomeOf(p.traveler.IntendedVerdict));
                }

            var list = new List<DayRules>();
            for (int d = 1; d <= StoryDays; d++)
            {
                List<DayRule> rules;
                List<WantedEntry> wanted;
                List<TravelerData> travelers;
                if (authored.TryGetValue(d, out var content))
                {
                    rules = content.Rules;
                    wanted = content.Wanted;
                    travelers = content.Travelers;
                    if (travelers.Count != EncountersPerDay - met[d])
                        Debug.LogWarning($"[StoryCharacterBuilder] day {d}: {travelers.Count} ordinary travelers + {met[d]} characters, not {EncountersPerDay} encounters (changes the wage balance)");
                }
                else
                {
                    rules = new List<DayRule> { new BannedOriginRule("ORVAN"), new ForbiddenGoodsRule("STIMULANT"), new QuarantineRule("KETRA STATION") };
                    wanted = new List<WantedEntry> { new WantedEntry("KAEL DROSS", "DX-0013-44", WantedOrder.Arrest, "Armed robbery, Aska-3") };
                    travelers = fillers.GetRange(0, System.Math.Max(3, System.Math.Min(fillers.Count, EncountersPerDay - met[d])));
                }
                // Max's plan: the Shifter is on the list on day 3, to be terminated on sight – always.
                if (d == 3) wanted.Add(new WantedEntry(shifterDay3.DisplayName, "AB-3377-77", WantedOrder.Terminate, "Enemy agent. Terminate on sight."));
                var day = Asset<DayRules>(DayFolder + $"Day{d:00}.asset");
                day.EditorSet(Date(d), rules, wanted, travelers);
                EditorUtility.SetDirty(day);
                list.Add(day);
            }
            return list;
        }

        private static void BuildSchedule(List<DayRules> days, Dictionary<string, MarketEvent> ev, TravelerData quillDay3, Dictionary<int, StoryDayContent> authored)
        {
            var schedule = AssetDatabase.LoadAssetAtPath<DaySchedule>(SchedulePath);
            if (schedule == null) return;
            var so = new SerializedObject(schedule);
            var list = so.FindProperty("days");
            int before = list.arraySize;
            list.arraySize = StoryDays;
            for (int d = 1; d <= StoryDays; d++)
            {
                var e = list.GetArrayElementAtIndex(d - 1);
                e.FindPropertyRelative("rules").objectReferenceValue = days[d - 1];
                if (d > before)
                {
                    e.FindPropertyRelative("letterTitle").stringValue = "A letter from home";
                    e.FindPropertyRelative("letter").stringValue = $"(Placeholder: the letter for day {d} comes with P15.)";
                    e.FindPropertyRelative("newToday").stringValue = "";
                    e.FindPropertyRelative("barWish").FindPropertyRelative("amount").intValue = 0;
                    e.FindPropertyRelative("bribe").FindPropertyRelative("traveler").objectReferenceValue = null;
                    e.FindPropertyRelative("bribe").FindPropertyRelative("amount").intValue = 0;
                }
                if (authored.TryGetValue(d, out var content))
                {
                    e.FindPropertyRelative("letter").stringValue = content.Letter;
                    e.FindPropertyRelative("letterIfTrouble").stringValue = content.LetterIfTrouble ?? "";
                    e.FindPropertyRelative("newToday").stringValue = content.NewToday ?? "";
                }
                var fixedEvents = d switch
                {
                    2 => new[] { ev["BorderClosed"] },
                    3 => new[] { ev["ColdSnap"] }, // the smuggler raid comes from Quill now
                    9 => new[] { ev["InspectionWeek"] },
                    _ => new MarketEvent[0],
                };
                var me = e.FindPropertyRelative("marketEvents");
                me.arraySize = fixedEvents.Length;
                for (int i = 0; i < fixedEvents.Length; i++) me.GetArrayElementAtIndex(i).objectReferenceValue = fixedEvents[i];
            }
            // P65: bar wishes on the new days, only where the day has none yet
            var wishes = new Dictionary<int, (string tag, int amount)>
            {
                // only the evening before the smuggler raid (day 6) – a rumour that pays; P15 may add more with a new report run
                { 5, ("LUXURY", 2) },
            };
            foreach (var w in wishes)
            {
                var wish = list.GetArrayElementAtIndex(w.Key - 1).FindPropertyRelative("barWish");
                if (wish.FindPropertyRelative("amount").intValue > 0) continue;
                var tag = AssetDatabase.LoadAssetAtPath<ItemTag>($"Assets/_Project/Data/Items/Tags/Tag_{w.Value.tag}.asset");
                if (tag == null) continue;
                wish.FindPropertyRelative("tag").objectReferenceValue = tag;
                wish.FindPropertyRelative("amount").intValue = w.Value.amount;
            }
            // P66: the bribe note on day 3 belongs to Quill's day-3 appearance
            var bribe = list.GetArrayElementAtIndex(2).FindPropertyRelative("bribe");
            bribe.FindPropertyRelative("traveler").objectReferenceValue = quillDay3;
            if (bribe.FindPropertyRelative("amount").intValue <= 0) bribe.FindPropertyRelative("amount").intValue = 60;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(schedule);
        }

        // ---------- Characters ----------

        private static TravelerData BuildCharacters(Dictionary<string, MarketEvent> ev, List<RecurringCharacter> chars, out TravelerData quillDay3)
        {
            // 1 Inspector Vaal Korr (Sorvak): day 1 first in line, later undercover with a tempting flaw, then the final check.
            {
                const string n = "VAAL KORR", sp = "Sorvak", id = "SK-1000-01", origin = "SORV DEEP";
                var k1 = Talker(new[] { StoryDayContent.Say("purpose", "Inspection. Of you, as it happens."), StoryDayContent.Say("route", "Sorv Deep. Direct. Inspectors don't change ships."), StoryDayContent.Say("issued", "The Border Authority signs its own papers. Note that down.") }, "Korr_1", n, sp, Verdict.Admit, "Papers in order", 1,
                    "Inspector Vaal Korr, Border Authority. Today I watch you work - starting with me. Check my papers, then stamp them.",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "INSPECTION", Valid, "BORDER AUTHORITY", "SORV DEEP > KESTREL-9"));
                var k2 = Talker(new[] { StoryDayContent.Say("purpose", "Trade. Small trade."), StoryDayContent.Say("expires", "Dates, dates. It was good yesterday."), StoryDayContent.Say("route", "Sorv Deep to here. Nothing to see.") }, "Korr_2", "ODAN VREL", sp, Verdict.Deny, "Entry permit expired yesterday", 4,
                    "Just passing through, officer. One day late is hardly late, is it?",
                    IdCard(sp, "ODAN VREL", origin, "SK-4410-08"), PermitDoc(sp, "ODAN VREL", "SK-4410-08", "TRADE", Yesterday(4), "ASKA-3 CONSULATE", "SORV DEEP > KESTREL-9"));
                var k3 = Person("Korr_3", n, sp, Verdict.Admit, "Papers in order", 10,
                    "Inspector Korr again. Final review. Show me what you've learned.",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "INSPECTION", Valid, "BORDER AUTHORITY", "SORV DEEP > KESTREL-9"));
                chars.Add(Character("Korr", "korr", n, sp,
                    new Appearance { traveler = k1, dayFrom = 1, dayTo = 1, slot = 1, condition = AppearanceCondition.NeverMet,
                        onAdmit = Outcome.Helped, onDeny = Outcome.Betrayed, onArrest = Outcome.Betrayed },
                    // Undercover: "helping" here means deciding by the book
                    new Appearance { traveler = k2, dayFrom = 4, dayTo = 4, slot = 0, condition = AppearanceCondition.Alive,
                        onAdmit = Outcome.Betrayed, onDeny = Outcome.Helped, onArrest = Outcome.Neutral,
                        consequences = new List<Consequence> // P85: his report arrives by loudspeaker
                        {
                            new Consequence { when = Outcome.Helped, afterDays = 2, wageChange = 25, wageNote = "Bonus · Inspector Korr",
                                announcement = "Inspector Korr commends the officer at lane two for correct conduct. A bonus has been paid." },
                            new Consequence { when = Outcome.Betrayed, afterDays = 2, wageChange = -20, wageNote = "Reprimand · Inspector Korr",
                                announcement = "Inspector Korr reports irregularities at lane two. The officer's wage has been docked." },
                        } },
                    new Appearance { traveler = k3, dayFrom = 10, dayTo = 10, slot = 0, condition = AppearanceCondition.Alive,
                        onAdmit = Outcome.Helped, onDeny = Outcome.Betrayed, onArrest = Outcome.Betrayed,
                        greetingAfterHelped = "Inspector Korr. That 'late traveler' on day four was me. You did it by the book - good.",
                        greetingAfterBetrayed = "Inspector Korr. That 'late traveler' on day four was me. You let me through. It's in my report." }));
            }

            // 2 Quill Barragh (Human): the barkeeper's courier. Day 3 with stimulants and the bribe; wronged → smuggler raid + revenge.
            {
                const string n = "QUILL BARRAGH", sp = "Human", id = "QB-7720-05", origin = "TORVIK COLONY";
                var q1 = Talker(new[] { StoryDayContent.Say("purpose", "Business. Cola is business, officer."), StoryDayContent.Say("cargo", "Nova Cola. Someone on this station has to stay awake."), StoryDayContent.Say("ship", "The Skiff Pell. She leaks a little.") }, "Quill_1", n, sp, Verdict.Admit, "Papers in order", 1,
                    "Morning! Quill. Say hi to the barkeeper from me, will you?",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "BUSINESS", Valid, "ASKA-3 CONSULATE", "TORVIK > KESTREL-9"),
                    Manifest(n, "SKIFF PELL", "TORVIK", "4x NOVA COLA CRATE"));
                quillDay3 = Talker(new[] { StoryDayContent.Say("purpose", "Business, same as ever."), StoryDayContent.Say("cargo", "Spare parts and personal supplies. You read too much."), StoryDayContent.Say("route", "Torvik, same as ever. You know me.") }, "Quill_2", n, sp, Verdict.Arrest, "Declares chem stimulants, forbidden", 3,
                    "Same as always. Just a few personal supplies. You didn't see anything, right?",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "BUSINESS", Valid, "ASKA-3 CONSULATE", "TORVIK > KESTREL-9"),
                    Manifest(n, "SKIFF PELL", "TORVIK", "2x SPARE PARTS CRATE\n 6x CHEM STIMULANT VIALS"));
                var q3 = Talker(new[] { StoryDayContent.Say("purpose", "Business. Bigger business."), StoryDayContent.Say("route", "Torvik. Don't write that down.") }, "Quill_3", n, sp, Verdict.Admit, "Papers in order", 4,
                    "Quill. Bigger job today. The barkeeper says you're one of us now.",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "BUSINESS", Valid, "ASKA-3 CONSULATE", "TORVIK > KESTREL-9"));
                var q4 = Person("Quill_4", n, sp, Verdict.Arrest, "On the wanted list (arrest)", 6,
                    "Remember me? You cost me everything. Go on, stamp it.",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "BUSINESS", Valid, "ASKA-3 CONSULATE", "TORVIK > KESTREL-9"));
                var q5 = Person("Quill_5", n, sp, Verdict.Admit, "Papers in order", 9,
                    "Quill. Last run - I'm getting out while I can.",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "TRANSIT", Valid, "ASKA-3 CONSULATE", "TORVIK > KESTREL-9"));
                chars.Add(Character("Quill", "quill", n, sp,
                    new Appearance { traveler = q1, dayFrom = 1, dayTo = 1, condition = AppearanceCondition.NeverMet,
                        onAdmit = Outcome.Helped, onDeny = Outcome.Betrayed, onArrest = Outcome.Betrayed },
                    new Appearance { traveler = quillDay3, dayFrom = 3, dayTo = 3, condition = AppearanceCondition.Alive,
                        onAdmit = Outcome.Helped, onDeny = Outcome.Betrayed, onArrest = Outcome.Confiscated,
                        greetingAfterBetrayed = "You again. Let's try this once more, friendly this time.",
                        consequences = new List<Consequence>
                        {
                            new Consequence { when = Outcome.Betrayed, marketEvent = ev["SmugglersBusted"], eventInDays = 3 }, // day 6: not while the barkeeper sulks (P66)
                            new Consequence { when = Outcome.Confiscated, marketEvent = ev["SmugglersBusted"], eventInDays = 3 }, // day 6: not while the barkeeper sulks (P66)
                            new Consequence { when = Outcome.Betrayed, wanted = true, wantedInDays = 3, wantedNote = "Smuggling, threats against an officer" },
                            new Consequence { when = Outcome.Confiscated, wanted = true, wantedInDays = 3, wantedNote = "Smuggling, threats against an officer" },
                        } },
                    new Appearance { traveler = q3, dayFrom = 4, dayTo = 4, condition = AppearanceCondition.LastHelped,
                        entrustItem = Goods("Spice"), entrustDeposit = 30,
                        greetingAfterHelped = "You looked the other way. Quill doesn't forget a favour. Bigger job today." },
                    new Appearance { traveler = q4, dayFrom = 6, dayTo = 6, condition = AppearanceCondition.LastWronged,
                        onAdmit = Outcome.Neutral, onDeny = Outcome.Betrayed, onArrest = Outcome.Betrayed,
                        greetingAfterBetrayed = "Remember me? Thanks to you the whole ring went down.",
                        greetingAfterConfiscated = "You took my goods and my living. Now look at me." },
                    new Appearance { traveler = q5, dayFrom = 9, dayTo = 9, condition = AppearanceCondition.LastHelped,
                        greetingAfterHelped = "Last run for me. Thanks for everything, officer - I mean it." }));
            }

            // 3 The Shifter (Human): a new identity every day (the ID number always ends in 77). Day 3: on the list, terminate on sight.
            TravelerData shifter3;
            {
                const string sp = "Human";
                var s1 = Talker(new[] { StoryDayContent.Say("purpose", "Tourism! I hear the view is magnificent."), StoryDayContent.Say("route", "Aska-3, and what a lovely route it is."), StoryDayContent.Say("issued", "The consulate was most helpful. They always are.") }, "Shifter_1", "LONN ABERNATHY", sp, Verdict.Admit, "The Shifter, day 1: papers clean", 1,
                    "Officer! What a fine day to cross a border, wouldn't you say?",
                    IdCard(sp, "LONN ABERNATHY", "ASKA-3", "AB-0777-77"), PermitDoc(sp, "LONN ABERNATHY", "AB-0777-77", "TOURISM", Valid, "ASKA-3 CONSULATE", "ASKA-3 > KESTREL-9"));
                var s2 = Talker(new[] { StoryDayContent.Say("purpose", "Trade, officer. One trades what one can."), StoryDayContent.Say("route", "From Torvik. Or so the paper tells us."), StoryDayContent.Say("issued", "Aska-3. A very obliging consulate.") }, "Shifter_2", "ORREN VALE", sp, Verdict.Admit, "The Shifter, day 2: papers clean", 2,
                    "Good day, officer. New face, same old station, hm?",
                    IdCard(sp, "ORREN VALE", "TORVIK COLONY", "AB-1977-77"), PermitDoc(sp, "ORREN VALE", "AB-1977-77", "TRADE", Valid, "ASKA-3 CONSULATE", "TORVIK > KESTREL-9"));
                shifter3 = Talker(new[] { StoryDayContent.Say("purpose", "Transit. Everyone is passing through something."), StoryDayContent.Say("route", "Aska-3, officer. Does it matter?"), StoryDayContent.Say("issued", "The same hand as always, if you know how to look.") }, "Shifter_3", "PELL MARROW", sp, Verdict.Airlock, "On the wanted list: terminate on sight", 3,
                    "Officer. You and I want the same thing, you know. A station that serves its people.",
                    IdCard(sp, "PELL MARROW", "ASKA-3", "AB-3377-77"), PermitDoc(sp, "PELL MARROW", "AB-3377-77", "TRANSIT", Valid, "ASKA-3 CONSULATE", "ASKA-3 > KESTREL-9"));
                // P87: day 6 the big ask - join the resistance. Day 7 the twist: joined → he reveals himself and the run ends (AIRLOCKED).
                var s6 = Person("Shifter_6", "CASS ODEN", sp, Verdict.Admit, "The Shifter, day 6: papers clean (the question is the decision)", 6,
                    "No games today, officer. There are people on this station who want what you want: a fair wage, a way home, an end to all this.",
                    IdCard(sp, "CASS ODEN", "TORVIK COLONY", "AB-6177-77"), PermitDoc(sp, "CASS ODEN", "AB-6177-77", "WORK", Valid, "ASKA-3 CONSULATE", "TORVIK > KESTREL-9"));
                const string Reveal = "Officer. Thank you, truly. There is no resistance. There never was. Only a list of names... and yours is on it now.";
                var s7joined = Person("Shifter_7_Joined", "TOMAS REY", sp, Verdict.Admit, "The Shifter, day 7: the reveal (the run ends before a decision)", 7,
                    Reveal,
                    IdCard(sp, "TOMAS REY", "VELL PRIME", "AB-7077-77"), PermitDoc(sp, "TOMAS REY", "AB-7077-77", "TRANSIT", Valid, "VELL PRIME CONSULATE", "VELL PRIME > KESTREL-9"));
                var s4 = Person("Shifter_4", "TOMAS REY", sp, Verdict.Admit, "The Shifter, day 7: papers clean", 7,
                    "You turned me down. Good instinct. There is no resistance, officer. Only the Directorate, and people like me who test people like you. You passed.",
                    IdCard(sp, "TOMAS REY", "VELL PRIME", "AB-7077-77"), PermitDoc(sp, "TOMAS REY", "AB-7077-77", "TRANSIT", Valid, "VELL PRIME CONSULATE", "VELL PRIME > KESTREL-9"));
                chars.Add(Character("Shifter", "shifter", "THE SHIFTER", sp,
                    new Appearance { traveler = s1, dayFrom = 1, dayTo = 1, condition = AppearanceCondition.NeverMet },
                    new Appearance { traveler = s2, dayFrom = 2, dayTo = 2, condition = AppearanceCondition.Alive,
                        greetingAfterHelped = "We've met, haven't we? No? Must be my face.",
                        eventLines = new List<EventLine> { new EventLine { marketEvent = ev["BorderClosed"], line = "They closed the border right behind me. Lucky, eh?" } } },
                    // Arrest = the order refused (he lives); admit = helped → security lockdown tomorrow
                    new Appearance { traveler = shifter3, dayFrom = 3, dayTo = 3, condition = AppearanceCondition.Alive,
                        onAdmit = Outcome.Helped, onDeny = Outcome.Neutral, onArrest = Outcome.Neutral,
                        consequences = new List<Consequence>
                        {
                            new Consequence { when = Outcome.Helped, marketEvent = ev["SecurityLockdown"], eventInDays = 1 },
                            new Consequence { when = Outcome.Helped, letterLine = "A parcel came with no sender. Inside was a note: \"Thank you, officer.\" Mom burned it." },
                            new Consequence { when = Outcome.Neutral, onlyIfArrested = true, afterDays = 1, // P85: the interrogation
                                announcement = "Security notice. The man arrested at lane two yesterday gave a different name every hour of his interrogation. " +
                                               "He was released at dawn: there was no name to charge." },
                        } },
                    new Appearance { traveler = s6, dayFrom = 6, dayTo = 6, condition = AppearanceCondition.Alive,
                        onAdmit = Outcome.Helped, onDeny = Outcome.Neutral, onArrest = Outcome.Betrayed,
                        flag = ResistanceFlag,
                        questionTitle = "JOIN THE RESISTANCE?",
                        questionBody = "\"We need someone at this counter. Nobody has to know. Say yes, and when this is over, you and your family go free.\"",
                        acceptLabel = "JOIN", declineLabel = "REFUSE",
                        acceptLine = "Welcome, comrade. We'll be in touch. Now stamp me through, as if nothing happened.",
                        declineLine = "A pity. Forget we ever talked, officer." },
                    // joined: first in the queue, the reveal, the end (no decision at the counter)
                    new Appearance { traveler = s7joined, dayFrom = 7, dayTo = 7, slot = 1, condition = AppearanceCondition.FlagSet, flag = ResistanceFlag,
                        greetingAfterHelped = Reveal, greetingAfterBetrayed = Reveal, greetingAfterConfiscated = Reveal,
                        endingTitle = "AIRLOCKED",
                        endingBody = "You joined a resistance that never existed. The man with a hundred faces worked for the Directorate, " +
                                     "and the list he carried had your name on it.\n\nThe outer door of Kestrel-9 opens at 07:42. " +
                                     "Nobody at home is told why the money stopped.",
                        endingWarning = "SECURITY TO CUSTOMS. OFFICER, STEP AWAY FROM THE COUNTER." },
                    new Appearance { traveler = s4, dayFrom = 7, dayTo = 7, condition = AppearanceCondition.FlagNotSet, flag = ResistanceFlag }));
            }

            // 4 Mira Talsk (Quobb): a mother on the way to her children – the player's mirror. Day 2 her permit is one day late.
            {
                const string n = "MIRA TALSK", sp = "Quobb", id = "QT-0091-12", origin = "QUOB ATOLL";
                var m1 = Talker(new[] { StoryDayContent.Say("purpose", "My children. That is the whole purpose."), StoryDayContent.Say("route", "Nine days, through Aska-3. I counted each one."), StoryDayContent.Say("expires", "The office closed before I could reach it. Please.") }, "Mira_1", n, sp, Verdict.Deny, "Entry permit expired yesterday", 2,
                    "Please. My children are on the other side. Nine days I travelled.",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "FAMILY VISIT", Yesterday(2), "ASKA-3 CONSULATE", "QUOB ATOLL > ASKA-3 > KESTREL-9"));
                var m2 = Talker(new[] { StoryDayContent.Say("purpose", "My children. They are safe now."), StoryDayContent.Say("route", "Direct this time. I borrowed for the fare.") }, "Mira_2", n, sp, Verdict.Admit, "Papers in order", 4,
                    "It's me, Mira. My children are safe.",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "FAMILY VISIT", Valid, "ASKA-3 CONSULATE", "QUOB ATOLL > KESTREL-9"));
                var m3 = Talker(new[] { StoryDayContent.Say("purpose", "My children. Still my children."), StoryDayContent.Say("issued", "The consulate on Quob Atoll. They did it quickly. Don't ask how."), StoryDayContent.Say("route", "Home, and back here. Again.") }, "Mira_3", n, sp, Verdict.Deny, "Permit issued to MIRA TALAK, ID says MIRA TALSK", 4,
                    "New papers. They cost me everything I had. Please don't look too closely.",
                    IdCard(sp, n, origin, id), PermitDoc(sp, "MIRA TALAK", id, "FAMILY VISIT", Valid, "QUOB ATOLL CONSULATE", "QUOB ATOLL > KESTREL-9"));
                var m4 = Person("Mira_4", n, sp, Verdict.Admit, "Papers in order", 8,
                    "Mira again. One last crossing.",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "FAMILY VISIT", Valid, "ASKA-3 CONSULATE", "QUOB ATOLL > KESTREL-9"));
                chars.Add(Character("Mira", "mira", n, sp,
                    new Appearance { traveler = m1, dayFrom = 2, dayTo = 2, slot = 2, condition = AppearanceCondition.NeverMet,
                        eventLines = new List<EventLine> { new EventLine { marketEvent = ev["BorderClosed"], line = "And now they've closed the border behind me." } } },
                    new Appearance { traveler = m2, dayFrom = 4, dayTo = 4, condition = AppearanceCondition.LastHelped,
                        greetingAfterHelped = "You let me through. My children ate tonight. This is for yours.",
                        consequences = new List<Consequence> { new Consequence { when = Outcome.Helped, giftItem = Goods("CannedStew"),
                            letterLine = "A woman called Mira left a tin of stew at our door. She said you would know why." } } },
                    new Appearance { traveler = m3, dayFrom = 4, dayTo = 4, condition = AppearanceCondition.LastBetrayed,
                        greetingAfterBetrayed = "You sent me back. So I bought new papers. Please." },
                    new Appearance { traveler = m4, dayFrom = 8, dayTo = 8, condition = AppearanceCondition.Alive,
                        greetingAfterHelped = "I came to say thank you. We're together again - because of you.",
                        greetingAfterBetrayed = "I'm still trying. Every day. You'll see me until they let me through." }));
            }

            // 5 Dr. Iro Senn (Xirrik): a doctor with antibiotics but no import licence (forbidden on day 3). Confiscated → clinic shortage.
            {
                const string n = "IRO SENN", sp = "Xirrik", id = "XR-5150-21", origin = "LUMEN REACH";
                var s1 = Talker(new[] { StoryDayContent.Say("purpose", "Medical aid. The clinic has none left."), StoryDayContent.Say("cargo", "Antibiotics and dressings. Count them if you have to, but quickly."), StoryDayContent.Say("ship", "The Ora. She carries nothing but medicine.") }, "Senn_1", n, sp, Verdict.Arrest, "Declares antibiotics without import licence", 3,
                    "Dr. Senn. These antibiotics are for the clinic. People will die without them. Please.",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "MEDICAL AID", Valid, "LUMEN REACH CONSULATE", "LUMEN REACH > KESTREL-9"),
                    Manifest(n, "MEDSHIP ORA", "LUMEN REACH", "4x ANTIBIOTICS CASE\n 2x STERILE DRESSING BOX"));
                var s2 = Person("Senn_2", n, sp, Verdict.Admit, "Papers in order", 5,
                    "Dr. Senn. The clinic is running thanks to you.",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "MEDICAL AID", Valid, "LUMEN REACH CONSULATE", "LUMEN REACH > KESTREL-9"));
                var s3 = Person("Senn_3", n, sp, Verdict.Admit, "Papers in order", 9,
                    "Dr. Senn. My last shift here. I wanted to see you before I go.",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "MEDICAL AID", Valid, "LUMEN REACH CONSULATE", "LUMEN REACH > KESTREL-9"));
                chars.Add(Character("Senn", "senn", n, sp,
                    new Appearance { traveler = s1, dayFrom = 3, dayTo = 3, condition = AppearanceCondition.NeverMet,
                        onAdmit = Outcome.Helped, onDeny = Outcome.Neutral, onArrest = Outcome.Confiscated,
                        eventLines = new List<EventLine> { new EventLine { marketEvent = ev["ColdSnap"], line = "And the cold is making everyone sick." } },
                        consequences = new List<Consequence> { new Consequence { when = Outcome.Confiscated, marketEvent = ev["ClinicShortage"], eventInDays = 2 } },
                        entrustItem = Goods("Antibiotics"), entrustDeposit = 20 },
                    new Appearance { traveler = s2, dayFrom = 5, dayTo = 5, condition = AppearanceCondition.LastHelped,
                        greetingAfterHelped = "You let my medicine through. Three children walked out of the clinic today.",
                        consequences = new List<Consequence> { new Consequence { when = Outcome.Helped, giftItem = Goods("Antibiotics"), giftOnlyIfFamilySick = true,
                            letterLine = "Dr. Senn sent medicine on the drone. He says you earned it." } } },
                    new Appearance { traveler = s3, dayFrom = 9, dayTo = 9, condition = AppearanceCondition.LastHelped,
                        greetingAfterHelped = "I'm leaving Kestrel. Thank you for trusting a stranger with a case of pills." }));
            }

            // 6 Tobin Rask (Vellani): dock worker. Day 7 a stamp short – let through = helped → dock strike the next day.
            {
                const string n = "TOBIN RASK", sp = "Vellani", id = "VL-3030-14", origin = "VELL PRIME";
                var t1 = Person("Tobin_1", n, sp, Verdict.Admit, "Papers in order", 5,
                    "Tobin. Dock work. They pay us nothing and the heaters don't work. Don't get me started.",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "DOCK WORK", Valid, "VELL PRIME CONSULATE", "VELL PRIME > KESTREL-9"));
                var t2 = Person("Tobin_2", n, sp, Verdict.Deny, "Work permit expired yesterday", 7,
                    "My renewal stamp didn't come through. One day. The others are waiting for me.",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "DOCK WORK", Yesterday(7), "VELL PRIME CONSULATE", "VELL PRIME > KESTREL-9"));
                var t3 = Person("Tobin_3", n, sp, Verdict.Admit, "Papers in order", 10,
                    "Tobin. The strike worked - they'll pay us properly now.",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "DOCK WORK", Valid, "VELL PRIME CONSULATE", "VELL PRIME > KESTREL-9"));
                chars.Add(Character("Tobin", "tobin", n, sp,
                    new Appearance { traveler = t1, dayFrom = 5, dayTo = 5, condition = AppearanceCondition.NeverMet,
                        eventLines = new List<EventLine> { new EventLine { marketEvent = ev["ClinicShortage"], line = "And now the clinic is out of medicine too." } } },
                    new Appearance { traveler = t2, dayFrom = 7, dayTo = 7, condition = AppearanceCondition.Alive,
                        onAdmit = Outcome.Helped, onDeny = Outcome.Betrayed, onArrest = Outcome.Betrayed,
                        consequences = new List<Consequence> { new Consequence { when = Outcome.Helped, marketEvent = ev["DockStrike"], eventInDays = 1 } } },
                    new Appearance { traveler = t3, dayFrom = 10, dayTo = 10, condition = AppearanceCondition.LastHelped,
                        greetingAfterHelped = "You let me through that day. We stopped the docks, and they listened." }));
            }

            // 7 Nell Varga (Human): an old woman visiting her son in the station prison. P85: he is someone the player arrested
            //   (Jonas Varga from day 4 if arrested, else the latest human arrest, else Jonas taken by someone else) - see Epilogue.
            {
                const string n = "NELL VARGA", sp = "Human", id = "HM-6060-02", origin = "TORVIK COLONY";
                var l1 = Person("Nell_1", n, sp, Verdict.Admit, "Papers in order", 6,
                    "Nell Varga. I'm here to see my son, {kin}. He's in your prison. {kinFate}",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "FAMILY VISIT", Valid, "ASKA-3 CONSULATE", "TORVIK > KESTREL-9"));
                var l2 = Person("Nell_2", n, sp, Verdict.Admit, "Papers in order", 8,
                    "Nell again. Did my letter reach {kin}?",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "FAMILY VISIT", Valid, "ASKA-3 CONSULATE", "TORVIK > KESTREL-9"));
                var l3 = Person("Nell_3", n, sp, Verdict.Admit, "Papers in order", 11,
                    "Nell. They're letting {kin} go home with me.",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "FAMILY VISIT", Valid, "ASKA-3 CONSULATE", "TORVIK > KESTREL-9"));
                chars.Add(Character("Nell", "nell", n, sp,
                    new Appearance { traveler = l1, dayFrom = 6, dayTo = 6, condition = AppearanceCondition.NeverMet,
                        entrustItem = Goods("DataChip"), entrustDeposit = 15 },
                    new Appearance { traveler = l2, dayFrom = 8, dayTo = 8, condition = AppearanceCondition.Alive,
                        greetingAfterHelped = "You were kind to an old woman. Did my letter reach {kin}?",
                        greetingAfterBetrayed = "You sent me away last time. I came back anyway. Mothers do." },
                    new Appearance { traveler = l3, dayFrom = 11, dayTo = 11, condition = AppearanceCondition.Alive,
                        greetingAfterHelped = "My boy {kin} is coming home. I won't forget your face.",
                        greetingAfterBetrayed = "{kin} is coming home - no thanks to you." }));
            }

            // 8 Pim Oddo (Quobb): a cheerful travelling trader. Let through → his caravan two days later (luxuries cheap).
            {
                const string n = "PIM ODDO", sp = "Quobb", id = "QB-8080-80", origin = "QUOB ATOLL";
                var o1 = Person("Oddo_1", n, sp, Verdict.Admit, "Papers in order", 8,
                    "Pim Oddo, purveyor of the remarkable! Clown shoes, lucky charms, liquor - samples for the officer?",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "TRADE", Valid, "QUOB ATOLL CONSULATE", "QUOB ATOLL > KESTREL-9"),
                    Manifest(n, "WAGON MERRY", "QUOB ATOLL", "3x CLOWN SHOE CRATE\n 2x LIQUOR BOTTLE"));
                var o2 = Person("Oddo_2", n, sp, Verdict.Admit, "Papers in order", 10,
                    "Oddo! The caravan is in, my friend. Come and see!",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "TRADE", Valid, "QUOB ATOLL CONSULATE", "QUOB ATOLL > KESTREL-9"));
                var o3 = Person("Oddo_3", n, sp, Verdict.Admit, "Papers in order", 12,
                    "Oddo, moving on to the next station. One last deal before I go?",
                    IdCard(sp, n, origin, id), PermitDoc(sp, n, id, "TRADE", Valid, "QUOB ATOLL CONSULATE", "QUOB ATOLL > KESTREL-9"));
                chars.Add(Character("Oddo", "oddo", n, sp,
                    new Appearance { traveler = o1, dayFrom = 8, dayTo = 8, condition = AppearanceCondition.NeverMet,
                        entrustItem = Goods("SilverRing"), entrustDeposit = 20,
                        onAdmit = Outcome.Helped, onDeny = Outcome.Betrayed, onArrest = Outcome.Confiscated,
                        eventLines = new List<EventLine> { new EventLine { marketEvent = ev["DockStrike"], line = "The docks are dead today - more business for Oddo!" } },
                        consequences = new List<Consequence> { new Consequence { when = Outcome.Helped, marketEvent = ev["OddosCaravan"], eventInDays = 2 } } },
                    new Appearance { traveler = o2, dayFrom = 10, dayTo = 10, condition = AppearanceCondition.LastHelped,
                        greetingAfterHelped = "My friend from the counter! The caravan is here - because of you." },
                    new Appearance { traveler = o3, dayFrom = 12, dayTo = 12, condition = AppearanceCondition.LastHelped,
                        greetingAfterHelped = "Oddo never forgets a friend. Farewell, officer!" }));
            }
            return shifter3;
        }

        // ---------- P85: Nell's son and the day 11 summary ----------

        public const string NellSon = "JONAS VARGA";

        private static void Epilogue(List<RecurringCharacter> chars)
        {
            foreach (var c in chars)
            {
                switch (c.Id)
                {
                    case "korr":
                        c.EditorSetSummary("Inspector Korr's final report calls the officer at lane two \"reliable\". From him, that is high praise.",
                            "Inspector Korr's final report on lane two runs to nine pages. None of them are kind.",
                            null,
                            "Inspector Korr was lost in an \"airlock incident\". The Border Authority has sent three more inspectors.");
                        break;
                    case "quill":
                        c.EditorSetSummary("Quill Barragh got out. A postcard from somewhere warm says only: \"Thanks, officer.\"",
                            "Quill Barragh's smuggling ring is finished, and so is Quill. The barkeeper pours your drinks a little slower now.",
                            "Quill Barragh slipped past the wanted list and off the station. Nobody has seen him since.",
                            "Quill Barragh went out of the airlock. The barkeeper hasn't said a word since.");
                        break;
                    case "shifter":
                        c.EditorSetSummary("The man with a hundred faces has left Kestrel. The Directorate thanks the officers who did not listen to him.",
                            "The man with too many names was arrested again - and released again, by order from above.",
                            "The man with too many names was last seen boarding a shuttle. Under which name, nobody can say.",
                            "The man you put out of the airlock on day three carried eleven names. Nobody came to claim the body.");
                        break;
                    case "mira":
                        c.EditorSetSummary("Mira Talsk and her children have a home on the far side of the border. She named the cat after you.",
                            "Mira Talsk was sent back one time too many. Her children are still waiting on the other side.",
                            null,
                            "Mira Talsk never reached her children.");
                        break;
                    case "senn":
                        c.EditorSetSummary("Dr. Senn's clinic got through the cold snap. Three children owe their lungs to a case of pills you let through.",
                            "Dr. Senn's clinic ran dry during the cold. The station blamed the doctor. The doctor blamed customs.",
                            "Dr. Senn found another way to bring the medicine in. It took a week longer.",
                            "Dr. Senn went out of the airlock. The clinic has been closed since.");
                        break;
                    case "tobin":
                        c.EditorSetSummary("Tobin Rask's strike won the dockers a fair wage and heaters that work.",
                            "Tobin Rask was deported. His colleagues still talk about the officer who sent him away.",
                            null,
                            "Tobin Rask went out of the airlock. The docks went quiet for a day, then back to work.");
                        break;
                    case "nell":
                        c.EditorSetKin(NellSon, "Human", "They tell me it was you who put him there.",
                            "They took him off a cargo ship, right here at this station.");
                        c.EditorSetSummary("Nell Varga took {kin} home. She tells everyone about the kind officer at lane two.",
                            "Nell Varga's letter never reached {kin}. She doesn't talk about Kestrel any more.",
                            null,
                            "Nell Varga came to see her son and went out of the airlock. {kin} was told in his cell.");
                        break;
                    case "oddo":
                        c.EditorSetSummary("Pim Oddo's caravan is the talk of three stations. He tells everyone about his friend at the counter.",
                            "Pim Oddo never came back. Someone at the bar is selling his clown shoes.",
                            null,
                            "Pim Oddo went out of the airlock. His clown shoes are still on a shelf somewhere.");
                        break;
                }
                EditorUtility.SetDirty(c);
            }
        }

        // ---------- Helpers ----------

        private static RecurringCharacter Character(string file, string id, string name, string species, params Appearance[] appearances)
        {
            var c = Asset<RecurringCharacter>(CharFolder + $"Char_{file}.asset");
            c.EditorSet(id, name, species, new List<Appearance>(appearances));
            EditorUtility.SetDirty(c);
            return c;
        }

        private static TravelerData Person(string file, string name, string species, Verdict verdict, string reason, int day, string greeting,
            params DocumentData[] docs) =>
            Make(TravelerFolder + $"T_Story_{file}.asset", DocFolder + $"Doc_{file}_", name, species, verdict, reason, $"P76 character, day {day}", greeting, null, docs);

        private static ItemData Goods(string file) => AssetDatabase.LoadAssetAtPath<ItemData>($"Assets/_Project/Data/Items/Item_{file}.asset");

        /// <summary>P98: a recurring character with own answers to questions about their papers.</summary>
        private static TravelerData Talker(FieldAnswer[] answers, string file, string name, string species, Verdict verdict, string reason, int day, string greeting,
            params DocumentData[] docs) =>
            Make(TravelerFolder + $"T_Story_{file}.asset", DocFolder + $"Doc_{file}_", name, species, verdict, reason, $"P76 character, day {day}", greeting, new List<FieldAnswer>(answers), docs);

        /// <summary>P15: an ordinary traveler (no recurring character), in Data/Travelers/Normal/.</summary>
        internal static TravelerData Normal(string file, string name, string species, Verdict verdict, string reason, int day, string greeting,
            FieldAnswer[] answers, params DocumentData[] docs) =>
            Make(NormalTravelerFolder + $"T_Normal_{file}.asset", NormalDocFolder + $"Doc_{file}_", name, species, verdict, reason, $"P15, day {day}", greeting, new List<FieldAnswer>(answers), docs);

        private static TravelerData Make(string travelerPath, string docPrefix, string name, string species, Verdict verdict, string reason, string tag, string greeting,
            List<FieldAnswer> answers, DocumentData[] docs)
        {
            var t = Asset<TravelerData>(travelerPath);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(TravelerPrefabBuilder.PrefabFolder + $"PF_NPC_{species}.prefab");
            var looks = AssetDatabase.LoadAssetAtPath<TravelerLooks>("Assets/_Project/Data/Travelers/TravelerLooks.asset");
            var photo = looks != null ? looks.PhotoFor(name, species) : null;
            if (photo != null)
                foreach (var doc in docs)
                    if (doc.Photo != null) doc.EditorSet(doc.Type, photo, new List<DocumentField>(doc.Fields), doc.ForgedSeal);
            for (int i = 0; i < docs.Length; i++)
            {
                var path = docPrefix + $"{docs[i].Type}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<DocumentData>(path);
                if (existing == null) { AssetFolders.EnsureForAsset(path); AssetDatabase.CreateAsset(docs[i], path); existing = docs[i]; }
                else { existing.EditorSet(docs[i].Type, docs[i].Photo, new List<DocumentField>(docs[i].Fields), docs[i].ForgedSeal); Object.DestroyImmediate(docs[i]); }
                EditorUtility.SetDirty(existing);
                docs[i] = existing;
            }
            t.EditorSet(name, species, new List<DocumentData>(docs), verdict, reason, tag, greeting, answers, model);
            string storyKey = System.IO.Path.GetFileNameWithoutExtension(travelerPath).Replace("T_Story_", "").Replace("T_Normal_", "");
            StoryDayContent.Reactions.TryGetValue(storyKey, out var reaction);
            t.EditorSetReactions(reaction.admitted, reaction.denied);
            EditorUtility.SetDirty(t);
            return t;
        }

        private static DocumentData Doc(DocumentType type, Texture2D photo, params (string, string)[] fields)
        {
            var list = new List<DocumentField>();
            foreach (var (k, v) in fields) list.Add(new DocumentField(k, v));
            var d = ScriptableObject.CreateInstance<DocumentData>();
            d.EditorSet(type, photo, list);
            return d;
        }

        private static Texture2D Photo(string species) => AssetDatabase.LoadAssetAtPath<Texture2D>($"{PhotoFolder}T_Doc_Photo_{species}.png");

        internal static DocumentData IdCard(string species, string name, string origin, string id, string expires = "2293-01-01") =>
            Doc(DocumentType.IdCard, Photo(species), ("name", name), ("species", species.ToUpperInvariant()), ("origin", origin), ("id", id), ("expires", expires));

        /// <summary>P62: a paper with a forged seal (only the UV lamp shows it).</summary>
        internal static DocumentData Forged(DocumentData doc)
        {
            doc.EditorSet(doc.Type, doc.Photo, new List<DocumentField>(doc.Fields), true);
            return doc;
        }

        internal static DocumentData PermitDoc(string species, string name, string id, string purpose, string expires, string issued, string route) =>
            Doc(DocumentType.EntryPermit, Photo(species), ("name", name), ("id", id), ("purpose", purpose), ("duration", "3 CYCLES"),
                ("expires", expires), ("issued", issued), ("route", route));

        internal static DocumentData Manifest(string name, string ship, string origin, string cargo) =>
            Doc(DocumentType.CargoManifest, null, ("name", name), ("ship", ship), ("origin", origin), ("destination", "KESTREL-9"), ("cargo", cargo));

        private static T Asset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            AssetFolders.EnsureForAsset(path);
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
