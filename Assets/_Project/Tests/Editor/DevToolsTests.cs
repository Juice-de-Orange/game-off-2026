using GameOff.Core;
using GameOff.Day;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>The dev tools switch in the options (P49): saved setting, inspector master switch, live change event.</summary>
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
    }
}
