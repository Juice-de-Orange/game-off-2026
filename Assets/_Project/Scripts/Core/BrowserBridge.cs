namespace GameOff.Core
{
    /// <summary>The browser's user agent for the bug report (P84) and the page's metrics session (P129): in WebGL via Plugins/WebGL/Browser.jslib, elsewhere the OS.</summary>
    public static class BrowserBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string GameOff_UserAgent();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string GameOff_PageSession();

        public static string UserAgent => GameOff_UserAgent();
        /// <summary>P129: the session id of the page's "visit" metric (index.html), empty when the page has none.</summary>
        public static string PageSession => GameOff_PageSession();
#else
        public static string UserAgent => "no browser (" + UnityEngine.SystemInfo.operatingSystem + ")";
        public static string PageSession => "";
#endif
    }
}
