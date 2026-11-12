using GameOff.Documents;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P139: how things lie on the desk: overlap of turned footprints, the pile heights, the desk area and the bare desk top.</summary>
    public class DeskLayoutTests
    {
        private const float Desk = 0.85f;
        private const float Step = 0.001f;

        private static DeskLayout.Footprint Box(float x, float z, float width, float depth, float yaw = 0f)
        {
            var turn = Quaternion.Euler(0f, yaw, 0f);
            return DeskLayout.Footprint.Of(new Vector3(x, 1f, z), turn * Vector3.right, turn * Vector3.forward, new Vector2(width * 0.5f, depth * 0.5f));
        }

        [Test]
        public void Overlap_OnlyWhenTheRectanglesReallyCover()
        {
            var a = Box(0f, 0f, 0.2f, 0.2f);
            Assert.IsTrue(DeskLayout.Overlap(a, Box(0.1f, 0.05f, 0.2f, 0.2f)));
            Assert.IsFalse(DeskLayout.Overlap(a, Box(0.2f, 0f, 0.2f, 0.2f)), "edges that only touch");
            Assert.IsFalse(DeskLayout.Overlap(a, Box(0.5f, 0f, 0.2f, 0.2f)));
            Assert.IsTrue(DeskLayout.Overlap(a, Box(0f, 0f, 0.05f, 0.05f, 30f)), "a small thing on a big one");
        }

        [Test]
        public void Overlap_TurnedThingsSideBySide_DontCount()
        {
            // Two long thin things turned 45°, 5 cm apart across: their axis-aligned boxes overlap, they don't.
            var across = Quaternion.Euler(0f, 45f, 0f) * Vector3.forward * 0.05f;
            var a = Box(0f, 0f, 0.3f, 0.02f, 45f);
            var b = Box(across.x, across.z, 0.3f, 0.02f, 45f);
            Assert.IsFalse(DeskLayout.Overlap(a, b));
            Assert.IsTrue(DeskLayout.Overlap(a, Box(across.x * 0.1f, across.z * 0.1f, 0.3f, 0.02f, 45f)));
        }

        [Test]
        public void Bottoms_PapersStackOnlyWhereTheyOverlap()
        {
            var footprints = new[] { Box(0f, 0f, 0.15f, 0.21f), Box(0.05f, 0f, 0.15f, 0.21f), Box(1f, 0f, 0.15f, 0.21f) };
            var bottoms = new float[3];
            DeskLayout.Bottoms(footprints, new[] { 0f, 0f, 0f }, Desk, Step, bottoms);
            Assert.AreEqual(Desk + Step, bottoms[0], 1e-6f);
            Assert.AreEqual(Desk + 2f * Step, bottoms[1], 1e-6f, "on the first paper");
            Assert.AreEqual(Desk + Step, bottoms[2], 1e-6f, "alone: flat on the desk");
        }

        [Test]
        public void Bottoms_WhatComesLaterLiesOnTheEnvelope_AndTheEnvelopeOnEarlierPapers()
        {
            const float envelope = 0.015f;
            var bottoms = new float[2];
            // envelope first, then a paper over it: the paper lies on the envelope, not inside it
            DeskLayout.Bottoms(new[] { Box(0f, 0f, 0.18f, 0.11f), Box(0.05f, 0.02f, 0.3f, 0.42f) }, new[] { envelope, 0f }, Desk, Step, bottoms);
            Assert.AreEqual(Desk + Step, bottoms[0], 1e-6f);
            Assert.AreEqual(Desk + Step + envelope + Step, bottoms[1], 1e-6f);
            // the note first, then the envelope (BribeNote lays it last): the money lies on top, visible (#43)
            DeskLayout.Bottoms(new[] { Box(0.05f, 0.02f, 0.3f, 0.42f), Box(0f, 0f, 0.18f, 0.11f) }, new[] { 0f, envelope }, Desk, Step, bottoms);
            Assert.Greater(bottoms[1], bottoms[0]);
        }

        [Test]
        public void ClampToArea_KeepsInsideAndHeight()
        {
            var size = new Vector2(1.1f, 3.9f);
            var inside = new Vector3(0.2f, 0.3f, -1f);
            Assert.AreEqual(inside, DeskLayout.ClampToArea(inside, size));
            var outside = DeskLayout.ClampToArea(new Vector3(5f, 0.3f, -7f), size);
            Assert.AreEqual(0.55f, outside.x, 1e-6f);
            Assert.AreEqual(0.3f, outside.y, 1e-6f, "the height stays");
            Assert.AreEqual(-1.95f, outside.z, 1e-6f);
        }

        [Test]
        public void OnDeskTop_OnlyTheBareDeskFacingUp()
        {
            Assert.IsTrue(DeskLayout.OnDeskTop(0.85f, 1f, 0.851f, 0.02f));
            Assert.IsFalse(DeskLayout.OnDeskTop(0.93f, 1f, 0.851f, 0.02f), "on top of the printer or a button");
            Assert.IsFalse(DeskLayout.OnDeskTop(0.85f, 0.3f, 0.851f, 0.02f), "the desk's front edge");
        }
    }
}
