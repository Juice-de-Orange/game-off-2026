using GameOff.Day;
using GameOff.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace GameOff.Rest
{
    /// <summary>
    /// P29: the family photo taped to the locker in room 103. UI_Family_Photo holds three versions side by side (intact, faded
    /// with a crease, torn and stained); the photo shows the one for the family's mood and darkens a little with every step.
    /// Follows Household.Changed (the night's settlement) and checks again every morning.
    /// </summary>
    public class FamilyPhoto : MonoBehaviour
    {
        private const int Frames = 3;

        [SerializeField] private RawImage photo;
        [SerializeField] private DayManager day;
        [Tooltip("Colour of the photo at full brightness (the world-space UI is unlit: a bit below white so it sits in the dim room).")]
        [SerializeField] private Color tint = new Color(0.82f, 0.8f, 0.76f);

        private Household _household;

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
            Show(FamilyMood.Of(_household));
        }

        // PlayerEconomy creates its household on Awake, so the first look may come before it exists.
        private void Listen(Household household)
        {
            if (household == _household) return;
            if (_household != null) _household.Changed -= Refresh;
            _household = household;
            if (_household != null) _household.Changed += Refresh;
        }

        private void Show(int mood)
        {
            if (photo == null) return;
            int frame = FamilyMood.PhotoFrame(mood);
            photo.uvRect = new Rect(frame / (float)Frames, 0f, 1f / Frames, 1f);
            float b = FamilyMood.PhotoBrightness(mood);
            photo.color = new Color(tint.r * b, tint.g * b, tint.b * b, 1f);
        }
    }
}
