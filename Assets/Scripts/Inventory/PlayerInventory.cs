using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory
{
    public Inventory MainInventory { get; private set; }
    public Player Player { get; private set; }

    public static readonly int InventorySlots = 18;
    public static readonly int HotbarSlots = 6;
    public static readonly int TotalSlots = 24;

    public PlayerInventory(Player player)
    {
        Player = player;
        MainInventory = new Inventory(24, "Inventory");
    }

    public bool IsHotbar(int slot)
    {
        // 0-5
        return slot >= 0 && slot < HotbarSlots;
    }
}
