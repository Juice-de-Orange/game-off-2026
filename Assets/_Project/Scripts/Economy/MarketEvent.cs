using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Economy
{
    /// <summary>
    /// A fixed market event of one day (P68), set in the day schedule: changes price, amount and buy price per label and the
    /// assortment of vendors. On its day it hangs on the counter's pinboard (MARKET note) and is announced at shift start;
    /// the day before only the barkeeper knows it (P65).
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Economy/Market Event", fileName = "Market_")]
    public class MarketEvent : ScriptableObject
    {
        [Tooltip("Line on the MARKET note at the pinboard.")]
        [TextArea(1, 3)] public string bulletinLine;
        [Tooltip("Loudspeaker line at shift start.")]
        [TextArea(1, 3)] public string announcement;
        [Tooltip("What the barkeeper whispers the evening before (P65). Must be true – it describes this event.")]
        [TextArea(1, 3)] public string rumour;
        public List<LabelEffect> labels = new List<LabelEffect>();
        public List<StockChange> stock = new List<StockChange>();
    }
}
