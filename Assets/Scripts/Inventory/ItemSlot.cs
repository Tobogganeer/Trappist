using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// GUI for holding an item
public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler
{
    public Image itemImage;
    public TMPro.TMP_Text amountText;
    public RectTransform graphicRectTransform;
    [ReadOnly] public int slot;

    //Inventory inventory;
    InventoryGUI gui;
    public Inventory Inventory => gui.inventory;

    [HideInInspector]
    public float scale = 1f;

    public void Init(InventoryGUI gui, int slot)
    {
        this.gui = gui;
        this.slot = slot;

        gui.inventory.OnInventoryChanged += OnInventoryChanged;

        UpdateGraphics();
    }

    public ItemStack GetItemStack() => gui.inventory[slot];

    private void Update()
    {
        scale = Mathf.MoveTowards(scale, 1f, Time.deltaTime); // About 100ms
        graphicRectTransform.localScale = Vector3.one * scale;
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
        itemImage.color = stack.IsEmpty ? Color.clear : Color.white;
        amountText.text = stack.IsStackable && !stack.IsEmpty ? "x" + stack.Count.ToString() : string.Empty;
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
        if (gui != null && !GetItemStack().IsEmpty)
            gui.OnSlotDragStarted(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (gui != null)
            gui.OnSlotHovered(this);
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
            gui.OnSlotDragStopped(this, eventData);
    }
}
