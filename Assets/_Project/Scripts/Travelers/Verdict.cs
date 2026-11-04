namespace GameOff.Travelers
{
    /// <summary>
    /// The officer's decision. The order is the severity: when several problems apply, the most severe one decides
    /// (mandatory airlock &gt; arrest &gt; deny &gt; admit).
    /// </summary>
    public enum Verdict
    {
        Admit = 0,
        Deny = 1,
        Arrest = 2,
        Airlock = 3
    }
}
