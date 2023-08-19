using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hotbar : MonoBehaviour
{
    public InventoryGUI gui;
    public ItemSlot[] slots;
    public bool flipScrollDirection;

    int numSlots;
    int selectedIndex; // Index into local slot array

    // Actual slot index in inventory, not local array
    public int SelectedSlot { get; private set; }

    private void Start()
    {
        numSlots = slots.Length;
    }

    private void Update()
    {
        int old = selectedIndex;

        float scroll = PlayerInputs.Scroll;

        if (flipScrollDirection)
            scroll *= -1f; // Flip it

        if (scroll > 0)
        {
            selectedIndex++;
            if (selectedIndex >= numSlots)
                selectedIndex = 0;
        }
        else if (scroll < 0)
        {
            selectedIndex--;
            if (selectedIndex < 0)
                selectedIndex = numSlots - 1;
        }

        // Number keys
        for (int i = 0; i < numSlots; i++)
        {
            if (PlayerInputs.NumberKeys[i].WasPressedThisFrame())
                selectedIndex = i;
        }


        // Change the colours
        slots[old].graphicState = ItemSlot.GraphicState.Default;
        slots[selectedIndex].graphicState = ItemSlot.GraphicState.Highlighted;
    }
}
