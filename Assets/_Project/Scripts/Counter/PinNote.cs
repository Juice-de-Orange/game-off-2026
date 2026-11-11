using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>P33: one note on the pinboard (bulletin, wanted list, market, memo) of today (PinBoardNotes).</summary>
    public class PinNote
    {
        public const string Bulletin = "bulletin";
        public const string Wanted = "wanted";
        public const string Market = "market";
        public const string Memo = "memo";

        public string Kind { get; }
        public string Title { get; }
        public string Date { get; }
        public string Body { get; }
        /// <summary>Wanted photos: name and passport photo (null = no photo on file).</summary>
        public IReadOnlyList<(string name, Texture2D photo)> Photos { get; }

        public PinNote(string kind, string title, string date, string body, IReadOnlyList<(string, Texture2D)> photos = null)
        {
            Kind = kind;
            Title = title;
            Date = date;
            Body = body ?? "";
            Photos = photos ?? new List<(string, Texture2D)>();
        }
    }
}
