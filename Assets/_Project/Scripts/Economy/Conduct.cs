using GameOff.Travelers;

namespace GameOff.Economy
{
    /// <summary>
    /// P08: the officer's record over the whole run – disciplinary penalties (separate from the daily warnings of P07; at the limit
    /// the run ends) and the hidden morale, which the endings (P12) read. P113: today's airlock uses, for the mood lines (MoraleTuning).
    /// </summary>
    public class Conduct
    {
        public int Disciplinary { get; private set; }
        public int Morale { get; private set; }
        public int AirlocksToday { get; private set; }
        /// <summary>The part of the morale that comes from the airlock: what the airlock lines (P12, P113) may talk about. P35 theft only lowers Morale.</summary>
        public int AirlockMorale { get; private set; }
        /// <summary>Of today's airlock uses, those on someone who didn't deserve it.</summary>
        public int InnocentToday { get; private set; }
        /// <summary>Today's innocent victim if there was one, else today's last one.</summary>
        public string AirlockedName { get; private set; } = "";

        public bool Discharged(ConfiscationTuning tuning) => tuning.disciplinaryLimit > 0 && Disciplinary >= tuning.disciplinaryLimit;

        public void AddDisciplinary(int n) => Disciplinary += n;

        /// <summary>P35: morale change without an airlock.</summary>
        public void AddMorale(int delta) => Morale += delta;

        /// <summary>P128: a saved run continues with this record (today's airlock counts start fresh with the day).</summary>
        public void Restore(int disciplinary, int morale, int airlockMorale)
        {
            Disciplinary = disciplinary;
            Morale = morale;
            AirlockMorale = airlockMorale;
            BeginDay();
        }

        public void BeginDay()
        {
            AirlocksToday = InnocentToday = 0;
            AirlockedName = "";
        }

        /// <summary>Morale change of one decision; returns it (0 for anything but the airlock).</summary>
        public int Judge(Verdict chosen, Judgement judgement, ConfiscationTuning tuning, string name = "")
        {
            if (chosen != Verdict.Airlock) return 0;
            bool deserved = judgement.CorrectVerdict == Verdict.Airlock || judgement.CorrectVerdict == Verdict.Arrest;
            int delta = deserved ? tuning.airlockGuilty : tuning.airlockInnocent;
            Morale += delta;
            AirlockMorale += delta;
            AirlocksToday++;
            if (!deserved) InnocentToday++;
            if (!deserved || InnocentToday == 0) AirlockedName = name;
            return delta;
        }
    }
}
