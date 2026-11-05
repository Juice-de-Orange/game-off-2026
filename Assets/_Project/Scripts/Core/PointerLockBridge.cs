namespace GameOff.Core
{
    /// <summary>
    /// Pointer lock in the browser (Plugins/WebGL/PointerLock.jslib). In the editor and desktop builds these do nothing:
    /// Cursor.lockState works there directly.
    /// </summary>
    public static class PointerLockBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int GameOff_IsPointerLocked();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void GameOff_RequestPointerLock();

        public static bool IsLocked => GameOff_IsPointerLocked() == 1;

        /// <summary>Request the lock now (works right after the user's click or key press).</summary>
        public static void Request() => GameOff_RequestPointerLock();
#else
        public static bool IsLocked => UnityEngine.Cursor.lockState == UnityEngine.CursorLockMode.Locked;

        public static void Request() { }
#endif
    }
}
