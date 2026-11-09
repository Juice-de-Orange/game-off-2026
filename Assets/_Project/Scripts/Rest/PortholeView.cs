using System.Collections;
using System.Collections.Generic;
using GameOff.Day;
using GameOff.Economy;
using UnityEngine;

namespace GameOff.Rest
{
    /// <summary>
    /// P28: the porthole in room 103. The view goes out to the home planet (its city lights follow the family, HomeLights) and
    /// after a moment a short thought about the family appears; it stays until the player turns away. P113: after the airlock another
    /// thought comes first.
    /// </summary>
    public class PortholeView : MonoBehaviour
    {
        [SerializeField] private SeatView seat;
        [SerializeField] private DayManager day;

        private Coroutine _think;

        private void OnEnable()
        {
            seat.SatDown += OnSatDown;
            seat.StandingUp += OnStandingUp;
        }

        private void OnDisable()
        {
            seat.SatDown -= OnSatDown;
            seat.StandingUp -= OnStandingUp;
        }

        private void OnSatDown() => _think = StartCoroutine(Think());

        private void OnStandingUp()
        {
            if (_think != null) StopCoroutine(_think);
            _think = null;
        }

        private IEnumerator Think()
        {
            yield return new WaitForSeconds(seat.Tuning.thoughtDelay);
            string haunting = Confiscation.Instance != null ? Confiscation.Instance.MoraleThought() : null;
            if (!string.IsNullOrEmpty(haunting))
            {
                seat.Hud.ShowLine(haunting, 0f);
                yield return new WaitForSeconds(seat.Tuning.subtitleSeconds);
            }
            seat.Hud.ShowLine(CurrentThought(), 0f);
            _think = null;
        }

        private string CurrentThought()
        {
            var economy = PlayerEconomy.Instance;
            var conditions = FamilyMood.Conditions(economy != null ? economy.Household : null);
            var names = new List<string>();
            if (economy != null && economy.HouseholdTuning != null)
                foreach (var member in economy.HouseholdTuning.members) names.Add(member.name);
            return seat.Tuning.Thought(conditions, names, day != null ? day.Day : 1);
        }
    }
}
