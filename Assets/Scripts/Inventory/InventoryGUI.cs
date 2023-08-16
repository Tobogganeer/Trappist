using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryGUI : MonoBehaviour
{
    public string containerName = "Container";

    public ItemSlot[] slots;
    public ItemStack.InspectorStack[] startingItems;

    public Inventory inventory;

    static ItemSlot dragFrom;


    private void Start()
    {
        inventory = new Inventory(slots.Length, containerName);

        for (int i = 0; i < slots.Length; i++)
            slots[i].Init(this, i);

        for (int i = 0; i < startingItems.Length; i++)
            inventory.AddItem(startingItems[i]);
    }

    private void Update()
    {
        if (PlayerInputs.LMB.WasReleasedThisFrame() && dragFrom != null)
            ClearDragStart();
    }

    public void OnSlotDragStarted(ItemSlot slot)
    {
        //Debug.Log($"Drag from {slot.Inventory.Name} ({slot.slot})");
        dragFrom = slot;
    }

    public void OnSlotDragStopped(ItemSlot slot)
    {
        // Called on new inventory
        //Debug.Log($"Drag to {slot.Inventory.Name} ({slot.slot})");

        // dragFrom belongs to another inventory
        if (dragFrom != null && slot != dragFrom)
        {
            dragFrom.Inventory.MoveItem(dragFrom.slot, slot.Inventory, slot.slot);
            //Debug.Log($"Moved to {slot.Inventory.Name} ({slot.slot}) from {dragFrom.Inventory.Name} ({dragFrom.slot})");
        }

        dragFrom = null;
    }

    public void ClearDragStart()
    {
        // Will be called a million times when an inventory is closed
        dragFrom = null;
    }
}
