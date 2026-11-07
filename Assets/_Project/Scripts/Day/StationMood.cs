using GameOff.Core;
using GameOff.Economy;
using UnityEngine;

namespace GameOff.Day
{
    /// <summary>
    /// The station's situation (P36): every morning it is today's crisis events (MarketEvent.crisis) plus what is left of yesterday,
    /// capped at 1. The worse it is, the more often the flicker lamps flicker, the neon signs drop out (from neonFrom) and the power
    /// fails for a few seconds (from blackoutFrom). Values in StationMoodTuning.
    /// </summary>
    public class StationMood : MonoBehaviour
    {
        [SerializeField] private DayManager day;
        [SerializeField] private StationMoodTuning tuning;
        [SerializeField] private LightFlicker[] lamps = new LightFlicker[0];
        [Tooltip("Glowing neon signs (emission is switched off when they drop out).")]
        [SerializeField] private Renderer[] neons = new Renderer[0];

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private MaterialPropertyBlock _dark;
        private float[] _neonNext = new float[0];
        private float[] _neonEnd = new float[0];
        private bool[] _neonOff = new bool[0];
        private float _nextBlackout;
        private float _blackoutEnd;
        private bool _blackout;

        /// <summary>0 = calm, 1 = the station is falling apart.</summary>
        public float Situation { get; private set; }

        /// <summary>Tomorrow's situation from today's and the crises of tomorrow.</summary>
        public static float Next(float previous, float crisis, float carryOver) => Mathf.Clamp01(crisis + previous * carryOver);

        private void Awake()
        {
            _dark = new MaterialPropertyBlock();
            _dark.SetColor(EmissionColor, Color.black);
            _neonNext = new float[neons.Length];
            _neonEnd = new float[neons.Length];
            _neonOff = new bool[neons.Length];
        }

        private void OnEnable()
        {
            if (day != null) day.DayStarted += OnDayStarted;
        }

        private void OnDisable()
        {
            if (day != null) day.DayStarted -= OnDayStarted;
            SetBlackout(false);
            for (int i = 0; i < neons.Length; i++) SetNeon(i, false);
        }

        private void OnDayStarted(int n)
        {
            float crisis = 0f;
            foreach (var e in Market.TodayEvents)
                if (e != null) crisis += e.crisis;
            Situation = Next(Situation, crisis, tuning != null ? tuning.carryOver : 0.5f);
            Apply();
        }

        /// <summary>Sets the situation directly (dev / tests) and applies it.</summary>
        public void SetSituation(float value)
        {
            Situation = Mathf.Clamp01(value);
            Apply();
        }

        private void Apply()
        {
            if (tuning == null) return;
            var pause = Vector2.Lerp(tuning.calmPause, tuning.worstPause, Situation);
            foreach (var lamp in lamps)
                if (lamp != null) lamp.SetPause(pause);
            float now = Time.time;
            for (int i = 0; i < neons.Length; i++) _neonNext[i] = now + Random.Range(NeonPause.x, NeonPause.y);
            _nextBlackout = now + Random.Range(tuning.blackoutPause.x, tuning.blackoutPause.y);
            if (Situation < tuning.blackoutFrom) SetBlackout(false);
        }

        private Vector2 NeonPause => Vector2.Lerp(tuning.neonCalmPause, tuning.neonWorstPause,
            Mathf.InverseLerp(tuning.neonFrom, 1f, Situation));

        private void Update()
        {
            if (tuning == null) return;
            float now = Time.time;

            if (Situation >= tuning.blackoutFrom && !_blackout && now >= _nextBlackout)
            {
                _blackoutEnd = now + Random.Range(tuning.blackoutSeconds.x, tuning.blackoutSeconds.y);
                _nextBlackout = _blackoutEnd + Random.Range(tuning.blackoutPause.x, tuning.blackoutPause.y);
                SetBlackout(true);
            }
            if (_blackout)
            {
                if (now < _blackoutEnd) return;
                SetBlackout(false);
            }

            bool neonsDrop = Situation >= tuning.neonFrom;
            for (int i = 0; i < neons.Length; i++)
            {
                if (neonsDrop && now >= _neonNext[i] && now >= _neonEnd[i])
                {
                    _neonEnd[i] = now + Random.Range(tuning.neonDropSeconds.x, tuning.neonDropSeconds.y);
                    _neonNext[i] = _neonEnd[i] + Random.Range(NeonPause.x, NeonPause.y);
                }
                // During a dropout the tube stutters: dark most of the time, with short flashes.
                SetNeon(i, now < _neonEnd[i] && Random.value < 0.75f);
            }
        }

        private void SetBlackout(bool off)
        {
            if (_blackout == off) return;
            _blackout = off;
            foreach (var lamp in lamps)
                if (lamp != null) lamp.SetOff(off);
            for (int i = 0; i < neons.Length; i++) SetNeon(i, off);
        }

        private void SetNeon(int i, bool off)
        {
            if (i >= _neonOff.Length || _neonOff[i] == off || neons[i] == null) return;
            _neonOff[i] = off;
            neons[i].SetPropertyBlock(off ? _dark : null);
        }
    }
}
