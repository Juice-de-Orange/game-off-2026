using System.Collections.Generic;

namespace GameOff.Economy
{
    /// <summary>What customs did with one drone shipment (P110).</summary>
    public class DroneCustomsResult
    {
        public bool Caught;
        /// <summary>The contraband items customs kept (one entry per item sent).</summary>
        public readonly List<ItemData> Lost = new List<ItemData>();
    }
}
