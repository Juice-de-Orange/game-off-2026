using System.Collections.Generic;
using GameOff.Documents;
using GameOff.Travelers;
using static GameOff.EditorTools.StoryCharacterBuilder;

namespace GameOff.EditorTools
{
    /// <summary>
    /// P15: the authored content of the story days, used by StoryCharacterBuilder: bulletin rules, wanted list, ordinary travelers,
    /// the morning letter (a second version for a family in trouble) and the "new today" line. Days without an entry here keep
    /// the placeholders of P76. The recurring characters stay in StoryCharacterBuilder; Day = 6 encounters including them.
    /// </summary>
    internal sealed class StoryDayContent
    {
        public List<DayRule> Rules;
        public List<WantedEntry> Wanted;
        public List<TravelerData> Travelers;
        public string Letter;
        public string LetterIfTrouble;
        public string NewToday;

        private const string Stimulant = "STIMULANT", Antibiotics = "ANTIBIOTICS";

        internal static FieldAnswer Say(string key, string line) => new FieldAnswer(key, line);

        /// <summary>P100: own lines after the stamp (admitted, denied), by asset file name; everybody else uses the species' pool.</summary>
        internal static readonly Dictionary<string, (string admitted, string denied)> Reactions = new Dictionary<string, (string, string)>
        {
            { "Irin", (null, "Orvan is all I have left. Do you even know where it is?") },
            { "Pela", (null, "A manifest? Nobody told me. Nobody ever tells me.") },
            { "Tzo", (null, "Kss. One new rule a day. I know your game.") },
            { "Oren", (null, "The list changes every week. I can't keep up with it.") },
            { "Kiss", ("Kss. Right. Efficient.", null) },
            { "Mira_1", (null, "Nine days. And you stamp it like it's nothing.") },
            { "Mira_2", ("Thank you. I won't forget it.", null) },
            { "Mira_3", (null, "Then there is nothing left to try.") },
            { "Tobin_2", (null, "It's one day old. One day! I have shifts to work.") },
            // P143: Korr's last words on day 1; the tutorial board closes with the decision, so these are all he says after it
            { "Korr_1", ("Good. Carry on.", "My papers were in order, officer. It's in my report. Carry on.") },
        };

        public static Dictionary<int, StoryDayContent> Build() => new Dictionary<int, StoryDayContent>
        {
            { 1, Day1() },
            { 2, Day2() },
            { 3, Day3() },
            { 4, Day4() },
            { 5, Day5() },
            { 6, Day6() },
            { 7, Day7() },
            { 8, Day8() },
            { 9, Day9() },
            { 10, Day10() },
            { 11, Day11() },
            { 12, Day12() },
        };

        private static StoryDayContent Day1() => new StoryDayContent
        {
            Rules = new List<DayRule> { new BannedOriginRule("ORVAN") },
            Wanted = new List<WantedEntry>(),
            NewToday = "New today: no entry from ORVAN.",
            Letter = "Dear Dad,\nMom says the heater makes the funny noise again. Pip drew you a picture of the station, it is in the next parcel.\n" +
                     "We miss you. Work hard and come home soon.\n— Lena",
            Travelers = new List<TravelerData>
            {
                Normal("Hana", "HANA ROEK", "Human", Verdict.Admit, "Papers in order", 1,
                    "First shift on the docks. Is it always this cold up here?",
                    new[] { Say("purpose", "Cargo handler. They promised heated boots."), Say("origin", "Torvik. You can smell it on my coat.") },
                    IdCard("Human", "HANA ROEK", "TORVIK COLONY", "HR-2204-11"),
                    PermitDoc("Human", "HANA ROEK", "HR-2204-11", "WORK", Valid, "ASKA-3 CONSULATE", "TORVIK > KESTREL-9")),
                Normal("Sudo", "SUDO VELK", "Vellani", Verdict.Deny, "Entry permit expired yesterday", 1,
                    "Trade visa. A quick stamp, yes? My ship is waiting.",
                    new[] { Say("expires", "Dates, dates. Close enough, surely."), Say("purpose", "I sell cables. Very good cables.") },
                    IdCard("Vellani", "SUDO VELK", "VELL PRIME", "VL-3120-07"),
                    PermitDoc("Vellani", "SUDO VELK", "VL-3120-07", "TRADE", Yesterday(1), "VELL PRIME CONSULATE", "VELL PRIME > KESTREL-9")),
                Normal("Ivo", "IVO TARSH", "Human", Verdict.Deny, "Origin ORVAN is banned today", 1,
                    "Please. I only want to see my brother.",
                    new[] { Say("origin", "Orvan. Since when is that a crime?"), Say("purpose", "He works in your hangar. Ask him.") },
                    IdCard("Human", "IVO TARSH", "ORVAN", "IT-8812-30"),
                    PermitDoc("Human", "IVO TARSH", "IT-8812-30", "FAMILY VISIT", Valid, "ASKA-3 CONSULATE", "ASKA-3 > KESTREL-9")),
            },
        };

        private static StoryDayContent Day2() => new StoryDayContent
        {
            Rules = new List<DayRule> { new BannedOriginRule("ORVAN"), new RequireDocumentRule(DocumentType.CargoManifest, "Vellani") },
            Wanted = new List<WantedEntry>(),
            NewToday = "New today: Vellani travelers need a cargo manifest.",
            Letter = "Dad,\nThe rent man came by and Mom paid him. They say the border is closed, so food costs more now. We are careful.\n" +
                     "Pip asks if you can see us from up there. Mrs Oren's son sends food on the cargo drone instead of money. Could you do that too?\n— Lena",
            LetterIfTrouble = "Dad,\nThe rent man came by and Mom paid him, but one of us is not well and the border is closed, so everything costs more.\n" +
                              "Please send what you can. Mrs Oren's son sends food on the cargo drone instead of money. Could you do that too?\n— Lena",
            Travelers = new List<TravelerData>
            {
                Normal("Nessa", "NESSA KILO", "Vellani", Verdict.Admit, "Papers in order, manifest present", 2,
                    "New paper rules again? Here, I brought everything.",
                    new[] { Say("cargo", "Batteries for the colony run. Nothing that bites."), Say("purpose", "Trade. Always trade.") },
                    IdCard("Vellani", "NESSA KILO", "VELL PRIME", "VL-4471-22"),
                    PermitDoc("Vellani", "NESSA KILO", "VL-4471-22", "TRADE", Valid, "VELL PRIME CONSULATE", "VELL PRIME > KESTREL-9"),
                    Manifest("NESSA KILO", "MARL SKIFF", "VELL PRIME", "3x BATTERY CRATE")),
                Normal("Bram", "BRAM OSTER", "Human", Verdict.Deny, "Name on the cargo manifest doesn't match the ID card", 2,
                    "Long queue today. Everyone's nervous about the border.",
                    new[] { Say("name", "Oster, Ostern, the clerk at Aska can't spell."), Say("cargo", "Rations. The border's closed, somebody has to eat.") },
                    IdCard("Human", "BRAM OSTER", "ASKA-3", "BO-6650-41"),
                    PermitDoc("Human", "BRAM OSTER", "BO-6650-41", "BUSINESS", Valid, "ASKA-3 CONSULATE", "ASKA-3 > KESTREL-9"),
                    Manifest("BRAM OSTERN", "THE LUCKY DUE", "ASKA-3", "2x RATION CRATE")),
                Normal("Xit", "XIT KOROL", "Xirrik", Verdict.Deny, "ID number on the entry permit doesn't match the ID card", 2,
                    "Kss. Numbers, numbers. Stamp, please.",
                    new[] { Say("id", "Kss. All the same number. Look again."), Say("route", "Straight from Lumen Reach. No stops.") },
                    IdCard("Xirrik", "XIT KOROL", "LUMEN REACH", "XR-2291-15"),
                    PermitDoc("Xirrik", "XIT KOROL", "XR-2219-15", "TRANSIT", Valid, "LUMEN REACH CONSULATE", "LUMEN REACH > KESTREL-9")),
                Normal("Pela", "PELA DRUN", "Vellani", Verdict.Deny, "Vellani traveler without a cargo manifest", 2,
                    "I only carry a bag. Why would I need a manifest?",
                    new[] { Say("purpose", "Visiting. Is one bag a cargo now?"), Say("origin", "Vell Prime, like every Vellani you'll send home today.") },
                    IdCard("Vellani", "PELA DRUN", "VELL PRIME", "VL-1183-60"),
                    PermitDoc("Vellani", "PELA DRUN", "VL-1183-60", "TRANSIT", Valid, "VELL PRIME CONSULATE", "VELL PRIME > KESTREL-9")),
            },
        };

        private static StoryDayContent Day3() => new StoryDayContent
        {
            Rules = new List<DayRule> { new ForbiddenGoodsRule(Stimulant), new ForbiddenGoodsRule(Antibiotics) }, // Dr. Senn's cargo needs an import licence today
            Wanted = new List<WantedEntry> { new WantedEntry("KAEL DROSS", "DX-0013-44", WantedOrder.Arrest, "Armed robbery, Aska-3") },
            NewToday = "New today: no stimulants, no antibiotics. Read the wanted list.",
            Letter = "Dad,\nIt got so cold last night that Pip slept in Mom's coat. The heater still works if you hit it.\n" +
                     "Tell us about the people you meet.\n— Lena",
            LetterIfTrouble = "Dad,\nIt got very cold and the house won't warm up. Being ill in this is hard. The clinic wants money first.\n" +
                              "Mom won't say it, so I will: we need medicine.\n— Lena",
            Travelers = new List<TravelerData>
            {
                Normal("Kael", "KAEL DROSS", "Human", Verdict.Arrest, "On the wanted list (arrest)", 3,
                    "Long trip. Let's keep this short.",
                    new[] { Say("name", "That's what it says, isn't it?"), Say("purpose", "Work. The kind that doesn't need a chat.") },
                    IdCard("Human", "KAEL DROSS", "ASKA-3", "DX-0013-44"),
                    PermitDoc("Human", "KAEL DROSS", "DX-0013-44", "WORK", Valid, "ASKA-3 CONSULATE", "ASKA-3 > KESTREL-9")),
                Normal("Tomi", "TOMI LARK", "Quobb", Verdict.Admit, "Stimulator pads are not stimulants: papers in order", 3,
                    "Everyone stares at my manifest today. It's for my back!",
                    new[] { Say("cargo", "Stimulator, not stimulant. Little pads. For my back. Everyone asks."), Say("origin", "Quob Atoll. Warmer than here.") },
                    IdCard("Quobb", "TOMI LARK", "QUOB ATOLL", "QU-3302-18"),
                    PermitDoc("Quobb", "TOMI LARK", "QU-3302-18", "FAMILY VISIT", Valid, "QUOB ATOLL CONSULATE", "QUOB ATOLL > KESTREL-9"),
                    Manifest("TOMI LARK", "SLOW TIDE", "QUOB ATOLL", "3x NERVE STIMULATOR PADS")),
                Normal("Roen", "ROEN HADDIX", "Sorvak", Verdict.Deny, "ID card expired yesterday", 3,
                    "Your heating is failing, officer. Mine is not. Proceed.",
                    new[] { Say("expires", "Dates are a formality. Any renewal takes eleven cycles on Sorv Deep."), Say("purpose", "Engineering contract. Your hull needs it.") },
                    IdCard("Sorvak", "ROEN HADDIX", "SORV DEEP", "SK-7715-33", Yesterday(3)),
                    PermitDoc("Sorvak", "ROEN HADDIX", "SK-7715-33", "WORK", Valid, "ASKA-3 CONSULATE", "SORV DEEP > KESTREL-9")),
            },
        };

        private static StoryDayContent Day4() => new StoryDayContent
        {
            Rules = new List<DayRule> { new ForbiddenGoodsRule(Stimulant), new QuarantineRule("KETRA STATION") },
            Wanted = new List<WantedEntry>(),
            NewToday = "New today: quarantine at KETRA STATION. Stimulants stay forbidden. Forged seals are turning up: the kiosk sells UV lamps.",
            Letter = "Dad,\nPip lost a tooth and wants you to have it. Mom said no. School is open again.\n" +
                     "We counted. It is too many days until you are home.\n— Lena",
            LetterIfTrouble = "Dad,\nNobody sleeps much here now. Pip keeps asking when you are coming.\n" +
                              "I said soon. Please don't make me a liar.\n— Lena",
            Travelers = new List<TravelerData>
            {
                Normal("Dela", "DELA MORN", "Human", Verdict.Airlock, "Route via quarantined KETRA STATION: airlock is mandatory", 4,
                    "Sorry. It's only a cough. Please, I just want to get home.",
                    new[] { Say("route", "We only refuelled at Ketra. Nobody even got off."), Say("purpose", "Going home. That's the whole plan.") },
                    IdCard("Human", "DELA MORN", "ASKA-3", "DM-5540-09"),
                    PermitDoc("Human", "DELA MORN", "DM-5540-09", "TRANSIT", Valid, "ASKA-3 CONSULATE", "ASKA-3 > KETRA STATION > KESTREL-9")),
                Normal("Jonas", "JONAS VARGA", "Human", Verdict.Arrest, "Declares stimulants, forbidden", 4,
                    "My mother says stay out of trouble. This isn't trouble, it's freight.",
                    new[] { Say("cargo", "Vials and a bit of liquor. Half the station buys it."), Say("name", "Varga. Don't tell my mother you saw me.") },
                    IdCard("Human", "JONAS VARGA", "TORVIK COLONY", "JV-3097-52"),
                    PermitDoc("Human", "JONAS VARGA", "JV-3097-52", "BUSINESS", Valid, "ASKA-3 CONSULATE", "TORVIK > KESTREL-9"),
                    Manifest("JONAS VARGA", "SECOND CHANCE", "TORVIK", "6x STIMULANT VIALS\n 2x LIQUOR CRATE")),
                Normal("Ola", "OLA TESSEN", "Xirrik", Verdict.Admit, "Papers in order", 4,
                    "Kss. My hatchlings are past this gate. Nine cycles I have not seen them.",
                    new[] { Say("purpose", "Kss. They will not know my face. I brought a picture of it."), Say("route", "Direct. I paid for the fast one.") },
                    IdCard("Xirrik", "OLA TESSEN", "LUMEN REACH", "XR-8826-44"),
                    PermitDoc("Xirrik", "OLA TESSEN", "XR-8826-44", "FAMILY VISIT", Valid, "LUMEN REACH CONSULATE", "LUMEN REACH > KESTREL-9")),
                Normal("Vosh", "VOSH ADEL", "Vellani", Verdict.Admit, "Papers in order", 4,
                    "Stew for the canteen. Your cook owes me money, tell him.",
                    new[] { Say("cargo", "Canned stew. Yes, all of it. Yes, again."), Say("purpose", "Trade, if you call stew trade.") },
                    IdCard("Vellani", "VOSH ADEL", "VELL PRIME", "VL-9034-71"),
                    PermitDoc("Vellani", "VOSH ADEL", "VL-9034-71", "TRADE", Valid, "VELL PRIME CONSULATE", "VELL PRIME > KESTREL-9"),
                    Manifest("VOSH ADEL", "BIG APPETITE", "VELL PRIME", "4x CANNED STEW CRATE")),
            },
        };

        private const string Orvan = "ORVAN", Ketra = "KETRA STATION";
        private const string AskaC = "ASKA-3 CONSULATE", VellC = "VELL PRIME CONSULATE", LumenC = "LUMEN REACH CONSULATE", QuobC = "QUOB ATOLL CONSULATE";

        private static StoryDayContent Day5() => new StoryDayContent
        {
            Rules = new List<DayRule> { new BannedOriginRule(Orvan), new ForbiddenGoodsRule("SPICE") },
            Wanted = new List<WantedEntry> { new WantedEntry("NESTOR KAI", "NK-4102-77", WantedOrder.Arrest, "Fraud, Lumen Reach") },
            NewToday = "New today: spice is forbidden. ORVAN stays closed.",
            Letter = "Dad,\nA man from the housing office wrote down how many of us live here. Mom told him not to worry about it.\nPip learned to whistle. It is all Pip does now.\n— Lena",
            LetterIfTrouble = "Dad,\nThe clinic has a list now and we are not near the top of it. Mom says the list is how they say no politely.\nDid the last parcel go out?\n— Lena",
            Travelers = new List<TravelerData>
            {
                Normal("Mako", "MAKO DRENN", "Human", Verdict.Admit, "Papers in order", 5,
                    "Back again. This station gets colder every month.",
                    new[] { Say("purpose", "Pipe fitter. Your pipes need me, believe me."), Say("origin", "Aska-3. Born there, leaving there.") },
                    IdCard("Human", "MAKO DRENN", "ASKA-3", "MD-3381-27"),
                    PermitDoc("Human", "MAKO DRENN", "MD-3381-27", "WORK", Valid, AskaC, "ASKA-3 > KESTREL-9")),
                Normal("Irin", "IRIN CALDE", "Human", Verdict.Deny, "Origin ORVAN is banned", 5,
                    "I left Orvan years ago. It only says so on the card.",
                    new[] { Say("origin", "A place on a card. I haven't been back in years."), Say("purpose", "Work. Any work.") },
                    IdCard("Human", "IRIN CALDE", Orvan, "IC-2217-64"),
                    PermitDoc("Human", "IRIN CALDE", "IC-2217-64", "WORK", Valid, AskaC, "ASKA-3 > KESTREL-9")),
                Normal("Bobo", "BOBO TILL", "Quobb", Verdict.Arrest, "Declares spice, forbidden today", 5,
                    "Only a little for cooking! A family recipe.",
                    new[] { Say("cargo", "Eight sacks is a little. We are a big family."), Say("route", "Straight here. Fresh is fresh.") },
                    IdCard("Quobb", "BOBO TILL", "QUOB ATOLL", "QU-7741-09"),
                    PermitDoc("Quobb", "BOBO TILL", "QU-7741-09", "TRADE", Valid, QuobC, "QUOB ATOLL > KESTREL-9"),
                    Manifest("BOBO TILL", "SALT DAUGHTER", "QUOB ATOLL", "8x SPICE SACK")),
                Normal("Nestor", "NESTOR KAI", "Human", Verdict.Arrest, "On the wanted list (arrest)", 5,
                    "A pleasure, officer. I'll be out of your way in a moment.",
                    new[] { Say("name", "Kai. Common name. Very common."), Say("purpose", "Consulting. The kind that pays in advance.") },
                    IdCard("Human", "NESTOR KAI", "LUMEN REACH", "NK-4102-77"),
                    PermitDoc("Human", "NESTOR KAI", "NK-4102-77", "BUSINESS", Valid, LumenC, "LUMEN REACH > KESTREL-9")),
                Normal("Zeth", "ZETH ORRIK", "Xirrik", Verdict.Arrest, "Forged seal on the entry permit", 5,
                    "Kss. Fresh from the office. Everyone knows that office.",
                    new[] { Say("expires", "Kss. Months yet. Read it."), Say("route", "Lumen Reach, as always.") },
                    IdCard("Xirrik", "ZETH ORRIK", "LUMEN REACH", "XR-3350-81"),
                    Forged(PermitDoc("Xirrik", "ZETH ORRIK", "XR-3350-81", "TRANSIT", Valid, LumenC, "LUMEN REACH > KESTREL-9"))),
            },
        };

        private static StoryDayContent Day6() => new StoryDayContent
        {
            Rules = new List<DayRule> { new ForbiddenGoodsRule(Stimulant), new RequireDocumentRule(DocumentType.CargoManifest, "Xirrik") },
            Wanted = new List<WantedEntry>(),
            NewToday = "New today: Xirrik travelers need a cargo manifest. No stimulants.",
            Letter = "Dad,\nThey showed the station on the news. Mom turned it off, then turned it back on to look for you.\nPip says hello. Well, Pip whistled.\n— Lena",
            LetterIfTrouble = "Dad,\nWe traded the good chairs for what we needed. We sit on crates now, it is fine.\nIf you can send anything this week, please do.\n— Lena",
            // P150: three, with Quill, Nell and the Shifter (or his accomplice, on the intended path) = 6 encounters
            Travelers = new List<TravelerData>
            {
                Normal("Kiss", "KISSA VEL", "Xirrik", Verdict.Admit, "Papers in order, manifest present", 6,
                    "Kss. The new paper is ready. I read the notices.",
                    new[] { Say("cargo", "Kss. Fuses. Your station eats them."), Say("purpose", "Trade. Kss.") },
                    IdCard("Xirrik", "KISSA VEL", "LUMEN REACH", "XR-1178-53"),
                    PermitDoc("Xirrik", "KISSA VEL", "XR-1178-53", "TRADE", Valid, LumenC, "LUMEN REACH > KESTREL-9"),
                    Manifest("KISSA VEL", "THIN REED", "LUMEN REACH", "5x SPARE FUSE BOX")),
                Normal("Tzo", "TZO ANKER", "Xirrik", Verdict.Deny, "Xirrik traveler without a cargo manifest", 6,
                    "Kss. Manifest? For what? I carry only myself.",
                    new[] { Say("purpose", "Kss. Visiting my brood."), Say("origin", "Kss. Lumen Reach. It is on the card.") },
                    IdCard("Xirrik", "TZO ANKER", "LUMEN REACH", "XR-6042-18"),
                    PermitDoc("Xirrik", "TZO ANKER", "XR-6042-18", "FAMILY VISIT", Valid, LumenC, "LUMEN REACH > KESTREL-9")),
                Normal("Rell", "RELL ASTA", "Vellani", Verdict.Arrest, "Declares stimulants, forbidden", 6,
                    "For the dock crews. They work double shifts now.",
                    new[] { Say("cargo", "Vials. The crews ask for them. I only carry."), Say("name", "Rell Asta. Ask anyone on the docks.") },
                    IdCard("Vellani", "RELL ASTA", "VELL PRIME", "VL-2288-46"),
                    PermitDoc("Vellani", "RELL ASTA", "VL-2288-46", "BUSINESS", Valid, VellC, "VELL PRIME > KESTREL-9"),
                    Manifest("RELL ASTA", "COLD MARGIN", "VELL PRIME", "12x STIMULANT VIALS")),
            },
        };

        private static StoryDayContent Day7() => new StoryDayContent
        {
            Rules = new List<DayRule> { new QuarantineRule(Ketra), new RequireDocumentRule(DocumentType.CargoManifest, "Quobb") },
            Wanted = new List<WantedEntry> { new WantedEntry("LIRA VOSS", "LV-9931-02", WantedOrder.Terminate, "Enemy agent. Terminate on sight.") },
            NewToday = "New today: Quobb travelers need a cargo manifest. Quarantine at KETRA STATION.",
            Letter = "Dad,\nI counted the money in the jar twice and it came out different, so I counted it a third time.\nMom says you will have enough for the ticket soon. Will you?\n— Lena",
            LetterIfTrouble = "Dad,\nMom sold her coat. She says it was too warm anyway.\nPlease come home soon. I am not asking for anything else.\n— Lena",
            // P150: four, with Tobin and the Shifter (or his accomplice, on the intended path) = 6 encounters
            Travelers = new List<TravelerData>
            {
                Normal("Lira", "LIRA VOSS", "Human", Verdict.Airlock, "On the wanted list: terminate on sight", 7,
                    "Please. Whatever they told you about me, I have children.",
                    new[] { Say("name", "It's my name. Is that what this is about?"), Say("purpose", "I just want to get through. Please.") },
                    IdCard("Human", "LIRA VOSS", "ASKA-3", "LV-9931-02"),
                    PermitDoc("Human", "LIRA VOSS", "LV-9931-02", "TRANSIT", Valid, AskaC, "ASKA-3 > KESTREL-9")),
                Normal("Odin", "ODIN FERRO", "Human", Verdict.Airlock, "Route via quarantined KETRA STATION: airlock is mandatory", 7,
                    "Ten hours in that shuttle. I feel fine, I promise.",
                    new[] { Say("route", "Ketra was just a stop. I never left the seat."), Say("purpose", "Work. They said it couldn't wait.") },
                    IdCard("Human", "ODIN FERRO", "TORVIK COLONY", "OF-1266-71"),
                    PermitDoc("Human", "ODIN FERRO", "OF-1266-71", "WORK", Valid, AskaC, "TORVIK > KETRA STATION > KESTREL-9")),
                Normal("Pung", "PUNG ARLO", "Quobb", Verdict.Deny, "Quobb traveler without a cargo manifest", 7,
                    "No cargo, no paper. That is logical, yes?",
                    new[] { Say("purpose", "I come to see the station. Is it not allowed to look?"), Say("route", "Direct. Very boring.") },
                    IdCard("Quobb", "PUNG ARLO", "QUOB ATOLL", "QU-5508-13"),
                    PermitDoc("Quobb", "PUNG ARLO", "QU-5508-13", "TOURISM", Valid, QuobC, "QUOB ATOLL > KESTREL-9")),
                Normal("Varn", "VARN TESK", "Sorvak", Verdict.Arrest, "Forged seal on the entry permit", 7,
                    "My clan's oldest died on this station. I bring the ashes home.",
                    new[] { Say("purpose", "Rites. It will not take long."), Say("origin", "Sorv Deep. We all go back.") },
                    IdCard("Sorvak", "VARN TESK", "SORV DEEP", "SK-8820-15"),
                    Forged(PermitDoc("Sorvak", "VARN TESK", "SK-8820-15", "FAMILY VISIT", Valid, AskaC, "SORV DEEP > KESTREL-9"))),
            },
        };

        private static StoryDayContent Day8() => new StoryDayContent
        {
            Rules = new List<DayRule> { new BannedOriginRule("HALDER"), new ForbiddenGoodsRule("FUSION"), new RequireDocumentRule(DocumentType.CargoManifest, "Sorvak") },
            Wanted = new List<WantedEntry>(),
            NewToday = "New today: no entry from HALDER, fusion cells are forbidden, Sorvak travelers need a cargo manifest.",
            Letter = "Dad,\nThere was a fight at the water line. Nobody we know. Mom keeps us in after dark now.\nPip drew the station again, with you in the window.\n— Lena",
            LetterIfTrouble = "Dad,\nThere was a fight at the water line and the shops are nearly empty. Everything takes longer now and nothing gets better.\nWrite to us, even a short one.\n— Lena",
            Travelers = new List<TravelerData>
            {
                Normal("Grom", "GROM ULLAR", "Sorvak", Verdict.Admit, "Papers in order, manifest present", 8,
                    "Papers complete. Inspect them and let me work.",
                    new[] { Say("cargo", "Batteries. Counted twice."), Say("purpose", "Contract work. Your strike is not my concern.") },
                    IdCard("Sorvak", "GROM ULLAR", "SORV DEEP", "SK-3307-59"),
                    PermitDoc("Sorvak", "GROM ULLAR", "SK-3307-59", "WORK", Valid, AskaC, "SORV DEEP > KESTREL-9"),
                    Manifest("GROM ULLAR", "IRON PATIENCE", "SORV DEEP", "4x BATTERY CRATE")),
                Normal("Sera", "SERA LOWE", "Human", Verdict.Deny, "Origin HALDER is banned today", 8,
                    "Halder was fine yesterday. What changed?",
                    new[] { Say("origin", "Halder. Since when is that a problem?"), Say("purpose", "My sister had a baby.") },
                    IdCard("Human", "SERA LOWE", "HALDER", "SL-7730-20"),
                    PermitDoc("Human", "SERA LOWE", "SL-7730-20", "FAMILY VISIT", Valid, AskaC, "ASKA-3 > KESTREL-9")),
                Normal("Yuna", "YUNA PRESS", "Vellani", Verdict.Arrest, "Declares fusion cells, forbidden today", 8,
                    "The strikers need power too, officer. Somebody has to bring it.",
                    new[] { Say("cargo", "Cells. For the families in the lower docks."), Say("name", "Yuna Press. You'll hear it again.") },
                    IdCard("Vellani", "YUNA PRESS", "VELL PRIME", "VL-6614-38"),
                    PermitDoc("Vellani", "YUNA PRESS", "VL-6614-38", "BUSINESS", Valid, VellC, "VELL PRIME > KESTREL-9"),
                    Manifest("YUNA PRESS", "LAST FAVOR", "VELL PRIME", "5x FUSION CELL CRATE")),
            },
        };

        private static StoryDayContent Day9() => new StoryDayContent
        {
            Rules = new List<DayRule>
            {
                new BannedOriginRule(Orvan), new ForbiddenGoodsRule(Stimulant), new ForbiddenGoodsRule("SPICE"), new QuarantineRule(Ketra),
                new RequireDocumentRule(DocumentType.CargoManifest, "Vellani"),
            },
            Wanted = new List<WantedEntry> { new WantedEntry("DARO HESK", "DH-5074-90", WantedOrder.Arrest, "Smuggling, Vell Prime") },
            NewToday = "Inspection week: every rule applies at once. Read the bulletin.",
            Letter = "Dad,\nThey say inspectors are at your station. Mom says that means you must be good at your job or in trouble.\nI said both.\n— Lena",
            LetterIfTrouble = "Dad,\nThe inspectors were on the news and all I could think about was the rent. Mom doesn't sleep. I do the counting now.\nPlease don't lose this job.\n— Lena",
            Travelers = new List<TravelerData>
            {
                Normal("Nimi", "NIMI SOLK", "Vellani", Verdict.Arrest, "Forged seal on the entry permit", 9,
                    "Everything is in order. Everyone has been so careful this week.",
                    new[] { Say("cargo", "Canned stew. Nothing that can be misread."), Say("purpose", "Trade, very quietly.") },
                    IdCard("Vellani", "NIMI SOLK", "VELL PRIME", "VL-4426-71"),
                    Forged(PermitDoc("Vellani", "NIMI SOLK", "VL-4426-71", "TRADE", Valid, VellC, "VELL PRIME > KESTREL-9")),
                    Manifest("NIMI SOLK", "SMALL MERCY", "VELL PRIME", "3x CANNED STEW CRATE")),
                Normal("Oren", "OREN PADD", "Vellani", Verdict.Deny, "Vellani traveler without a cargo manifest", 9,
                    "I carry nothing, I declare nothing. Isn't that the point?",
                    new[] { Say("purpose", "A visit. Short, if you allow it."), Say("route", "Direct from home.") },
                    IdCard("Vellani", "OREN PADD", "VELL PRIME", "VL-8253-07"),
                    PermitDoc("Vellani", "OREN PADD", "VL-8253-07", "TRANSIT", Valid, VellC, "VELL PRIME > KESTREL-9")),
                Normal("Cato", "CATO BRILL", "Human", Verdict.Arrest, "Declares spice, forbidden", 9,
                    "Inspection week, I know. Which is why nobody will look at spice.",
                    new[] { Say("cargo", "It's for the officers' mess. Ask them."), Say("name", "Brill. You'll want to remember it kindly.") },
                    IdCard("Human", "CATO BRILL", "TORVIK COLONY", "CB-3145-62"),
                    PermitDoc("Human", "CATO BRILL", "CB-3145-62", "BUSINESS", Valid, AskaC, "TORVIK > KESTREL-9"),
                    Manifest("CATO BRILL", "GOOD TERMS", "TORVIK", "4x SPICE SACK")),
                Normal("Ansa", "ANSA REEVE", "Human", Verdict.Airlock, "Route via quarantined KETRA STATION: airlock is mandatory", 9,
                    "It's only a rash. The doctor at Ketra said so.",
                    new[] { Say("route", "Ketra was the cheapest way. Is it not allowed?"), Say("purpose", "Going to my mother. She is old.") },
                    IdCard("Human", "ANSA REEVE", "ASKA-3", "AR-6698-23"),
                    PermitDoc("Human", "ANSA REEVE", "AR-6698-23", "FAMILY VISIT", Valid, AskaC, "ASKA-3 > KETRA STATION > KESTREL-9")),
                Normal("Daro", "DARO HESK", "Human", Verdict.Arrest, "On the wanted list (arrest)", 9,
                    "Quiet week. Let's keep it that way, you and me.",
                    new[] { Say("name", "Daro. Just Daro, between friends."), Say("purpose", "Business. The usual kind.") },
                    IdCard("Human", "DARO HESK", "VELL PRIME", "DH-5074-90"),
                    PermitDoc("Human", "DARO HESK", "DH-5074-90", "BUSINESS", Valid, VellC, "VELL PRIME > KESTREL-9")),
                Normal("Hasz", "HASZ URR", "Sorvak", Verdict.Deny, "Origin ORVAN is banned", 9,
                    "Orvan is my birth clan. I have never set foot there.",
                    new[] { Say("origin", "Clan record. Nothing more."), Say("purpose", "Work. As ordered.") },
                    IdCard("Sorvak", "HASZ URR", Orvan, "SK-1937-66"),
                    PermitDoc("Sorvak", "HASZ URR", "SK-1937-66", "WORK", Valid, AskaC, "ASKA-3 > KESTREL-9")),
            },
        };

        private static StoryDayContent Day10() => new StoryDayContent
        {
            Rules = new List<DayRule> { new ForbiddenGoodsRule("PISTOL"), new ForbiddenGoodsRule("WEAPON"), new RequireDocumentRule(DocumentType.CargoManifest, "Xirrik") },
            Wanted = new List<WantedEntry> { new WantedEntry("GRAL OTHEN", "GO-2810-44", WantedOrder.Arrest, "Armed assault, Halder") },
            NewToday = "New today: weapons are forbidden. Xirrik travelers need a cargo manifest.",
            Letter = "Dad,\nI am taller than Mom now. She measured us on the door frame and then went quiet.\nThere is a mark there for you too, from before.\n— Lena",
            LetterIfTrouble = "Dad,\nI stopped going to school so I can queue for things. Don't be angry. Somebody has to.\nHow many more days?\n— Lena",
            Travelers = new List<TravelerData>
            {
                Normal("Issk", "ISSK DORR", "Xirrik", Verdict.Admit, "Papers in order, manifest present", 10,
                    "Kss. All in order. I learn your rules quickly.",
                    new[] { Say("cargo", "Kss. Painkillers. Half the station has a headache."), Say("purpose", "Kss. Trade.") },
                    IdCard("Xirrik", "ISSK DORR", "LUMEN REACH", "XR-9175-30"),
                    PermitDoc("Xirrik", "ISSK DORR", "XR-9175-30", "TRADE", Valid, LumenC, "LUMEN REACH > KESTREL-9"),
                    Manifest("ISSK DORR", "PALE HOUR", "LUMEN REACH", "6x PAINKILLERS BOX")),
                Normal("Bartok", "BARTOK SAYE", "Human", Verdict.Arrest, "Declares pistols, forbidden today", 10,
                    "For personal safety. Have you seen this place lately?",
                    new[] { Say("cargo", "Two pistols. It's a dangerous route."), Say("purpose", "Security work. Private.") },
                    IdCard("Human", "BARTOK SAYE", "ASKA-3", "BS-4471-58"),
                    PermitDoc("Human", "BARTOK SAYE", "BS-4471-58", "WORK", Valid, AskaC, "ASKA-3 > KESTREL-9"),
                    Manifest("BARTOK SAYE", "LOW PROFILE", "ASKA-3", "2x PISTOL CASE")),
                Normal("Gral", "GRAL OTHEN", "Human", Verdict.Arrest, "On the wanted list (arrest)", 10,
                    "Let's not make this hard for either of us.",
                    new[] { Say("name", "You read it fine. Move on."), Say("purpose", "None of yours.") },
                    IdCard("Human", "GRAL OTHEN", "HALDER", "GO-2810-44"),
                    PermitDoc("Human", "GRAL OTHEN", "GO-2810-44", "BUSINESS", Valid, AskaC, "ASKA-3 > KESTREL-9")),
                Normal("Lumi", "LUMI VASK", "Quobb", Verdict.Deny, "ID number on the entry permit doesn't match the ID card", 10,
                    "So many numbers! I copied them myself, very carefully.",
                    new[] { Say("id", "I wrote it myself. Is it not the same?"), Say("purpose", "To see the big station! And my cousin.") },
                    IdCard("Quobb", "LUMI VASK", "QUOB ATOLL", "QU-6620-75"),
                    PermitDoc("Quobb", "LUMI VASK", "QU-6620-57", "FAMILY VISIT", Valid, QuobC, "QUOB ATOLL > KESTREL-9")),
            },
        };

        private static StoryDayContent Day11() => new StoryDayContent
        {
            Rules = new List<DayRule> { new BannedOriginRule(Orvan), new QuarantineRule("MERIDIAN GATE") },
            Wanted = new List<WantedEntry>(),
            NewToday = "New today: quarantine at MERIDIAN GATE. ORVAN stays closed.",
            Letter = "Dad,\nThe heater stopped making the funny noise. It also stopped working, but Mom fixed it with a spoon.\nWe saved you the good bowl.\n— Lena",
            LetterIfTrouble = "Dad,\nIt is very quiet here now. Mom says we should not count the days, so I count them where she can't see.\nPlease be careful up there.\n— Lena",
            Travelers = new List<TravelerData>
            {
                Normal("Tess", "TESS ORLO", "Human", Verdict.Admit, "Papers in order", 11,
                    "Last shuttle before the holiday. Lucky me.",
                    new[] { Say("purpose", "Going home. Finally."), Say("route", "Torvik to here. No stops, thank god.") },
                    IdCard("Human", "TESS ORLO", "TORVIK COLONY", "TO-8804-16"),
                    PermitDoc("Human", "TESS ORLO", "TO-8804-16", "TRANSIT", Valid, AskaC, "TORVIK > KESTREL-9")),
                Normal("Jago", "JAGO PERRIN", "Human", Verdict.Airlock, "Route via quarantined MERIDIAN GATE: airlock is mandatory", 11,
                    "Meridian was fine when I left it. They don't tell you anything out there.",
                    new[] { Say("route", "Meridian Gate. Everyone goes through Meridian."), Say("purpose", "Work. Same as you.") },
                    IdCard("Human", "JAGO PERRIN", "ASKA-3", "JP-2963-48"),
                    PermitDoc("Human", "JAGO PERRIN", "JP-2963-48", "WORK", Valid, AskaC, "ASKA-3 > MERIDIAN GATE > KESTREL-9")),
                Normal("Fenn", "FENN CARO", "Vellani", Verdict.Deny, "Entry permit expired yesterday", 11,
                    "One day. In all these years, one day.",
                    new[] { Say("expires", "A single day, officer. Dates are cruel."), Say("purpose", "Trade. It's all I know.") },
                    IdCard("Vellani", "FENN CARO", "VELL PRIME", "VL-7702-29"),
                    PermitDoc("Vellani", "FENN CARO", "VL-7702-29", "TRADE", Yesterday(11), VellC, "VELL PRIME > KESTREL-9")),
                Normal("Ombo", "OMBO LAIN", "Quobb", Verdict.Deny, "Origin ORVAN is banned", 11,
                    "I am Quobb! Only the hatchery was on Orvan.",
                    new[] { Say("origin", "The hatchery, yes. A very small one."), Say("purpose", "To sell songs.") },
                    IdCard("Quobb", "OMBO LAIN", Orvan, "QU-1849-52"),
                    PermitDoc("Quobb", "OMBO LAIN", "QU-1849-52", "TOURISM", Valid, QuobC, "QUOB ATOLL > KESTREL-9")),
                Normal("Sirr", "SIRR TAAL", "Xirrik", Verdict.Arrest, "Forged seal on the entry permit", 11,
                    "Kss. You look tired, officer. Your kind sleeps, yes?",
                    new[] { Say("purpose", "Kss. A small errand."), Say("origin", "Kss. Lumen Reach. Warm there.") },
                    IdCard("Xirrik", "SIRR TAAL", "LUMEN REACH", "XR-2506-97"),
                    Forged(PermitDoc("Xirrik", "SIRR TAAL", "XR-2506-97", "TRANSIT", Valid, LumenC, "LUMEN REACH > KESTREL-9"))),
            },
        };

        private static StoryDayContent Day12() => new StoryDayContent
        {
            Rules = new List<DayRule>
            {
                new BannedOriginRule(Orvan), new ForbiddenGoodsRule(Stimulant), new ForbiddenGoodsRule("CASH"), new QuarantineRule(Ketra),
                new RequireDocumentRule(DocumentType.CargoManifest, "Human"),
            },
            Wanted = new List<WantedEntry>(),
            NewToday = "New today: human travelers need a cargo manifest. No cash shipments.",
            Letter = "Dad,\nMom says the shuttle home leaves whenever you are ready. I told Pip it could be any day now.\nPip stopped whistling and sat by the window.\n— Lena",
            LetterIfTrouble = "Dad,\nI don't know how to write this one. Please just come home.\n— Lena",
            Travelers = new List<TravelerData>
            {
                Normal("Wren", "WREN ALDER", "Human", Verdict.Admit, "Papers in order, manifest present", 12,
                    "One more delivery and I retire. I've said that for ten years.",
                    new[] { Say("cargo", "Rations. Always rations."), Say("purpose", "Business, one last time.") },
                    IdCard("Human", "WREN ALDER", "TORVIK COLONY", "WA-6312-05"),
                    PermitDoc("Human", "WREN ALDER", "WA-6312-05", "BUSINESS", Valid, AskaC, "TORVIK > KESTREL-9"),
                    Manifest("WREN ALDER", "OLD HABIT", "TORVIK", "6x RATION CRATE")),
                Normal("Tomas", "TAVIN ROSK", "Human", Verdict.Deny, "Human traveler without a cargo manifest", 12,
                    "A manifest? For a coat and a sandwich?",
                    new[] { Say("purpose", "Visiting. Or I was."), Say("origin", "Aska-3. It's on the card.") },
                    IdCard("Human", "TAVIN ROSK", "ASKA-3", "TR-4085-61"),
                    PermitDoc("Human", "TAVIN ROSK", "TR-4085-61", "FAMILY VISIT", Valid, AskaC, "ASKA-3 > KESTREL-9")),
                Normal("Sava", "SAVA MOOR", "Vellani", Verdict.Arrest, "Declares cash shipment, forbidden today", 12,
                    "Wages for the dock crews. They are owed.",
                    new[] { Say("cargo", "Cash. It has to arrive today or they don't eat."), Say("name", "Moor. I keep the crews' books.") },
                    IdCard("Vellani", "SAVA MOOR", "VELL PRIME", "VL-3390-84"),
                    PermitDoc("Vellani", "SAVA MOOR", "VL-3390-84", "BUSINESS", Valid, VellC, "VELL PRIME > KESTREL-9"),
                    Manifest("SAVA MOOR", "HONEST COUNT", "VELL PRIME", "3x CASH BOX")),
                Normal("Korin", "KORIN DAHL", "Sorvak", Verdict.Airlock, "Route via quarantined KETRA STATION: airlock is mandatory", 12,
                    "We do not fall ill. Your rule is for weaker species.",
                    new[] { Say("route", "Ketra. A stop like any other."), Say("purpose", "Contract work.") },
                    IdCard("Sorvak", "KORIN DAHL", "SORV DEEP", "SK-5247-39"),
                    PermitDoc("Sorvak", "KORIN DAHL", "SK-5247-39", "WORK", Valid, AskaC, "SORV DEEP > KETRA STATION > KESTREL-9")),
                Normal("Xenn", "XENN IMRA", "Xirrik", Verdict.Deny, "Name on the entry permit doesn't match the ID card", 12,
                    "Kss. Your letters are difficult. It is nearly the same.",
                    new[] { Say("name", "Kss. Imra, Imrah. A sound is a sound."), Say("purpose", "Kss. To trade, and to leave.") },
                    IdCard("Xirrik", "XENN IMRA", "LUMEN REACH", "XR-7133-26"),
                    PermitDoc("Xirrik", "XENN IMRAH", "XR-7133-26", "TRADE", Valid, LumenC, "LUMEN REACH > KESTREL-9")),
            },
        };
    }
}
