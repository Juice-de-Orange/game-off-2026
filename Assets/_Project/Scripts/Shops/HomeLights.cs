using GameOff.Day;
using GameOff.Economy;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>
    /// P29: the lights on the home planet's night side follow the family: bright while everyone is well, dimmer the worse it gets,
    /// and flickering once someone is critical. Each death takes some of them for good.
    /// P111: also follows Household.Changed (the night's settlement), not only the morning.
    /// </summary>
    public class HomeLights : MonoBehaviour
    {
        private static readonly int Strength = Shader.PropertyToID("_EmissionStrength");
        private static readonly float[] ByCondition = { 1.6f, 1.25f, 0.85f, 0.5f };

        [SerializeField] private Renderer planet;
        [SerializeField] private DayManager day;

        private MaterialPropertyBlock _block;
        private float _level = 1.6f;
        private bool _flicker;
        private Household _household;

        private void OnEnable()
        {
            if (day != null) day.DayStarted += OnDay;
        }

        private void OnDisable()
        {
            if (day != null) day.DayStarted -= OnDay;
            Listen(null);
        }

        private void OnDay(int _)
        {
            Listen(PlayerEconomy.Instance != null ? PlayerEconomy.Instance.Household : null);
            Refresh();
        }

        private void Listen(Household household)
        {
            if (household == _household) return;
            if (_household != null) _household.Changed -= Refresh;
            _household = household;
            if (_household != null) _household.Changed += Refresh;
        }

        private void Refresh()
        {
            var family = _household;
            if (family == null) return;
            int worst = Household.Well, dead = 0;
            for (int i = 0; i < family.Count; i++)
            {
                if (family.IsDead(i)) dead++;
                else if (family.Condition(i) > worst) worst = family.Condition(i);
            }
            _level = Mathf.Max(0.15f, ByCondition[worst] - 0.35f * dead);
            _flicker = worst == Household.Critical;
        }

        private void Update()
        {
            if (planet == null) return;
            float flicker = _flicker ? Mathf.PerlinNoise(Time.time * 3f, 0f) * 0.7f + 0.3f : 1f;
            _block ??= new MaterialPropertyBlock();
            planet.GetPropertyBlock(_block);
            _block.SetFloat(Strength, _level * flicker);
            planet.SetPropertyBlock(_block);
        }
    }
}
