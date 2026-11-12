using UnityEngine;

namespace GameOff.Core
{
    /// <summary>
    /// P138: drops the mouse movement of the first frames after the pointer lock changed, and after nobody looked around for a
    /// while (a menu, the counter, a window): browsers report the jump of the freshly captured pointer as one big movement, which
    /// would jerk the camera on the first click. Plain logic so it is tested; PointerLockBridge.LookDelta uses one instance.
    /// </summary>
    public class LookSettle
    {
        /// <summary>Frames whose movement is dropped after a change.</summary>
        public const int SettleFrames = 2;

        private bool _started;
        private int _lastFrame;
        private bool _wasLocked;
        private int _quietUntil;

        /// <summary>The movement to use in this frame: zero while settling. Several readers in one frame get the same answer.</summary>
        public Vector2 Filter(int frame, bool locked, Vector2 delta)
        {
            if (!_started || frame != _lastFrame)
            {
                bool fresh = !_started || frame - _lastFrame > 1; // nobody read the mouse last frame: looking starts anew
                if (fresh || locked != _wasLocked) _quietUntil = frame + SettleFrames;
                _started = true;
                _lastFrame = frame;
                _wasLocked = locked;
            }
            return frame < _quietUntil ? Vector2.zero : delta;
        }
    }
}
