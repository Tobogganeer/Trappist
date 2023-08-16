using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : ScriptableObject
{
    public const int MaxPossibleStack = 999;

    //group
    //rarity
    //stack
    //id

    [SerializeField] string _name;
    [SerializeField] ItemID id;
    [SerializeField] ItemFlags flags;
    [SerializeField, Range(1, MaxPossibleStack)] int maxStackSize = 99;

    [Space]

    [SerializeField] Sprite sprite;
    [SerializeField] Sound.ID selectSound = Sound.ID.Item_Select_Generic;
    [SerializeField] Sound.ID dragSound = Sound.ID.Item_Drag_Generic;
    [SerializeField] Sound.ID dropSound = Sound.ID.Item_Drop_Generic;
    string description; // Unused currently

    public string Name => _name;
    public ItemID ID => id;
    public ItemFlags Flags => flags;
    public int MaxStackSize => maxStackSize;

    public Sprite Sprite => sprite;
    public Sound.ID SelectSound => selectSound;
    public Sound.ID DragSound => dragSound;
    public Sound.ID DropSound => dropSound;



    public bool Stackable => MaxStackSize > 1;


    public virtual string GetDisplayName(ItemStack stack) => stack.Item.Name;
    public virtual void OnUpdate(ItemStack stack, float dt, Player player, int slot, bool held) { }
    public virtual ItemStack OnFinishedUsing(ItemStack stack, Player player) => stack;
    public virtual void OnStoppedUsing(ItemStack stack, Player player, float timeUsed) { }
    public virtual void OnStartedUsing(ItemStack stack, Player player) { }
    public virtual ItemStack OnRMB(ItemStack stack, Player player) => stack;
}


// Moved out of Item class so I could have Item.Flags and use capitals etc
[System.Flags]
public enum ItemFlags
{
    None, // Tool, helmet, ammo etc (for specific slots)
    Armour = 1 << 0,
    Head = 1 << 1,
    Chest = 1 << 2,
    Legs = 1 << 3,
    Feet = 1 << 4,
    Hands = 1 << 5,
    Tool = 1 << 6,
    Weapon = 1 << 7,
    Medical = 1 << 8,
    Ammo = 1 << 9,

}

public enum ItemID
{
    Empty,
    P510,
    Hatchet,
    Compass,
    Bandage,
    Seed,
    Wood,
    Beancan
}

public static class ItemExtensions
{
    public static Item GetItem(this ItemID id)
    {
        return ItemManager.Items[id];
    }
}