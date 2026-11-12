using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameOff.Counter;
using GameOff.Documents;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>
    /// P139: the bribe note and its envelope lie loose on the desk: no traveler papers (the return check and an arrest leave them
    /// alone), stacked so the money is never under the note; a stamp set down on the desk stays there.
    /// </summary>
    public class DeskThingsTests
    {
        private const float DeskTop = 0.85f;
        private const float Step = 0.001f;
        private readonly List<Object> _made = new List<Object>();

        [TearDown]
        public void CleanUp()
        {
            for (int i = _made.Count - 1; i >= 0; i--)
                if (_made[i] != null) Object.DestroyImmediate(_made[i]);
            _made.Clear();
        }

        private T Made<T>(T thing) where T : Object
        {
            _made.Add(thing);
            return thing;
        }

        /// <summary>A desk without counter, drawer or spots (desk top = its own height): enough for laying and stacking.</summary>
        private DocumentDesk MakeDesk()
        {
            var go = Made(new GameObject("Desk"));
            go.transform.position = new Vector3(0f, DeskTop, 0f);
            var desk = go.AddComponent<DocumentDesk>();
            var tuning = Made(ScriptableObject.CreateInstance<DocumentTuning>());
            tuning.stackStep = Step;
            var noticeGo = Made(new GameObject("NoticePrefab"));
            noticeGo.AddComponent<BoxCollider>();
            var notice = noticeGo.AddComponent<DocumentView>();
            var view = new SerializedObject(notice);
            view.FindProperty("type").enumValueIndex = (int)DocumentType.Notice;
            view.FindProperty("pageSize").vector2Value = new Vector2(0.30f, 0.42f);
            view.ApplyModifiedPropertiesWithoutUndo();
            var so = new SerializedObject(desk);
            so.FindProperty("tuning").objectReferenceValue = tuning;
            so.FindProperty("noticePrefab").objectReferenceValue = notice;
            so.ApplyModifiedPropertiesWithoutUndo();
            return desk;
        }

        private DocumentView LayNote(DocumentDesk desk, Vector3 at)
        {
            var spot = Made(new GameObject("NoteSpot")).transform;
            spot.SetPositionAndRotation(at, Quaternion.Euler(0f, 84f, 0f));
            var data = Made(DocumentData.CreateRuntime(DocumentType.Notice, new List<DocumentField>
            {
                new DocumentField("title", "A NOTE"),
                new DocumentField("body", "Let QUILL BARRAGH through. 60 CR."),
            }));
            return desk.Lay(data, spot);
        }

        /// <summary>A flat box like the bribe envelope (18 x 11 x 1.5 cm), pivot in its middle.</summary>
        private Transform MakeEnvelope(Vector3 at)
        {
            var envelope = Made(GameObject.CreatePrimitive(PrimitiveType.Cube)).transform;
            envelope.localScale = new Vector3(0.18f, 0.015f, 0.11f);
            envelope.SetPositionAndRotation(at, Quaternion.Euler(0f, 70f, 0f));
            return envelope;
        }

        [Test]
        public void BribeNote_LiesLoose_IsNoTravelerPaper_AndStaysThroughAnArrest()
        {
            var desk = MakeDesk();
            var note = LayNote(desk, new Vector3(0f, 0.9f, 0f));
            Assert.IsNotNull(note);
            Assert.IsTrue(desk.Loose.Contains(note.transform));
            Assert.IsEmpty(desk.OnDesk, "not on the desk list: the return check never waits for it to be handed back");
            Assert.IsEmpty(desk.InDrawer);
            Assert.AreEqual(DeskTop + Step, note.transform.position.y, 1e-5f, "flat on the desk, whatever height the spot had");

            desk.Confiscate();
            Assert.IsTrue(note != null && desk.Loose.Contains(note.transform), "an arrest takes the traveler's papers, not the note");
        }

        [Test]
        public void Envelope_LaidAfterTheNote_LiesOnTopOfIt()
        {
            var desk = MakeDesk();
            var note = LayNote(desk, new Vector3(0f, DeskTop, 0f));
            var envelope = MakeEnvelope(new Vector3(0.05f, DeskTop, 0.05f));
            desk.AddLoose(envelope);
            float bottom = envelope.position.y - 0.015f * 0.5f;
            Assert.AreEqual(note.transform.position.y + Step, bottom, 1e-5f, "the money is never under the note (#43)");
        }

        [Test]
        public void Note_LaidOverTheEnvelope_LiesOnItsTop_AndThingsApartStayOnTheDesk()
        {
            var desk = MakeDesk();
            var envelope = MakeEnvelope(new Vector3(0f, DeskTop, 0f));
            desk.AddLoose(envelope);
            Assert.AreEqual(DeskTop + Step, envelope.position.y - 0.0075f, 1e-5f);
            var over = LayNote(desk, new Vector3(0.05f, DeskTop, 0.02f));
            Assert.AreEqual(DeskTop + Step + 0.015f + Step, over.transform.position.y, 1e-5f, "on the envelope, not inside it");
            var apart = LayNote(desk, new Vector3(1.5f, DeskTop, 0f));
            Assert.AreEqual(DeskTop + Step, apart.transform.position.y, 1e-5f);
        }

        [Test]
        public void LooseThingWithoutCollider_GetsABoxAroundItsMesh()
        {
            var desk = MakeDesk();
            var cube = Made(GameObject.CreatePrimitive(PrimitiveType.Cube));
            var thing = Made(new GameObject("Envelope")).transform;
            thing.gameObject.AddComponent<MeshFilter>().sharedMesh = cube.GetComponent<MeshFilter>().sharedMesh;
            thing.gameObject.AddComponent<MeshRenderer>();
            thing.localScale = new Vector3(0.18f, 0.015f, 0.11f);
            desk.AddLoose(thing);
            var box = thing.GetComponent<BoxCollider>();
            Assert.IsNotNull(box, "the mouse must be able to catch it");
            Assert.AreEqual(Vector3.one.x, box.size.x, 1e-4f);
            Assert.AreEqual(Vector3.one.y, box.size.y, 1e-4f);
            Assert.AreEqual(0f, box.center.magnitude, 1e-4f);
        }

        [Test]
        public void Stamp_SetDownOnTheDesk_StaysThere_EvenAfterEscOrLeaving()
        {
            var controls = Made(new GameObject("Controls")).transform;
            controls.position = new Vector3(-2f, DeskTop, -9f);
            var go = Made(new GameObject("Stamp_Approve"));
            go.transform.SetParent(controls, false);
            go.transform.localPosition = new Vector3(0.9f, 0f, 0.62f);
            var collider = go.AddComponent<BoxCollider>();
            var stamp = go.AddComponent<StampTool>();
            typeof(StampTool).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(stamp, null);

            stamp.PickUp();
            Assert.IsFalse(collider.enabled, "held: the mouse ray sees what is under it");
            var spot = new Vector3(-0.6f, DeskTop, -8.5f);
            stamp.SetDown(spot);
            Assert.IsFalse(stamp.IsHeld);
            Assert.IsTrue(collider.enabled);
            Assert.AreEqual(0f, Vector3.Distance(spot, go.transform.position), 1e-5f);

            stamp.PickUp();
            go.transform.position += Vector3.up * 0.06f; // hovering over the desk
            stamp.PutBack(); // Esc, right click or leaving the counter
            Assert.AreEqual(0f, Vector3.Distance(spot, go.transform.position), 1e-5f, "back where it was set down, not in the air");
        }

        [Test]
        public void Stamp_IsDrawnSharpOnlyWhileSeated()
        {
            int sharp = LayerMask.NameToLayer(StampTool.SharpLayer);
            if (sharp < 0) Assert.Ignore("no Documents layer in this project");
            var go = Made(new GameObject("Stamp"));
            var stamp = go.AddComponent<StampTool>();
            typeof(StampTool).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(stamp, null);
            stamp.ShowSharp(true);
            Assert.AreEqual(sharp, go.layer);
            stamp.ShowSharp(false);
            Assert.AreEqual(0, go.layer, "like the world again when walking around (never through walls)");
        }
    }
}
