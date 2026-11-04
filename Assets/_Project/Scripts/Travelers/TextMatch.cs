using System;
using System.Text.RegularExpressions;

namespace GameOff.Travelers
{
    /// <summary>
    /// Text comparison for rules: case-insensitive and trimmed, so "Torvik" and "TORVIK " are the same. No loose substring
    /// matching: a ban on "ARMS" must not hit "FARMSTUFF" (that would be a wrong verdict in the game).
    /// </summary>
    public static class TextMatch
    {
        public static bool Same(string a, string b) =>
            !string.IsNullOrWhiteSpace(a) && !string.IsNullOrWhiteSpace(b)
            && string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);

        /// <summary>The phrase appears as whole word(s): "STIMULANT" matches "6x CHEM STIMULANT VIALS", "ARMS" not "FARMSTUFF".</summary>
        public static bool ContainsWords(string text, string phrase) =>
            !string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(phrase)
            && Regex.IsMatch(text, @"(?<![A-Za-z0-9])" + Regex.Escape(phrase.Trim()) + @"(?![A-Za-z0-9])", RegexOptions.IgnoreCase);

        /// <summary>A route like "TORVIK > KETRA STATION > KESTREL-9" passes through the stop (exact stop name).</summary>
        public static bool RouteHasStop(string route, string stop)
        {
            if (string.IsNullOrWhiteSpace(route)) return false;
            foreach (var s in route.Split('>'))
                if (Same(s, stop)) return true;
            return false;
        }
    }
}
