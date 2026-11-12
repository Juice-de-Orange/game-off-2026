using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>
    /// P143, without Unity objects (EditMode-tested): where the speech bubble stands next to the speaker's head, in view fractions
    /// (0..1, origin bottom left). Beside the head, right of it unless only the left has room, so it never covers the speaker; its
    /// tail's tip at the head when there is room above, else as high as the view allows. The bubble never leaves the view.
    /// </summary>
    public static class BubbleLayout
    {
        /// <summary>The desk view ends just over the travelers' heads: the bubble's top stays below this.</summary>
        public const float TopLimit = 0.96f;
        public const float Margin = 0.02f;
        /// <summary>Share of the view width between the head and the bubble, so it doesn't cover the speaker.</summary>
        public const float BesideHead = 0.07f;

        /// <summary>True: the bubble stands left of the speaker (its tail mirrored to point down-right).</summary>
        /// <param name="speakerX">The head, share of the view width.</param>
        /// <param name="width">The bubble, share of the view width.</param>
        /// <param name="wasLeft">The side it stood on so far: it stays there while it fits, so it doesn't flicker between sides.</param>
        public static bool LeftOfSpeaker(float speakerX, float width, bool wasLeft = false)
        {
            bool fitsRight = speakerX + BesideHead + width <= 1f - Margin;
            bool fitsLeft = speakerX - BesideHead - width >= Margin;
            if (wasLeft ? fitsLeft : fitsRight) return wasLeft;
            if (fitsRight) return false;
            if (fitsLeft) return true;
            return speakerX > 0.5f; // room on neither side: the wider one
        }

        /// <summary>The bubble's top-left corner.</summary>
        /// <param name="speaker">The head in view fractions.</param>
        /// <param name="size">The bubble in view fractions.</param>
        /// <param name="tailHeight">How far the tail reaches below the bubble, share of the view height (0 = no tail).</param>
        public static Vector2 TopLeft(Vector2 speaker, Vector2 size, float tailHeight, bool leftOfSpeaker)
        {
            float x = leftOfSpeaker ? speaker.x - BesideHead - size.x : speaker.x + BesideHead;
            x = Mathf.Clamp(x, Margin, Mathf.Max(Margin, 1f - Margin - size.x));
            float top = speaker.y + tailHeight + size.y; // the tail's tip at the head
            top = Mathf.Min(Mathf.Max(top, Margin + tailHeight + size.y), TopLimit);
            return new Vector2(x, top);
        }

        /// <summary>
        /// Centre of the tail on the bubble's bottom edge, from the bubble's left edge. Its tip (left end, or right end when mirrored)
        /// goes as close to the speaker as the edge allows. All values in the same unit (canvas units).
        /// </summary>
        /// <param name="speakerX">The head, from the bubble's left edge.</param>
        /// <param name="inset">Keeps the tail off the frame's corners.</param>
        public static float TailCentre(float speakerX, float width, float tailWidth, float inset, bool leftOfSpeaker)
        {
            float half = tailWidth * 0.5f;
            float min = inset + half, max = width - inset - half;
            if (min > max) return width * 0.5f; // a bubble too narrow for the tail between its corners
            return Mathf.Clamp(leftOfSpeaker ? speakerX - half : speakerX + half, min, max);
        }
    }
}
