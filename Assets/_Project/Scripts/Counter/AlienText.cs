using System.Text;

namespace GameOff.Counter
{
    /// <summary>
    /// Alien speech (P40): text shown as foreign glyphs, then "decrypted" from left to right with a cyan edge.
    /// The glyphs are Greek and Cyrillic letters from the fallback font. The same letter always becomes the same glyph.
    /// </summary>
    public static class AlienText
    {
        private const string Glyphs = "ΞΨΩΣΠΦΛΔΓЖЩЮЯДБЧ";
        private const string EdgeColor = "#5EC8FF"; // screen cyan (STYLE.md): the bubble is dark glass since P143

        public static string Scramble(string plain)
        {
            var glyphs = new StringBuilder(plain.Length);
            foreach (char c in plain)
                glyphs.Append(char.IsLetterOrDigit(c) ? Glyphs[char.ToUpperInvariant(c) % Glyphs.Length] : c);
            return glyphs.ToString();
        }

        /// <param name="revealed">Characters already translated.</param>
        /// <param name="edge">How many characters after those are shown translated but still cyan.</param>
        public static string Reveal(string plain, int revealed, int edge) => Reveal(plain, Scramble(plain), revealed, edge);

        /// <param name="glyphs">Scramble(plain), made once per line.</param>
        public static string Reveal(string plain, string glyphs, int revealed, int edge)
        {
            int done = System.Math.Clamp(revealed, 0, plain.Length);
            int edgeEnd = System.Math.Min(plain.Length, done + System.Math.Max(0, edge));
            var text = new StringBuilder(plain.Length + 32);
            text.Append(plain, 0, done);
            if (edgeEnd < plain.Length || edgeEnd > done)
            {
                text.Append("<color=").Append(EdgeColor).Append('>');
                text.Append(plain, done, edgeEnd - done);
                text.Append(glyphs, edgeEnd, glyphs.Length - edgeEnd);
                text.Append("</color>");
            }
            return text.ToString();
        }
    }
}
