namespace GameOff.Player
{
    /// <summary>When the next step sounds and which of the clips it uses: every stepLength meters, never the same clip twice in a row.</summary>
    public sealed class Footsteps
    {
        private float _walked;
        private int _last = -1;

        /// <summary>Adds walked meters. True when a step is due; at most one per call, the rest counts toward the next.</summary>
        public bool Advance(float meters, float stepLength)
        {
            if (meters <= 0f || stepLength <= 0f) return false;
            _walked += meters;
            if (_walked < stepLength) return false;
            _walked %= stepLength;
            return true;
        }

        /// <param name="random01">Random value from 0 up to (not including) 1.</param>
        public int NextIndex(int count, float random01)
        {
            if (count <= 1) return 0;
            int candidates = _last < 0 ? count : count - 1;
            int index = (int)(random01 * candidates);
            if (index >= candidates) index = candidates - 1;
            if (_last >= 0 && index >= _last) index++; // skips the last clip
            _last = index;
            return index;
        }
    }
}
