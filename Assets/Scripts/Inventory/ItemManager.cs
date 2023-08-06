using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public ItemLibrary library;

    private void Awake()
    {
        foreach (Item item in library.items)
        {
            items.Add(item.ID, item);
            if (item.ID == ItemID.Empty)
                Empty = item;
        }
    }

    public static Dictionary<ItemID, Item> items = new Dictionary<ItemID, Item>();
    public static Item Empty;
}
