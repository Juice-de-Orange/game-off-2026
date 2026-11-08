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

        private static FieldAnswer Say(string key, string line) => new FieldAnswer(key, line);

        public static Dictionary<int, StoryDayContent> Build() => new Dictionary<int, StoryDayContent>
        {
            { 1, Day1() },
            { 2, Day2() },
            { 3, Day3() },
            { 4, Day4() },
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
                     "Pip asks if you can see us from up there.\n— Lena",
            LetterIfTrouble = "Dad,\nThe rent man came by and Mom paid him, but one of us is not well and the border is closed, so everything costs more.\n" +
                              "Please send what you can.\n— Lena",
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
            NewToday = "New today: quarantine at KETRA STATION. Stimulants stay forbidden.",
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
    }
}
