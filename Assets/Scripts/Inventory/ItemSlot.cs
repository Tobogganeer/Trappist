using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// GUI for holding an item
public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image itemImage;
    public TMPro.TMP_Text amountText;
    [ReadOnly] public int slot;

    //Inventory inventory;
    InventoryGUI gui;
    public Inventory Inventory => gui.inventory;

    public void Init(InventoryGUI gui, int slot)
    {
        this.gui = gui;
        this.slot = slot;

        gui.inventory.OnInventoryChanged += OnInventoryChanged;

        UpdateGraphics();
    }

    private void OnDestroy()
    {
        if (gui != null)
            gui.inventory.OnInventoryChanged -= OnInventoryChanged;
    }


    private void OnInventoryChanged(Inventory inventory)
    {
        UpdateGraphics();
    }

    void UpdateGraphics()
    {
        if (gui == null)
            return;

        ItemStack stack = gui.inventory[slot];
        itemImage.sprite = stack.Item.Sprite;
        itemImage.color = stack.Item.Sprite == null ? Color.clear : Color.white;
        amountText.text = stack.IsStackable ? "x" + stack.Count.ToString() : string.Empty;
    }


    /*
    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("Enter");
        // Graphic stuff
        //throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Graphic stuff
        //throw new System.NotImplementedException();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (gui != null)
            gui.OnSlotDragStarted(this);

        eventData.
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (gui != null)
            gui.OnSlotDragStopped(this);
    }
    */

    private void OnDisable()
    {
        if (gui != null)
            gui.ClearDragStart();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gui != null)
            gui.OnSlotDragStarted(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //if (gui != null)
        //    gui.OnSlotDragStopped(this);
        //throw new System.NotImplementedException();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (gui != null)
            gui.OnSlotDragStopped(this);
    }
}
