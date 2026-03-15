using System.Collections.Generic;
using BK;
using BK.Inventory;
using UnityEngine;

public class GridItem : Item
{
    public int width = 1;
    public int height = 1;
    public float weight = 0;
    public List<ItemAbility> itemAbilities;
}
