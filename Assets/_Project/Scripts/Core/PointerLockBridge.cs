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

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void GameOff_SetPointerLockWanted(int wanted);

        public static bool IsLocked => GameOff_IsPointerLocked() == 1;

        /// <summary>Request the lock now (works right after the user's click or key press).</summary>
        public static void Request() => GameOff_RequestPointerLock();

        /// <summary>
        /// P138: while wanted, every mouse press on the canvas asks the browser for the lock inside the press itself, so the
        /// next click always catches the mouse again (after Esc, a tab switch or a refused request). GameCursor keeps it in step
        /// with Cursor.lockState.
        /// </summary>
        public static void SetWanted(bool wanted) => GameOff_SetPointerLockWanted(wanted ? 1 : 0);
#else
        public static bool IsLocked => UnityEngine.Cursor.lockState == UnityEngine.CursorLockMode.Locked;

        public static void Request() { }

        public static void SetWanted(bool wanted) { }
#endif

        private static readonly LookSettle Settle = new LookSettle();

        /// <summary>
        /// P138: mouse movement for looking around with the captured pointer. The first frames after the lock changed (or after
        /// a pause in looking) are dropped, so catching the mouse never jerks the camera. Call it every frame you look.
        /// </summary>
        public static UnityEngine.Vector2 LookDelta(UnityEngine.Vector2 delta) => Settle.Filter(UnityEngine.Time.frameCount, IsLocked, delta);
    }
}
