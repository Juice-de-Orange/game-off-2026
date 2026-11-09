using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>
    /// P111: when the coin telescope jumps from the home planet to the home street and back. Pure, so the tests can check it.
    /// </summary>
    public static class TelescopeFocus
    {
        /// <summary>Zoomed in far enough and aimed at the planet: the view goes down to the street.</summary>
        /// <param name="planetFov">Current field of view on the planet.</param>
        /// <param name="yaw">Pan away from the planet's centre, degrees.</param>
        /// <param name="pitch">Tilt away from the planet's centre, degrees.</param>
        public static bool EntersStreet(float planetFov, float yaw, float pitch, HomeViewTuning t) =>
            planetFov <= t.enterFov + 0.001f && OffCentre(yaw, pitch) <= t.aimTolerance;

        /// <summary>Zoomed out beyond the street's widest view: back to the planet.</summary>
        /// <param name="streetFov">The street zoom after the wheel step, not yet clamped.</param>
        public static bool LeavesStreet(float streetFov, HomeViewTuning t) => streetFov > t.streetWideFov + 0.001f;

        /// <summary>Planet zoom after coming back from the street: just outside the jump, so it doesn't jump straight down again.</summary>
        public static float PlanetFovAfterStreet(HomeViewTuning t, float wideFov) => Mathf.Min(wideFov, t.enterFov * 1.25f);

        /// <summary>Degrees between the planet's centre and where the view points.</summary>
        public static float OffCentre(float yaw, float pitch) => Quaternion.Angle(Quaternion.identity, Quaternion.Euler(pitch, yaw, 0f));

        /// <summary>Black over the view during the focus blink: 0 → 1 at the middle → 0 at the end (progress 0..1).</summary>
        public static float BlinkAlpha(float progress) => 1f - Mathf.Abs(2f * Mathf.Clamp01(progress) - 1f);
    }
}
