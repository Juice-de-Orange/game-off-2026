using System.Collections.Generic;

namespace GameOff.UI
{
    /// <summary>
    /// P140: the keys behind [H], one list per situation. Only real bindings: PlayerControls.inputactions (Player: WASD/arrows,
    /// mouse, E; Counter: Q, Esc, right click, left click), InventoryWindow (I), PauseMenu (Esc, P), CounterStation (R),
    /// HallBench (F), SeatView / CoinTelescope (mouse, WASD/arrows, wheel, Q, Esc), PinBoardCloseUp (E, Esc),
    /// ShuttleFlight (W/S, A/D, arrows, left Shift).
    /// </summary>
    public static class HelpKeys
    {
        /// <summary>The only hint left at the bottom of the screen.</summary>
        public const string Prompt = "[H]  Help";
        public const string Footer = "[H] or [Esc]  close help";

        private static readonly (string keys, string action)[] Walking =
        {
            ("[W] [A] [S] [D] / arrows", "Walk"),
            ("Mouse", "Look around"),
            ("[E]", "Use what you look at"),
            ("[I]", "Inventory"),
            ("[Esc] / [P]", "Pause"),
        };

        private static readonly (string keys, string action)[] Counter =
        {
            ("Click", "Pick up a stamp or paper, press a button"),
            ("Click a stamp, then a paper", "Stamp it"),
            ("Click a stamp, then the desk", "Set it down there"),
            ("Drag", "Move papers, notes and the envelope"),
            ("Drag papers into the drawer", "Hand them back"),
            ("Click the CRT", "Terminal"),
            ("[R]", "Hold up all papers"),
            ("[A] [D] / left / right", "Look along the desk"),
            ("[I]", "Goods confiscated this shift"),
            ("[Q] / [Esc] / right click", "Put back, then stand up"),
        };

        private static readonly (string keys, string action)[] ReadingPapers =
        {
            ("Click a field", "Ask the traveler about it"),
            ("Click another paper", "Read it side by side"),
            ("Click a held paper", "Put it back"),
            ("[R]", "Put all papers down"),
            ("[Q] / [Esc] / right click", "Put all papers down"),
        };

        private static readonly (string keys, string action)[] Terminal =
        {
            ("Click", "Choose a query"),
            ("[Q] / [Esc] / right click", "Close the terminal"),
        };

        private static readonly (string keys, string action)[] PinBoard =
        {
            ("[E] / [Esc]", "Put it away"),
            ("Click CLOSE", "Put it away"),
        };

        private static readonly (string keys, string action)[] RestSeat =
        {
            ("Mouse or [W] [A] [S] [D] / arrows", "Look around"),
            ("Click", "Catch the mouse again"),
            ("[Q] / [Esc]", "Stand up, step back"),
        };

        private static readonly (string keys, string action)[] BenchEvening =
        {
            ("Mouse or [W] [A] [S] [D] / arrows", "Look around"),
            ("[F]", "Wait until late"),
            ("[Q] / [Esc]", "Stand up"),
        };

        private static readonly (string keys, string action)[] CloseLook =
        {
            ("[Q] / [Esc]", "Put it down, step back"),
        };

        private static readonly (string keys, string action)[] Telescope =
        {
            ("Mouse or [W] [A] [S] [D] / arrows", "Pan"),
            ("Mouse wheel", "Zoom"),
            ("Click", "Catch the mouse again"),
            ("[Q] / [Esc]", "Step back"),
        };

        private static readonly (string keys, string action)[] Flight =
        {
            ("[W] / [S] or up / down", "Thrust, brake"),
            ("Mouse or [A] [D] / left / right", "Steer"),
            ("[Shift]", "Boost"),
            ("Click", "Catch the mouse again"),
        };

        private static readonly (string keys, string action)[] Inventory =
        {
            ("Click an item", "Select it"),
            ("[I] / [Esc]", "Close"),
        };

        private static readonly (string keys, string action)[] Trade =
        {
            ("Click an item", "Select it"),
            ("[Esc]", "Close"),
        };

        private static readonly (string keys, string action)[] Nothing = { };

        /// <summary>The innermost thing the player is doing wins: a window over the counter beats the counter, and so on.</summary>
        public static HelpSituation SituationFor(HelpContext c)
        {
            if (c.InMenu || c.Ending || c.PanelOpen || c.Paused) return HelpSituation.None;
            if (c.Flying) return HelpSituation.Flight;
            if (c.Telescope) return HelpSituation.Telescope;
            if (c.PinBoard) return HelpSituation.PinBoard;
            if (c.Trade) return HelpSituation.Trade;
            if (c.Inventory) return HelpSituation.Inventory;
            if (c.Terminal) return HelpSituation.Terminal;
            if (c.AtCounter) return c.ReadingPapers ? HelpSituation.ReadingPapers : HelpSituation.Counter;
            switch (c.Seat)
            {
                case HelpContext.SeatKind.LookAround: return HelpSituation.RestSeat;
                case HelpContext.SeatKind.LookAroundEvening: return HelpSituation.BenchEvening;
                case HelpContext.SeatKind.CloseLook: return HelpSituation.CloseLook;
            }
            return HelpSituation.Walking;
        }

        public static IReadOnlyList<(string keys, string action)> Lines(HelpSituation situation) => situation switch
        {
            HelpSituation.Walking => Walking,
            HelpSituation.Counter => Counter,
            HelpSituation.ReadingPapers => ReadingPapers,
            HelpSituation.Terminal => Terminal,
            HelpSituation.PinBoard => PinBoard,
            HelpSituation.RestSeat => RestSeat,
            HelpSituation.BenchEvening => BenchEvening,
            HelpSituation.CloseLook => CloseLook,
            HelpSituation.Telescope => Telescope,
            HelpSituation.Flight => Flight,
            HelpSituation.Inventory => Inventory,
            HelpSituation.Trade => Trade,
            _ => Nothing,
        };

        public static string Title(HelpSituation situation) => situation switch
        {
            HelpSituation.Walking => "ON FOOT",
            HelpSituation.Counter => "AT THE COUNTER",
            HelpSituation.ReadingPapers => "READING PAPERS",
            HelpSituation.Terminal => "TERMINAL",
            HelpSituation.PinBoard => "PINBOARD",
            HelpSituation.RestSeat => "SITTING",
            HelpSituation.BenchEvening => "ON THE BENCH",
            HelpSituation.CloseLook => "A CLOSER LOOK",
            HelpSituation.Telescope => "TELESCOPE",
            HelpSituation.Flight => "FLYING HOME",
            HelpSituation.Inventory => "INVENTORY",
            HelpSituation.Trade => "TRADE",
            _ => "",
        };

        /// <summary>Open state after this frame: [H] opens, [H] or Esc closes; where there is no help it is closed.</summary>
        public static bool NextOpen(bool open, bool available, bool helpPressed, bool escapePressed)
        {
            if (!available) return false;
            return open ? !(helpPressed || escapePressed) : helpPressed;
        }
    }
}
