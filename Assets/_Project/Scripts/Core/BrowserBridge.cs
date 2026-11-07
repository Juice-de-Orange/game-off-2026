namespace GameOff.Core
{
    /// <summary>The browser's user agent for the bug report (P84): in WebGL via Plugins/WebGL/Browser.jslib, elsewhere the OS.</summary>
    public static class BrowserBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string GameOff_UserAgent();

        public static string UserAgent => GameOff_UserAgent();
#else
        public static string UserAgent => "no browser (" + UnityEngine.SystemInfo.operatingSystem + ")";
#endif
    }
}
