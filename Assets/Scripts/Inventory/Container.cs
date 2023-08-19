using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bare GameObject wrapper for an inventory
/// </summary>
public class Container : MonoBehaviour
{
    [SerializeField] private string containerName = "Container";
    [SerializeField] private int numSlots;
    [SerializeField] private ItemStack.InspectorStack[] startingItems;

    // DEBUG
    public InventoryGUI gui;

    public string Name { get; private set; }
    public Inventory Inventory { get; private set; }
    public int SlotCount { get; private set; }

    private void Awake()
    {
        Name = containerName;
        SlotCount = numSlots;
        Inventory = new Inventory(this);

        for (int i = 0; i < startingItems.Length; i++)
            Inventory.AddItem(startingItems[i]);

        gui.Init(Inventory);
    }
}
