using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack
{
    public static readonly ItemStack EmptyItem = new ItemStack(ItemID.Empty);


    private Item _item;
    public ItemID ID { get; private set; }
    public Item Item
    {
        get
        {
            if (_item == null)
                _item = ItemManager.items[ID];
            return _item;
        }
    } // TODO: Better error handling

    int _count;
    public int Count
    {
        get => _count;
        set
        {
            _count = Mathf.Clamp(value, 0, Item.MaxStackSize);
            if (_count <= 0)
            {
                ID = ItemID.Empty;
                _item = ItemManager.Empty;
                // Set empty
            }
        }
    }

    public ItemStack(ItemID id) : this(id, 1) { }
    public ItemStack(ItemID id, int count)
    {
        ID = id;
        Count = count;
    }


    public bool Empty => IsEmpty();
    public bool IsEmpty()
    {
        if (this == EmptyItem)
            return true;
        else if (ID != ItemID.Empty)
            return Count <= 0;
        return true;
    }

    public ItemStack Split(int amount)
    {
        int i = Mathf.Min(amount, Count);
        ItemStack itemstack = Copy();
        itemstack.Count = i;
        Count -= i;
        return itemstack;
    }

    public ItemStack Copy()
    {
        if (Empty)
        {
            return EmptyItem;
        }
        else
        {
            ItemStack itemstack = new ItemStack(ID, Count);
            return itemstack;
        }
    }
}
