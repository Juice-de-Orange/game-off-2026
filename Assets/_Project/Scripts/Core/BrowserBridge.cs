namespace GameOff.Core
{
    /// <summary>
    /// New tab and user agent (P78). In the browser via Plugins/WebGL/Browser.jslib: the tab opens on the browser's next
    /// pointerup, so call it from a pointer-down handler. Elsewhere Application.OpenURL right away and the operating system.
    /// </summary>
    public static class BrowserBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void GameOff_OpenUrlOnRelease(string url);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string GameOff_UserAgent();

        public static void OpenUrlOnRelease(string url) => GameOff_OpenUrlOnRelease(url);
        public static string UserAgent => GameOff_UserAgent();
#else
        public static void OpenUrlOnRelease(string url) => UnityEngine.Application.OpenURL(url);
        public static string UserAgent => "no browser (" + UnityEngine.SystemInfo.operatingSystem + ")";
#endif
    }
}
