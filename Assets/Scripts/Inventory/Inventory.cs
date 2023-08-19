using System; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    /// <summary>
    /// This inventory's container, if it has one
    /// </summary>
    public readonly Container Container;
    public readonly string Name;

    /// <summary>
    /// Called when the contents of the inventory have (potentially) changed.
    /// </summary>
    public event Action<Inventory> OnInventoryChanged;

    /// <summary>
    /// How many slots this inventory has. Identical to Items.Length.
    /// </summary>
    public readonly int SlotCount;
    //public readonly int MaxStackSize; // Slots like armour might be lower than item stack sizes

    //[field: SerializeField]
    // Don't show until I can stop adding/removing items
    //public ItemStack[] Items { get; private set; } // Should never have null elements
    private ItemStack[] Items { get; set; } // Should never have null elements
    public bool IsFull => NextOpenSlot() == -1;
    public bool IsEmpty => AllSlotsEmpty();


    public Inventory(int slots, string name = "Container")//, int maxStackSize = 999)
    {
        Name = name;
        SlotCount = slots;
        Items = new ItemStack[slots];
        //MaxStackSize = Mathf.Clamp(maxStackSize, 1, 999);
        Clear(); // Fill all slots with empty item
    }

    public Inventory(Container container)
    {
        Container = container;
        Name = container.Name;
        SlotCount = container.SlotCount;
        Items = new ItemStack[SlotCount];
        Clear();
    }


    /// <summary>
    /// Gets the item at the given <paramref name="index"/>, or ItemStack.Empty if it is out of range.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemStack Get(int index)
    {
        return index >= 0 && index < SlotCount ? Items[index] : ItemStack.Empty;
    }

    /// <summary>
    /// Sets the item at index <paramref name="index"/> to <paramref name="stack"/>.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="stack"></param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public void Set(int index, ItemStack stack)
    {
        if (index >= 0 && index < SlotCount)
            Items[index] = stack;
        else
            throw new IndexOutOfRangeException($"Tried to set slot with index {index}, while inventory size is {SlotCount}.");
    }

    public ItemStack this[int slot]
    {
        get => Get(slot);
        set => Set(slot, value);
    }


    public bool Contains(Item item) => Contains(item.ID);
    public bool Contains(Item item, int atLeast) => Contains(item.ID, atLeast);
    public int AmountOf(Item item) => AmountOf(item.ID);

    public bool Contains(ItemID item)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (Items[i].ID == item)
                return true;
        }

        return false;
    }
    public bool Contains(ItemID item, int atLeast)
    {
        return AmountOf(item) >= atLeast;
    }
    public int AmountOf(ItemID item)
    {
        int amount = 0;

        for (int i = 0; i < SlotCount; i++)
        {
            if (Items[i].ID == item)
                amount += Items[i].Count;
        }

        return amount;
    }

    /// <summary>
    /// Adds the <paramref name="stack"/> to the inventory.
    /// </summary>
    /// <param name="stack">The ItemStack to add. Count will be greater than 0 if not all items could be added.</param>
    public void AddItem(ItemStack stack)
    {
        if (stack == null || stack.IsEmpty)
            return;

        if (stack.IsStackable && Contains(stack.ID))
        {
            AddStackableItem(stack);

            // If we couldn't add it all to existing stacks
            if (!stack.IsEmpty)
            {
                PutStackInNextOpenSlot(stack);
            }
        }
        else
        {
            PutStackInNextOpenSlot(stack);
        }

        // If stack could not be added, it will simply not be empty. The calling code will handle it.
        OnInventoryChanged?.Invoke(this);
    }

    /// <summary>
    /// Moves as many items as possible from <paramref name="fromSlot"/> to <paramref name="toSlot"/>, or swaps slots.
    /// </summary>
    /// <param name="fromSlot"></param>
    /// <param name="toSlot"></param>
    public void MoveItem(int fromSlot, int toSlot)
    {
        if (fromSlot == toSlot)
            return;

        MoveItem(fromSlot, this, toSlot);
    }

    /// <summary>
    /// Moves as many items as possible from <paramref name="fromSlot"/> in this inventory to <paramref name="toSlot"/> in <paramref name="toInventory"/>, or swaps slots.
    /// </summary>
    /// <param name="fromSlot"></param>
    /// <param name="toInventory"></param>
    /// <param name="toSlot"></param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public void MoveItem(int fromSlot, Inventory toInventory, int toSlot)
    {
        if (fromSlot < 0 || toSlot < 0 || fromSlot >= SlotCount || toSlot >= toInventory.SlotCount)
        {
            throw new IndexOutOfRangeException($"fromSlot ({fromSlot}) & toSlot ({toSlot}) must be valid slots!");
        }

        ItemStack from = Get(fromSlot);
        ItemStack to = toInventory.Get(toSlot);

        if (from.IsEmpty && to.IsEmpty)
            return;

        if (from.EqualTo(to) && from.IsStackable)
        {
            MoveAsMuchAsPossible(from, to);
            if (from.IsEmpty)
                Set(fromSlot, ItemStack.Empty);
        }
        else
            Swap(fromSlot, toInventory, toSlot);

        OnInventoryChanged?.Invoke(this);
        if (this != toInventory)
            toInventory.OnInventoryChanged?.Invoke(toInventory);
    }

    public void SplitItem(int fromSlot, int amount, Inventory toInventory, int toSlot)
    {
        if (fromSlot < 0 || toSlot < 0 || fromSlot >= SlotCount || toSlot >= toInventory.SlotCount)
        {
            throw new IndexOutOfRangeException($"fromSlot ({fromSlot}) & toSlot ({toSlot}) must be valid slots!");
        }

        ItemStack from = Get(fromSlot);
        ItemStack to = toInventory.Get(toSlot);

        amount = Mathf.Clamp(amount, 0, from.Count);

        if (amount <= 0) return;

        if (from.IsEmpty && to.IsEmpty)
            return;

        // Non-stackable items are just moved normally
        if (!from.IsStackable)
            MoveItem(fromSlot, toInventory, toSlot);

        if (from.EqualTo(to))
        {
            MoveAsMuchAsPossible(from, to, amount);
            if (from.IsEmpty)
                Set(fromSlot, ItemStack.Empty);
        }
        else if (to.IsEmpty)
            toInventory.Set(toSlot, from.Split(amount));
        // Don't move if the slot is some other item, only if it is same or empty

        OnInventoryChanged?.Invoke(this);
        if (this != toInventory)
            toInventory.OnInventoryChanged?.Invoke(toInventory);
    }

    /// <summary>
    /// Returns a list contains copies of all non-empty items.
    /// </summary>
    /// <returns></returns>
    public List<ItemStack> GetAllItems()
    {
        List<ItemStack> items = new List<ItemStack>();
        for (int i = 0; i < Items.Length; i++)
        {
            if (!Items[i].IsEmpty)
            {
                items.Add(Items[i].Copy());
            }
        }

        return items;
    }

    /// <summary>
    /// Clears all items from the inventory.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < Items.Length; i++)
        {
            Items[i] = ItemStack.Empty;
        }

        OnInventoryChanged?.Invoke(this);
    }

    /// <summary>
    /// Sets the stack at index <paramref name="slot"/> to ItemStack.Empty.
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public ItemStack RemoveStackFromSlot(int slot)
    {
        if (slot < 0 || slot >= SlotCount)
        {
            throw new IndexOutOfRangeException($"slot ({slot}) must be a valid slot!");
        }

        ItemStack stack = Get(slot);
        Set(slot, ItemStack.Empty);

        OnInventoryChanged?.Invoke(this);

        return stack.IsEmpty ? ItemStack.Empty : stack;
    }

    /// <summary>
    /// Tries to collect <paramref name="desiredCount"/> items of type <paramref name="item"/>, and returns the collected stack.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="desiredCount"></param>
    /// <returns></returns>
    public ItemStack CollectAndRemove(ItemID item, int desiredCount)
    {
        ItemStack collected = new ItemStack(item, 0);
        int slotsCount = SlotCount;

        desiredCount = Mathf.Clamp(desiredCount, 0, collected.MaxStackSize);

        if (desiredCount == 0 || item == ItemID.Empty)
            return ItemStack.Empty;

        for (int i = slotsCount - 1; i >= 0; --i)
        {
            ItemStack stackToCheck = Get(i);
            if (stackToCheck.ID == item)
            {
                int neededAmount = desiredCount - collected.Count;
                ItemStack splitStack = stackToCheck.Split(neededAmount);
                collected.Count += splitStack.Count;
                if (collected.Count == desiredCount)
                {
                    break;
                }
            }
        }

        OnInventoryChanged?.Invoke(this);

        return collected;
    }


    // Private (don't call OnInventoryChanged) vvv

    private void PutStackInNextOpenSlot(ItemStack stack)
    {
        int nextOpenSlot = NextOpenSlot();

        if (nextOpenSlot == -1)
            return;

        Set(nextOpenSlot, stack.Copy());
        stack.Count = 0;
    }

    private void Swap(int thisSlot, Inventory other, int otherSlot)
    {
        // Assume slots are valid here (private code)

        ItemStack thisStack = Get(thisSlot);
        ItemStack otherStack = other.Get(otherSlot);

        if (otherStack.IsEmpty)
            RemoveStackFromSlot(thisSlot);
        else
            Set(thisSlot, otherStack.Copy());

        // Don't copy over empty stacks, just set them to be empty
        if (thisStack.IsEmpty)
            other.RemoveStackFromSlot(otherSlot);
        else
            other.Set(otherSlot, thisStack.Copy());
    }

    private int NextOpenSlot(int startingSlot = 0)
    {
        if (startingSlot >= SlotCount) return -1;

        for (int i = startingSlot; i < SlotCount; i++)
        {
            if (Items[i].IsEmpty)
                return i;
        }

        return -1;
    }

    private bool AllSlotsEmpty()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (!Get(i).IsEmpty)
                return false;
        }

        return true;
    }

    private void AddStackableItem(ItemStack stack)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            ItemStack existingStack = Get(i);
            if (existingStack.EqualTo(stack))
            {
                MoveAsMuchAsPossible(stack, existingStack);
                if (stack.IsEmpty)
                {
                    return;
                }
            }
        }
    }

    private void MoveAsMuchAsPossible(ItemStack from, ItemStack to, int limit = 0)
    {
        //int i = Mathf.Min(this.getInventoryStackLimit(), p_223373_2_.getMaxStackSize());
        //int maxCapacity = Mathf.Min(to.Item.MaxStackSize, MaxStackSize);

        if (!from.EqualTo(to))
            throw new ArgumentException("Cannot move items of different types together!", "from/to");

        int maxCapacity = to.Item.MaxStackSize;
        int maxAvailableToMove = Mathf.Min(from.Count, maxCapacity - to.Count);
        if (limit > 0)
            maxAvailableToMove = Mathf.Min(maxAvailableToMove, limit);
        if (maxAvailableToMove > 0)
        {
            to.Grow(maxAvailableToMove);
            from.Shrink(maxAvailableToMove);
        }
    }

    /*
    public ItemStack DecreaseStackSize(int index, int amount)
    {
        return Get(index).Split(amount);
    }
    */

    // OnInventoryChanged
}
