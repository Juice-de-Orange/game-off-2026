using System;

namespace GameOff.Travelers
{
    /// <summary>What a traveler says when the officer asks about a document field (P40): field key → spoken line.</summary>
    [Serializable]
    public struct FieldAnswer
    {
        public string key;
        public string line;

        public FieldAnswer(string key, string line)
        {
            this.key = key;
            this.line = line;
        }
    }
}
