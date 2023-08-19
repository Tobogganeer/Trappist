using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// GUI for holding an item
public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler
{
    public Image itemImage;
    public Image backgroundImage;
    public TMPro.TMP_Text amountText;
    public RectTransform graphicRectTransform;
    [ReadOnly] public int slot;
    [ReadOnly] public GraphicState graphicState = GraphicState.Default;
    GraphicState oldState;

    //Inventory inventory;
    InventoryGUI gui;
    public Inventory Inventory => gui.inventory;

    [HideInInspector]
    public float scale = 1f;

    public void Init(InventoryGUI gui, int slot)
    {
        this.gui = gui;
        this.slot = slot;

        Inventory.OnInventoryChanged += OnInventoryChanged;

        UpdateGraphics();
    }

    public ItemStack GetItemStack() => Inventory[slot];

    private void Update()
    {
        scale = Mathf.MoveTowards(scale, 1f, Time.deltaTime); // About 100ms
        graphicRectTransform.localScale = Vector3.one * scale;

        if (graphicState != oldState)
        {
            oldState = graphicState;
            backgroundImage.color = StateColour(graphicState);
        }
    }

    private void OnDestroy()
    {
        if (gui != null && gui.inventory != null)
            Inventory.OnInventoryChanged -= OnInventoryChanged;
    }




    private void OnInventoryChanged(Inventory inventory)
    {
        UpdateGraphics();
    }

    void UpdateGraphics()
    {
        if (gui == null)
            return;

        ItemStack stack = Inventory[slot];
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

    public enum GraphicState
    {
        /// <summary>
        /// Default - Grey
        /// </summary>
        Default,
        /// <summary>
        /// Hotbar - Blue
        /// </summary>
        Highlighted,
        /// <summary>
        /// Inventory - Yellow
        /// </summary>
        Selected
    }


    // Hardcoded because I feel like it
    static readonly Color _defaultColour = new Color(100f / 255f, 100f / 255f, 100f / 255f, 1.0f);
    static readonly Color _highlightedColour = new Color(127f / 255f, 170f / 255f, 188f / 255f, 1.0f);
    static readonly Color _selectedColour = new Color(224f / 255f, 215f / 255f, 133f / 255f, 1.0f);
    public static Color StateColour(GraphicState state)
    {
        return state switch
        {
            GraphicState.Default => _defaultColour,
            GraphicState.Highlighted => _highlightedColour,
            GraphicState.Selected => _selectedColour,
            _ => throw new System.NotImplementedException(),
        };
    }
}
