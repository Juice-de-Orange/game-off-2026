namespace GameOff.Core
{
    /// <summary>
    /// Copy text to the clipboard: in the browser via Plugins/WebGL/Clipboard.jslib (Unity's copy buffer stays inside the
    /// page there), elsewhere via GUIUtility.systemCopyBuffer.
    /// </summary>
    public static class ClipboardBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void GameOff_CopyText(string text);

        public static void Copy(string text) => GameOff_CopyText(text);
#else
        public static void Copy(string text) => UnityEngine.GUIUtility.systemCopyBuffer = text;
#endif
    }
}
