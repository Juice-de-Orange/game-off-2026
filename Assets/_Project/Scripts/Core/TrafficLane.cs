using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Core
{
    /// <summary>
    /// One flight lane for ShipTraffic (P47): ships fly in a straight line between two points (either direction),
    /// each with a random sideways/vertical offset, speed and pause before the next one.
    /// </summary>
    [Serializable]
    public class TrafficLane
    {
        public string name = "Lane";
        public Vector3 from = new Vector3(-300f, 30f, 200f);
        public Vector3 to = new Vector3(300f, 30f, 200f);
        [Tooltip("Random offset of each flight around the lane (meters, per axis).")]
        public Vector3 randomOffset = new Vector3(0f, 10f, 20f);
        [Tooltip("Ship models for this lane (FBX from Art/Models/Ships). One is picked at random per flight.")]
        public List<GameObject> ships = new List<GameObject>();
        public float scale = 1f;
        public Vector2 speed = new Vector2(20f, 30f);
        [Tooltip("Seconds between two ships on this lane (random in this range).")]
        public Vector2 interval = new Vector2(20f, 40f);
        [Tooltip("Seconds before the very first ship (so the menu shows something soon).")]
        public Vector2 firstDelay = new Vector2(1f, 6f);
        [Tooltip("Chance that a flight goes to → from instead of from → to.")]
        [Range(0f, 1f)] public float reverseChance = 0.5f;
        [Tooltip("Colour of the lane in the traffic dev tool (P56).")]
        public Color devColor = Color.white;
    }
}
