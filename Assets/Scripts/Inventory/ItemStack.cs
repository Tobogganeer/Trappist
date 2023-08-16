using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack
{
    public static readonly ItemStack Empty = new ItemStack(ItemID.Empty);


    public readonly ItemID ID;
    public Item Item => ItemManager.Items[ID];


    /*
    private ItemID _id;
    private Item _item;

    public ItemID ID
    {
        get { return _id; }
        set
        {
            _id = value; 
            _item = ItemManager.items[value];
        }
    }
    
    public Item Item
    {
        get
        {
            if (_item == null)
                _item = ItemManager.items[_id];
            return _item;
        }
    }
    */

    int _count;
    public int Count
    {
        get
        {
            // Just in case it has been changed in the inspector
            _count = Mathf.Clamp(_count, 0, Item.MaxStackSize);
            return _count;
        }
        set
        {
            _count = Mathf.Clamp(value, 0, Item.MaxStackSize);
        }
    } // Now won't change ID if count is 0
    public int MaxStackSize => Item.MaxStackSize;

    /*
    public int Count
    {
        get
        {
            // Just in case it has been changed in the inspector
            _count = Mathf.Clamp(_count, 0, Item.MaxStackSize);
            if (_count <= 0)
                ID = ItemID.Empty;
            return _count;
        }
        set
        {
            // Don't change the stack size of empty items
            if (ID == ItemID.Empty)
            {
                _count = 0;
                return;
            }

            _count = Mathf.Clamp(value, 0, Item.MaxStackSize);
            //if (_count <= 0)
            //    ID = ItemID.Empty;
        }
    }
    */

    public ItemStack(Item item) : this(item.ID, 1) { }
    public ItemStack(Item item, int count) : this(item.ID, count) { }
    public ItemStack(ItemID id) : this(id, 1) { }
    public ItemStack(ItemID id, int count)
    {
        ID = id;
        Count = count;
    }


    public bool IsEmpty => ID == ItemID.Empty || Count <= 0;
    public bool IsStackable => Item.Stackable;

    public ItemStack Split(int desiredCount)
    {
        int actualCount = Mathf.Min(desiredCount, Count);
        ItemStack itemstack = Copy();
        itemstack.Count = actualCount;
        Count -= actualCount;
        return itemstack;
    }

    public void Grow(int amount) => Count += amount;
    public void Shrink(int amount) => Count -= amount;

    public ItemStack Copy()
    {
        if (IsEmpty)
        {
            return Empty;
        }
        else
        {
            ItemStack itemstack = new ItemStack(ID, Count);
            return itemstack;
        }
    }

    public bool EqualTo(ItemStack other)
    {
        return this.ID == other.ID;
    }

    [System.Serializable]
    public class InspectorStack
    {
        // Used when you want to show/create ItemStacks in the inspector

        public ItemID id;
        [Range(0, 999)]
        public int count;

        public ItemStack ToItemStack()
        {
            return new ItemStack(id, count);
        }

        public static implicit operator ItemStack(InspectorStack stack)
        {
            return stack.ToItemStack();
        }
    }
}
