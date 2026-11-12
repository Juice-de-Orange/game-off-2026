using GameOff.Station;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P136: the morning queue walks in single file, front first, and nobody walks through anybody.</summary>
    public class ArrivalQueueTests
    {
        // Defaults of ArrivingGroup and the four hangar pads of EconomyBuilder.BuildHangarShips.
        private static readonly Vector3 LaneEntry = new Vector3(2f, 0f, -22.5f);
        private static readonly Vector3 LaneEnd = new Vector3(2f, 0f, -13.6f);
        private static readonly float[] QueueZ = { -16f, -18f, -20f };
        private const float Speed = 1.5f, Gap = 1.8f, StartSpacing = 1.5f, StepOut = 3f;
        private static readonly Vector3[] Pads =
        {
            new Vector3(-10f, 0f, -23f), new Vector3(10f, 0f, -23f), new Vector3(-8f, 0f, -33f), new Vector3(10f, 0f, -35f),
        };

        [Test]
        public void Places_FrontOfTheQueueIsNearestTheLaneEnd()
        {
            CollectionAssert.AreEqual(new[] { -16f, -18f, -20f }, ArrivalQueue.PlacesFrontFirst(new[] { -20f, -16f, -18f }, LaneEnd.z));
            CollectionAssert.AreEqual(new[] { -16f, -18f, -20f }, ArrivalQueue.PlacesFrontFirst(QueueZ, LaneEnd.z), "already in order: unchanged");
            Assert.AreEqual(0, ArrivalQueue.PlacesFrontFirst(null, LaneEnd.z).Length);
        }

        [Test]
        public void Delays_FrontFirst_ThenOneGapBehindEachOther()
        {
            Assert.AreEqual(0f, ArrivalQueue.StepOutDelay(0, Gap, Speed));
            Assert.AreEqual(1.2f, ArrivalQueue.StepOutDelay(1, Gap, Speed), 1e-5f);
            Assert.AreEqual(2.4f, ArrivalQueue.StepOutDelay(2, Gap, Speed), 1e-5f);
            Assert.AreEqual(0f, ArrivalQueue.StepOutDelay(2, Gap, 0f), "no speed: no waiting either");
            Assert.AreEqual(0f, ArrivalQueue.LeaveDelay(0, StartSpacing));
            Assert.AreEqual(3f, ArrivalQueue.LeaveDelay(2, StartSpacing), 1e-5f);
        }

        [Test]
        public void ExitPoint_IsBesideThePad_TowardTheLane_OnTheFloor()
        {
            var exit = ArrivalQueue.ExitPoint(new Vector3(-10f, 0.4f, -22.5f), LaneEntry, StepOut);
            Assert.AreEqual(new Vector3(-7f, 0f, -22.5f), exit);
            Assert.AreEqual(LaneEntry, ArrivalQueue.ExitPoint(LaneEntry + Vector3.left, LaneEntry, StepOut), "never beyond the lane entry");
        }

        [Test]
        public void Along_WalksThePolyline_AndStopsAtItsEnds()
        {
            var path = new[] { Vector3.zero, new Vector3(3f, 0f, 0f), new Vector3(3f, 0f, 4f) };
            Assert.AreEqual(7f, ArrivalQueue.Length(path), 1e-5f);
            Assert.AreEqual(Vector3.zero, ArrivalQueue.Along(path, -1f, out _));
            Assert.AreEqual(new Vector3(2f, 0f, 0f), ArrivalQueue.Along(path, 2f, out var heading));
            Assert.AreEqual(Vector3.right, heading);
            Assert.AreEqual(new Vector3(3f, 0f, 1f), ArrivalQueue.Along(path, 4f, out heading));
            Assert.AreEqual(Vector3.forward, heading);
            Assert.AreEqual(new Vector3(3f, 0f, 4f), ArrivalQueue.Along(path, 99f, out _));
        }

        [Test]
        public void Queue_FromEveryPad_WhateverTheShiftStart_StaysInSingleFile()
        {
            foreach (var pad in Pads)
            foreach (float shiftStart in new[] { 0.5f, 4f, 9f, 13f, 60f })
            {
                var places = ArrivalQueue.PlacesFrontFirst(QueueZ, LaneEnd.z);
                var exit = ArrivalQueue.ExitPoint(pad, LaneEntry, StepOut);
                var figures = new Figure[places.Length];
                for (int i = 0; i < figures.Length; i++) figures[i] = new Figure(i, ArrivalQueue.WayIn(exit, LaneEntry, places[i]), shiftStart);

                for (float t = 0f; t < 90f; t += 0.02f)
                    for (int i = 1; i < figures.Length; i++)
                    {
                        var front = figures[i - 1];
                        var back = figures[i];
                        if (!back.Visible(t) || !front.Visible(t)) continue;
                        float ahead = front.Progress(t) - back.Progress(t);
                        var a = front.Position(t);
                        var b = back.Position(t);
                        float apart = Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
                        if (ahead < 1f || apart < 1f)
                            Assert.Fail($"pad {pad}, shift start {shiftStart} s, t {t:0.00} s: figure {i - 1} should stay ahead of figure {i} " +
                                        $"(ahead {ahead:0.00} m on the way, {apart:0.00} m apart)");
                    }

                for (int i = 0; i < figures.Length; i++)
                    Assert.AreEqual(places[i], figures[i].Position(figures[i].Arrive).z, 1e-3f, $"figure {i} stops at its own place");
                for (int i = 1; i < figures.Length; i++)
                    Assert.Less(figures[i - 1].Gone, figures[i].Gone, "the front of the queue walks on first");
            }
        }

        /// <summary>The timeline ArrivingGroup.Route follows for one figure, measured from the landing.</summary>
        private sealed class Figure
        {
            private readonly Vector3[] _wayIn, _wayOn;
            private readonly float _spawn, _leave;
            public readonly float Arrive, Gone;

            public Figure(int index, Vector3[] wayIn, float shiftStart)
            {
                _wayIn = wayIn;
                _wayOn = new[] { wayIn[wayIn.Length - 1], LaneEnd };
                _spawn = ArrivalQueue.StepOutDelay(index, Gap, Speed);
                Arrive = _spawn + ArrivalQueue.Length(_wayIn) / Speed;
                _leave = Mathf.Max(Arrive, shiftStart) + ArrivalQueue.LeaveDelay(index, StartSpacing);
                Gone = _leave + ArrivalQueue.Length(_wayOn) / Speed;
            }

            public bool Visible(float t) => t >= _spawn && t < Gone;

            /// <summary>Metres along the shared way: out of the ship, to the lane entry, up the lane.</summary>
            public float Progress(float t)
            {
                if (t < Arrive) return (t - _spawn) * Speed;
                if (t < _leave) return ArrivalQueue.Length(_wayIn);
                return ArrivalQueue.Length(_wayIn) + (t - _leave) * Speed;
            }

            public Vector3 Position(float t)
            {
                if (t < Arrive) return ArrivalQueue.Along(_wayIn, (t - _spawn) * Speed, out _);
                if (t < _leave) return _wayIn[_wayIn.Length - 1];
                return ArrivalQueue.Along(_wayOn, (t - _leave) * Speed, out _);
            }
        }
    }
}
