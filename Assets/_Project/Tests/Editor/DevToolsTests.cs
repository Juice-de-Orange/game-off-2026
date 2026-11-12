using GameOff.Core;
using GameOff.Day;
using GameOff.EditorTools;
using GameOff.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameOff.Tests
{
    /// <summary>
    /// The dev tools switch in the options (P49): saved setting, inspector master switch, live change event.
    /// P146: only the jam build (WebBuild.BuildRelease, K9_RELEASE) forbids them and hides the DEV tab.
    /// </summary>
    public class DevToolsTests
    {
        private const string Key = "settings.devTools";
        private bool _hadKey;
        private int _saved;

        [SetUp]
        public void RememberSetting()
        {
            _hadKey = PlayerPrefs.HasKey(Key);
            _saved = PlayerPrefs.GetInt(Key, 0);
            PlayerPrefs.DeleteKey(Key);
        }

        [TearDown]
        public void RestoreSetting()
        {
            if (_hadKey) PlayerPrefs.SetInt(Key, _saved);
            else PlayerPrefs.DeleteKey(Key);
        }

        [Test]
        public void Default_IsOnInTheEditor() => Assert.IsTrue(GameSettings.DevTools);

        [Test]
        public void Setting_IsSavedAndRaisesChanged()
        {
            int changed = 0;
            void OnChanged() => changed++;
            GameSettings.Changed += OnChanged;
            try
            {
                GameSettings.DevTools = false;
                Assert.IsFalse(GameSettings.DevTools);
                Assert.AreEqual(1, changed);
            }
            finally
            {
                GameSettings.Changed -= OnChanged;
            }
        }

        [Test]
        public void ResetControls_LeavesDevToolsAlone()
        {
            float sensitivity = GameSettings.MouseSensitivity;
            bool invertY = GameSettings.InvertY;
            try
            {
                GameSettings.DevTools = false;
                GameSettings.ResetControls();
                Assert.IsFalse(GameSettings.DevTools);
            }
            finally
            {
                GameSettings.MouseSensitivity = sensitivity;
                GameSettings.InvertY = invertY;
            }
        }

        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(false, false, false)]
        public void DayManager_NeedsMasterSwitchAndSetting(bool master, bool setting, bool expected)
        {
            var go = new GameObject("DayManagerTest");
            try
            {
                var manager = go.AddComponent<DayManager>();
                var so = new SerializedObject(manager);
                so.FindProperty("showDevTools").boolValue = master;
                so.ApplyModifiedPropertiesWithoutUndo();
                GameSettings.DevTools = setting;
                Assert.AreEqual(expected, manager.DevTools);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // K9_RELEASE comes only from WebBuild.BuildRelease: in Player Settings it would take the dev tools from the live version too
        [Test]
        public void DevTools_AllowedOutsideTheJamBuild() => Assert.IsTrue(GameSettings.DevToolsAllowed);

        [Test]
        public void ReleaseBuild_AloneGetsTheReleaseDefine()
        {
            CollectionAssert.AreEqual(new[] { "K9_RELEASE" }, WebBuild.ExtraDefines(release: true));
            CollectionAssert.IsEmpty(WebBuild.ExtraDefines(release: false));
        }

        [Test]
        public void DevToolsOn_WithoutDayManager_OnUnlessTheBuildForbidsThem() =>
            Assert.AreEqual(GameSettings.DevToolsAllowed, DayManager.DevToolsOn(null));

        [Test]
        public void HideDevTab_TakesOutTheDevPageAndCentresTheOtherTabs()
        {
            var root = new GameObject("OptionsTest", typeof(RectTransform));
            try
            {
                // Like MenuSceneBuilder: AUDIO, CONTROLS, DEV in a centred row 175 apart, the dev switch on the third page
                var tabs = new Button[3];
                var pages = new GameObject[3];
                for (int i = 0; i < 3; i++)
                {
                    var tab = new GameObject("Tab" + i, typeof(RectTransform), typeof(Image), typeof(Button));
                    tab.transform.SetParent(root.transform, false);
                    ((RectTransform)tab.transform).anchoredPosition = new Vector2((i - 1) * 175f, 145f);
                    tabs[i] = tab.GetComponent<Button>();
                    pages[i] = new GameObject("Page" + i, typeof(RectTransform));
                    pages[i].transform.SetParent(root.transform, false);
                }
                var devSwitch = new GameObject("DevTools", typeof(RectTransform), typeof(Image), typeof(Button));
                devSwitch.transform.SetParent(pages[2].transform, false);
                var options = root.AddComponent<OptionsPanel>();
                var so = new SerializedObject(options);
                var tabList = so.FindProperty("tabButtons");
                var pageList = so.FindProperty("pages");
                tabList.arraySize = 3;
                pageList.arraySize = 3;
                for (int i = 0; i < 3; i++)
                {
                    tabList.GetArrayElementAtIndex(i).objectReferenceValue = tabs[i];
                    pageList.GetArrayElementAtIndex(i).objectReferenceValue = pages[i];
                }
                so.FindProperty("devTools").objectReferenceValue = devSwitch.GetComponent<Button>();
                so.ApplyModifiedPropertiesWithoutUndo();

                options.HideDevTab();

                Assert.IsFalse(tabs[2].gameObject.activeSelf);
                Assert.IsFalse(pages[2].activeSelf);
                Assert.AreEqual(-87.5f, ((RectTransform)tabs[0].transform).anchoredPosition.x, 0.01f);
                Assert.AreEqual(87.5f, ((RectTransform)tabs[1].transform).anchoredPosition.x, 0.01f);
                Assert.AreEqual(145f, ((RectTransform)tabs[1].transform).anchoredPosition.y, 0.01f);
                options.SelectTab(2); // the old DEV index: now the last tab left
                Assert.IsTrue(pages[1].activeSelf);
                Assert.IsFalse(pages[0].activeSelf);
                Assert.IsFalse(pages[2].activeSelf);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
