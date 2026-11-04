using UnityEngine;

namespace GameOff.Documents
{
    /// <summary>Tuning values for handling documents on the counter desk (P06).</summary>
    [CreateAssetMenu(menuName = "GameOff/Document Tuning", fileName = "DocumentTuning")]
    public class DocumentTuning : ScriptableObject
    {
        [Tooltip("How high a dragged document floats above the desk (m).")]
        public float dragLift = 0.03f;
        [Tooltip("Mouse movement (screen pixels) after which a press becomes a drag instead of a click.")]
        public float dragThresholdPixels = 6f;
        [Tooltip("Part of the screen height a document fills when held up for reading.")]
        public float inspectScreenFill = 0.8f;
        [Tooltip("Duration of picking up / laying down a document for reading (s).")]
        public float inspectDuration = 0.2f;
        [Tooltip("How far the drawer slides toward the traveler (m).")]
        public float drawerTravel = 0.4f;
        [Tooltip("Duration of one drawer move (s).")]
        public float drawerDuration = 0.45f;
        [Tooltip("Duration of a document gliding from the drawer onto the desk (s).")]
        public float slideDuration = 0.35f;
        [Tooltip("Delay between documents gliding out of the drawer (s).")]
        public float slideStagger = 0.15f;
        [Tooltip("Height step between stacked documents, against z-fighting (m).")]
        public float stackStep = 0.001f;
    }
}
