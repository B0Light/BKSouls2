using UnityEngine;
using System.Collections.Generic;

namespace BK.Inventory
{
    public class ItemInfo : ItemData
    {

        public int saleCost = 0;
        [HideInInspector] public bool purChaseWithItem = false;

        public GameObject itemModel;

        public List<ItemAbility> itemAbilities;
        public float weight = 0;
    }
}