using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Core
{
    /// <summary>Lanes and limits for ShipTraffic (P47 menu; P25 can add its own asset for Main with longer intervals).</summary>
    [CreateAssetMenu(menuName = "GameOff/Ship Traffic Tuning", fileName = "ShipTrafficTuning")]
    public class ShipTrafficTuning : ScriptableObject
    {
        public List<TrafficLane> lanes = new List<TrafficLane>();
        [Tooltip("Never more ships than this in the air at once (all lanes together).")]
        public int maxActive = 4;
        [Tooltip("Rotation that turns the models' nose to +Z (the FBX export forward).")]
        public float modelYaw = 0f;
    }
}
