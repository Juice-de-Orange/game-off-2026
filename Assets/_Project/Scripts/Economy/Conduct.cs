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
        /// <summary>Airlock uses over the whole run: the ticket ending's closing sentence (P12) reads these, not the morale sum.</summary>
        public int Airlocks { get; private set; }
        /// <summary>Of the run's airlock uses, those on someone who didn't deserve it (the correct verdict was neither airlock nor arrest).</summary>
        public int InnocentAirlocks { get; private set; }
        /// <summary>Today's innocent victim if there was one, else today's last one.</summary>
        public string AirlockedName { get; private set; } = "";

        public bool Discharged(ConfiscationTuning tuning) => tuning.disciplinaryLimit > 0 && Disciplinary >= tuning.disciplinaryLimit;

        public void AddDisciplinary(int n) => Disciplinary += n;

        /// <summary>P35: morale change without an airlock.</summary>
        public void AddMorale(int delta) => Morale += delta;

        /// <summary>P128: a saved run continues with this record (today's airlock counts start fresh with the day).
        /// Saves from before the run counts have none; an airlock morale below 0 then still means at least one airlock.</summary>
        public void Restore(int disciplinary, int morale, int airlockMorale, int airlocks = 0, int innocentAirlocks = 0)
        {
            Disciplinary = disciplinary;
            Morale = morale;
            AirlockMorale = airlockMorale;
            InnocentAirlocks = innocentAirlocks;
            Airlocks = System.Math.Max(airlocks, innocentAirlocks);
            if (Airlocks == 0 && airlockMorale < 0) Airlocks = 1;
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
            Airlocks++;
            if (!deserved)
            {
                InnocentToday++;
                InnocentAirlocks++;
            }
            if (!deserved || InnocentToday == 0) AirlockedName = name;
            return delta;
        }
    }
}
