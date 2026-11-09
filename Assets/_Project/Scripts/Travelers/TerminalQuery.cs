namespace GameOff.Travelers
{
    /// <summary>P34: one value the officer can look up on the counter terminal: a name or an ID number printed on the traveler's papers.</summary>
    public readonly struct TerminalQuery
    {
        public enum Kind { Name, Id }

        public readonly Kind Field;
        public readonly string Value;

        public TerminalQuery(Kind field, string value)
        {
            Field = field;
            Value = value;
        }

        public string Label => Field == Kind.Name ? "NAME" : "ID NO.";

        public override string ToString() => $"{Label} {Value}";
    }
}
