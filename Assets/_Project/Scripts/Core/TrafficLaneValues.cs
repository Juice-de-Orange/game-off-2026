using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Core
{
    /// <summary>
    /// The editable numbers of traffic lanes as JSON (P56): the dev tool's SAVE LANE / COPY VALUES put this on the clipboard
    /// in a build, and GameOff → Paste Traffic Lanes applies it to the tuning asset in Unity (matched by lane name).
    /// </summary>
    [Serializable]
    public class TrafficLaneValues
    {
        [Serializable]
        public class Entry
        {
            public string name;
            public Vector3 from, to, randomOffset;
            public float scale;
            public Vector2 speed, interval;
        }

        public List<Entry> lanes = new List<Entry>();

        public static string ToJson(IEnumerable<TrafficLane> lanes)
        {
            var values = new TrafficLaneValues();
            foreach (var l in lanes)
                values.lanes.Add(new Entry
                {
                    name = l.name, from = l.from, to = l.to, randomOffset = l.randomOffset,
                    scale = l.scale, speed = l.speed, interval = l.interval,
                });
            return JsonUtility.ToJson(values, true);
        }

        /// <summary>Copies the values onto lanes with the same name; returns how many lanes changed.</summary>
        public static int Apply(string json, IList<TrafficLane> lanes)
        {
            TrafficLaneValues values;
            try { values = JsonUtility.FromJson<TrafficLaneValues>(json); }
            catch (ArgumentException) { return 0; }
            if (values == null) return 0;
            int changed = 0;
            foreach (var e in values.lanes)
                foreach (var l in lanes)
                {
                    if (l.name != e.name) continue;
                    l.from = e.from; l.to = e.to; l.randomOffset = e.randomOffset;
                    l.scale = e.scale; l.speed = e.speed; l.interval = e.interval;
                    changed++;
                }
            return changed;
        }
    }
}
