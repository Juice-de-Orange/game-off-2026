using GameOff.Travelers;

namespace GameOff.Economy
{
    /// <summary>
    /// P08: the officer's record over the whole run – disciplinary penalties (separate from the daily warnings of P07; at the limit
    /// the run ends) and the hidden morale, which only the endings (P12) read.
    /// </summary>
    public class Conduct
    {
        public int Disciplinary { get; private set; }
        public int Morale { get; private set; }

        public bool Discharged(ConfiscationTuning tuning) => tuning.disciplinaryLimit > 0 && Disciplinary >= tuning.disciplinaryLimit;

        public void AddDisciplinary(int n) => Disciplinary += n;

        /// <summary>Morale change of one decision; returns it (0 for anything but the airlock).</summary>
        public int Judge(Verdict chosen, Judgement judgement, ConfiscationTuning tuning)
        {
            if (chosen != Verdict.Airlock) return 0;
            bool deserved = judgement.CorrectVerdict == Verdict.Airlock || judgement.CorrectVerdict == Verdict.Arrest;
            int delta = deserved ? tuning.airlockGuilty : tuning.airlockInnocent;
            Morale += delta;
            return delta;
        }
    }
}
