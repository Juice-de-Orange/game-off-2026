using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Station
{
    /// <summary>
    /// P136: order and timing of the morning queue (ArrivingGroup), kept apart from the component so it can be tested.
    /// Everyone walks the same way: out of the ship to the south end of the barrier lane, then up the lane. The front of the queue
    /// steps out first and the others follow one gap behind at the same speed, so they walk in single file and stop behind each other
    /// (front = the place nearest the inspection door). When the shift starts the front walks on first. Nobody passes anybody.
    /// </summary>
    public static class ArrivalQueue
    {
        /// <summary>Where they step out of the ship: <paramref name="stepOut"/> metres from the pad toward the lane, at floor height.</summary>
        public static Vector3 ExitPoint(Vector3 pad, Vector3 laneEntry, float stepOut)
        {
            var toLane = laneEntry - pad;
            toLane.y = 0f;
            var exit = toLane.sqrMagnitude > 0.0001f ? pad + toLane.normalized * Mathf.Min(stepOut, toLane.magnitude) : pad;
            exit.y = laneEntry.y;
            return exit;
        }

        /// <summary>The places in the lane (world Z), front of the queue first: the one nearest the end of the lane.</summary>
        public static float[] PlacesFrontFirst(float[] queueZ, float laneEndZ)
        {
            var places = queueZ != null ? (float[])queueZ.Clone() : new float[0];
            System.Array.Sort(places, (a, b) => Mathf.Abs(a - laneEndZ).CompareTo(Mathf.Abs(b - laneEndZ)));
            return places;
        }

        /// <summary>The way in for one figure: from the ship to the lane entry, then up the lane to its place.</summary>
        public static Vector3[] WayIn(Vector3 exit, Vector3 laneEntry, float placeZ) =>
            new[] { exit, laneEntry, new Vector3(laneEntry.x, laneEntry.y, placeZ) };

        /// <summary>Seconds after the landing at which figure <paramref name="index"/> (0 = front) steps out: one gap behind the one before.</summary>
        public static float StepOutDelay(int index, float gapMetres, float walkSpeed) =>
            walkSpeed > 0f ? Mathf.Max(0, index) * Mathf.Max(0f, gapMetres) / walkSpeed : 0f;

        /// <summary>Seconds after the shift start (or after reaching its place, if that is later) at which figure index walks on.</summary>
        public static float LeaveDelay(int index, float spacingSeconds) => Mathf.Max(0, index) * Mathf.Max(0f, spacingSeconds);

        /// <summary>Length of the path in metres.</summary>
        public static float Length(IReadOnlyList<Vector3> path)
        {
            float length = 0f;
            for (int i = 1; i < path.Count; i++) length += Vector3.Distance(path[i - 1], path[i]);
            return length;
        }

        /// <summary>The point <paramref name="distance"/> metres along the path (clamped to its ends) and the direction walked there.</summary>
        public static Vector3 Along(IReadOnlyList<Vector3> path, float distance, out Vector3 heading)
        {
            heading = Vector3.zero;
            if (path == null || path.Count == 0) return Vector3.zero;
            float left = Mathf.Max(0f, distance);
            for (int i = 1; i < path.Count; i++)
            {
                var segment = path[i] - path[i - 1];
                float length = segment.magnitude;
                if (length < 0.0001f) continue;
                heading = segment / length;
                if (left <= length) return path[i - 1] + heading * left;
                left -= length;
            }
            return path[path.Count - 1];
        }
    }
}
