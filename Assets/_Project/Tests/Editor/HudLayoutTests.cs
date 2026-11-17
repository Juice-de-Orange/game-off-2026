using System.Linq;
using GameOff.Rest;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameOff.Tests
{
    /// <summary>
    /// P151: the seat HUD's line (a thought at the porthole, an announcement on the bench) stands above the day UI's hint bar,
    /// so a thought and a hint or announcement shown at the same time can both be read.
    /// </summary>
    public class HudLayoutTests
    {
        private const string DayFlowPrefab = "Assets/_Project/Prefabs/Day/PF_DayFlow.prefab";
        private const string RestSpotsPrefab = "Assets/_Project/Prefabs/Rest/PF_RestSpots.prefab";

        [Test]
        public void SeatLine_StandsAboveTheHintBar()
        {
            var day = AssetDatabase.LoadAssetAtPath<GameObject>(DayFlowPrefab);
            var rest = AssetDatabase.LoadAssetAtPath<GameObject>(RestSpotsPrefab);
            if (day == null || rest == null) Assert.Ignore("PF_DayFlow or PF_RestSpots is not built yet");

            var hud = rest.GetComponentInChildren<RestHud>(true);
            Assert.That(hud, Is.Not.Null, "PF_RestSpots has a RestHud");
            var line = hud.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(r => r.name == "Line");
            var hint = day.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(r => r.name == "Hint");
            Assert.That(line, Is.Not.Null, "the seat HUD has its line");
            Assert.That(hint, Is.Not.Null, "the day UI has its hint bar");

            // Both bottom-anchored on canvases with the same reference resolution, so their anchored positions compare directly.
            Assert.That(ReferenceOf(line), Is.EqualTo(ReferenceOf(hint)), "both canvases scale alike");
            Assert.That(line.anchorMin.y, Is.EqualTo(0f));
            Assert.That(hint.anchorMin.y, Is.EqualTo(0f));
            float hintTop = hint.anchoredPosition.y + (1f - hint.pivot.y) * hint.sizeDelta.y;
            float lineBottom = line.anchoredPosition.y - line.pivot.y * line.sizeDelta.y;
            Assert.That(lineBottom, Is.GreaterThanOrEqualTo(hintTop), "the thought line starts above the hint bar");
        }

        private static Vector2 ReferenceOf(Transform t)
        {
            var scaler = t.GetComponentInParent<CanvasScaler>(true);
            return scaler != null ? scaler.referenceResolution : Vector2.zero;
        }
    }
}
