using GameOff.Day;
using GameOff.Economy;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>
    /// P111: the three windows of the family house in the home street diorama (seen only through the coin telescope), one per family
    /// member in household order (Mom, Lena, Pip): bright while well or weak, dim while sick, flickering while critical, dark once dead.
    /// Follows Household.Changed and checks again every morning and whenever the diorama is switched on.
    /// </summary>
    public class HomeWindows : MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [Tooltip("The window quads in household order (Mom, Lena, Pip).")]
        [SerializeField] private Renderer[] windows;
        [SerializeField] private DayManager day;
        [SerializeField] private HomeViewTuning tuning;

        private readonly int[] _conditions = new int[3];
        private Household _household;
        private MaterialPropertyBlock _block;

        /// <summary>Brightness of a window (0 dark … 1 full) and whether it flickers, for one household condition.</summary>
        public static (float level, bool flicker) Light(int condition, HomeViewTuning t)
        {
            if (condition >= Household.Dead || condition < Household.Well) return (0f, false);
            if (condition == Household.Critical) return (t.criticalLevel, true);
            if (condition == Household.Sick) return (t.dimLevel, false);
            return (t.brightLevel, false);
        }

        /// <summary>The brightness shown right now; noise01 is the flicker noise (0..1), only used while critical.</summary>
        public static float Level(int condition, HomeViewTuning t, float noise01)
        {
            var (level, flicker) = Light(condition, t);
            return flicker ? level * Mathf.Lerp(t.flickerFloor, 1f, Mathf.Clamp01(noise01)) : level;
        }

        /// <summary>The colour of a window at a brightness: from dark glass to the lamp colour.</summary>
        public static Color Colour(float level, HomeViewTuning t) => Color.Lerp(t.darkColor, t.windowColor, Mathf.Clamp01(level));

        /// <summary>Condition of window i: a member who doesn't exist leaves a dark window, no household at all leaves them lit.</summary>
        public static int ConditionOf(Household family, int i) =>
            family == null ? Household.Well : i < family.Count ? family.Condition(i) : Household.Dead;

        private void OnEnable()
        {
            if (day != null) day.DayStarted += OnDay;
            Refresh();
        }

        private void OnDisable()
        {
            if (day != null) day.DayStarted -= OnDay;
            Listen(null);
        }

        private void OnDay(int _) => Refresh();

        private void Refresh()
        {
            var economy = PlayerEconomy.Instance;
            Listen(economy != null ? economy.Household : null);
            for (int i = 0; i < _conditions.Length; i++) _conditions[i] = ConditionOf(_household, i);
        }

        // PlayerEconomy creates its household on Awake, so the first look may come before it exists.
        private void Listen(Household household)
        {
            if (household == _household) return;
            if (_household != null) _household.Changed -= Refresh;
            _household = household;
            if (_household != null) _household.Changed += Refresh;
        }

        private void Update()
        {
            if (windows == null || tuning == null) return;
            _block ??= new MaterialPropertyBlock();
            for (int i = 0; i < windows.Length && i < _conditions.Length; i++)
            {
                if (windows[i] == null) continue;
                float noise = Mathf.PerlinNoise(Time.time * tuning.flickerSpeed, i * 7.3f);
                windows[i].GetPropertyBlock(_block);
                _block.SetColor(BaseColor, Colour(Level(_conditions[i], tuning, noise), tuning));
                windows[i].SetPropertyBlock(_block);
            }
        }
    }
}
