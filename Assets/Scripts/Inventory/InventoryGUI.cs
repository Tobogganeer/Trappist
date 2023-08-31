using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryGUI : MonoBehaviour
{
    public TMPro.TMP_Text label;
    public UnityEngine.UI.Image draggedItemImage;
    public ItemSlot[] slots;

    static ItemSlot dragFrom;

    public Inventory inventory;
    //Vector2 dragOffset;


    public void Init(Inventory inventory)
    {
        this.inventory = inventory;
        for (int i = 0; i < slots.Length; i++)
            slots[i].Init(this, i);

        draggedItemImage.enabled = false;
        label.text = inventory.Name;
    }

    private void Update()
    {
        if (inventory == null)
            return;

        if (PlayerInputs.LMB.WasReleasedThisFrame() && dragFrom != null)
            ClearDragStart();
        if (draggedItemImage.enabled)
            draggedItemImage.rectTransform.position = UnityEngine.InputSystem.Mouse.current.position.value;// + dragOffset;
    }

    public void OnSlotDragStarted(ItemSlot slot, UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (inventory == null)
            return;

        //Debug.Log($"Drag from {slot.Inventory.Name} ({slot.slot})");
        dragFrom = slot;
        draggedItemImage.sprite = slot.itemImage.sprite;
        draggedItemImage.enabled = true;
        slot.GetItemStack().Item.DragSound.PlayLocal2D();

        // This works, but I actually don't like how the drag offset looks
        //dragOffset = (Vector2)slot.itemImage.rectTransform.position - UnityEngine.InputSystem.Mouse.current.position.value;
    }

    public void OnSlotHovered(ItemSlot slot)
    {
        if (inventory == null)
            return;

        // Not dragging any item
        if (dragFrom == null)
        {
            slot.scale = 1.1f; // Idk just hardcoded
            Sound.ID.SlotHover.PlayLocal2D();
            //slot.GetItemStack().Item.SelectSound.PlayLocal2D();
        }
    }

    public void OnSlotDragStopped(ItemSlot slot, UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (inventory == null)
            return;

        // Called on new inventory
        //Debug.Log($"Drag to {slot.Inventory.Name} ({slot.slot})");

        // dragFrom belongs to another inventory
        if (dragFrom != null && slot != dragFrom)
        {
            dragFrom.GetItemStack().Item.DropSound.PlayLocal2D();
            if (eventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Left)
            {
                // Normal move/swap on left mouse drag
                dragFrom.Inventory.MoveItem(dragFrom.slot, slot.Inventory, slot.slot);
            }
            else if (eventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Middle)
            {
                // Split stack in half on right mouse drag
                int amount = dragFrom.GetItemStack().Count;
                if (amount > 1) // If there is only one item, it will be moved.
                    amount /= 2; // Otherwise, move half
                dragFrom.Inventory.SplitItem(dragFrom.slot, amount, slot.Inventory, slot.slot);
            }
            else if (eventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Right)
            {
                dragFrom.Inventory.SplitItem(dragFrom.slot, 1, slot.Inventory, slot.slot);
            }

            //Debug.Log($"Moved to {slot.Inventory.Name} ({slot.slot}) from {dragFrom.Inventory.Name} ({dragFrom.slot})");
        }

        ClearDragStart();
    }

    public void ClearDragStart()
    {
        // Will be called a million times when an inventory is closed
        dragFrom = null;
        draggedItemImage.enabled = false;
    }


    public static void Open()
    {
        OpenContainer(Type.OnlyInventory, null);
    }

    public static void OpenContainer(Type type, Inventory other)
    {

    }

    public enum Type
    {
        OnlyInventory,
        _18Slot,
    }
}
