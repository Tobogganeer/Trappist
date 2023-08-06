using System; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    //public event Action<Inventory> OnInventoryChanged;

    public readonly int slotCount;
    public readonly ItemStack[] items;

    public Inventory(int slots)
    {
        slotCount = slots;
        items = new ItemStack[slots];
    }

    public ItemStack Get(int index)
    {
        return index >= 0 && index < slotCount ? items[index] : ItemStack.EmptyItem;
    }
}
