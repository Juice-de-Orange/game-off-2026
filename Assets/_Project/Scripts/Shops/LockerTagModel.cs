using System;
using GameOff.Economy;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>P32: the model shown in the evidence locker for goods with this label, when the item has no model of its own.</summary>
    [Serializable]
    public struct LockerTagModel
    {
        public ItemTag tag;
        public GameObject model;

        public LockerTagModel(ItemTag tag, GameObject model)
        {
            this.tag = tag;
            this.model = model;
        }
    }
}
