using System;

namespace GameOff.Counter
{
    /// <summary>P100 and P101: what the travelers of one species say when turned away, when let through, and later in the station.</summary>
    [Serializable]
    public class SpeciesLines
    {
        public string species;
        public string[] denied;
        public string[] admitted;
        public string[] evening;
    }
}
