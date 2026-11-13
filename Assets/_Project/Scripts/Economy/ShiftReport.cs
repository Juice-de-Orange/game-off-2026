using System.Collections.Generic;

namespace GameOff.Economy
{
    /// <summary>P08: the three answers at shift end about what was confiscated (call 07.11.).</summary>
    public enum ShiftChoice
    {
        /// <summary>Keep everything secretly; often caught (fine + disciplinary penalty, goods taken back).</summary>
        NothingToReport,
        /// <summary>Keep one piece of one item; sometimes caught (fine only, the piece is taken back).</summary>
        MissedThat,
        /// <summary>Hand everything in to the evidence locker; no risk.</summary>
        AsOrdered,
    }

    /// <summary>What a shift-end answer leads to. Kept items are the CONFISCATED variants (P82).</summary>
    public class ShiftOutcome
    {
        public readonly List<(ItemData item, int count)> Kept = new List<(ItemData, int)>();
        public bool Caught;
        public int Fine;
        public int Disciplinary;
        public string Text = "";
    }

    /// <summary>
    /// P08 rules for the shift-end answer, without Unity (EditMode-tested). roll is a random number in [0, 1);
    /// "caught" means roll &lt; catch chance.
    /// </summary>
    public static class ShiftReport
    {
        public static ShiftOutcome Resolve(ShiftChoice choice, IReadOnlyList<(ItemData item, int count)> confiscated, ItemData picked,
            ConfiscationTuning tuning, float roll)
        {
            var o = new ShiftOutcome();
            switch (choice)
            {
                case ShiftChoice.NothingToReport:
                    if (roll < tuning.keepAllCaughtChance)
                    {
                        o.Caught = true;
                        o.Fine = tuning.keepAllFine;
                        o.Disciplinary = 1;
                        o.Text = "Security searched your locker on the way out. Everything is gone, and so is your clean record.";
                    }
                    else
                    {
                        foreach (var (item, count) in confiscated) o.Kept.Add((Variant(item), count));
                        o.Text = "Nobody asked. It's all in your bag now.";
                    }
                    break;
                case ShiftChoice.MissedThat:
                    if (picked == null) goto default;
                    if (roll < 1f - tuning.missedSuccessChance)
                    {
                        o.Caught = true;
                        o.Fine = tuning.missedFine;
                        o.Text = "\"Missed that, did you?\" The supervisor takes it and writes you a fine.";
                    }
                    else
                    {
                        o.Kept.Add((Variant(picked), 1));
                        o.Text = $"{picked.DisplayName} ×1 slipped into your pocket. The rest was handed in.";
                    }
                    break;
                default:
                    o.Text = confiscated.Count > 0 ? "Everything was handed in. By the book." : "Nothing to hand in.";
                    break;
            }
            return o;
        }

        private static ItemData Variant(ItemData item) => item.ConfiscatedVariant != null ? item.ConfiscatedVariant : item;
    }
}
