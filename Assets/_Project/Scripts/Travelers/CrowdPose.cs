namespace GameOff.Travelers
{
    /// <summary>
    /// P134: how an evening guest holds themselves at their spot (EveningCrowd). P145: Sit plays the "Sit" state (A_NPC_Sit, added by
    /// Build Travelers) on the spot's seat; a figure without it never takes a sitting spot, it gets the next standing one.
    /// </summary>
    public enum CrowdPose
    {
        Stand,
        Sit,
    }
}
