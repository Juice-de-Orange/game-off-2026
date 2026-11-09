namespace GameOff.Counter
{
    /// <summary>The four decisions the officer can make at the counter (see docs/DESIGN.md).</summary>
    public enum CounterAction
    {
        Approve,
        Deny,
        Arrest,
        Airlock,
        UvLamp,
        Scanner,
        /// <summary>P34: the CRT terminal in the desk (wanted list and entry record).</summary>
        Terminal
    }
}
