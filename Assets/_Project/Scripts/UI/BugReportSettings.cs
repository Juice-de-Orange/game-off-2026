using UnityEngine;

namespace GameOff.UI
{
    /// <summary>P84: where the in-game bug report goes (deploy/reports on Geoffrey; CORS allows the itch.io build too).</summary>
    [CreateAssetMenu(menuName = "GameOff/Bug Report Settings", fileName = "BugReportSettings")]
    public class BugReportSettings : ScriptableObject
    {
        public string endpoint = "https://kestrel9.maxlworks.com/api/report";
        [Tooltip("Seconds before a report counts as failed.")]
        public int timeoutSeconds = 15;
    }
}
