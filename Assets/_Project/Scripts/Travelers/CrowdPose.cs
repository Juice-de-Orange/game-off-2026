namespace GameOff.Travelers
{
    /// <summary>
    /// P134: how an evening guest holds themselves at their spot (EveningCrowd). Sit plays a "Sit" state if the figure's controller
    /// has one (A_NPC_Sit is ordered); until then a sitting spot falls back to standing.
    /// </summary>
    public enum CrowdPose
    {
        Stand,
        Sit,
    }
}
