using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    [SerializeField] private IDAndCount[] initialItems;
    private (int ItemID, int Count)[] currentItems;

    private void Awake()
    {
        currentItems = new (int, int)[initialItems.Length];
        for (int i = 0; i < initialItems.Length; i++)
            currentItems[i] = (initialItems[i].ItemID, initialItems[i].Count);
    }

    public void UpdateItems((int ItemID, int Count)[] newItems)
    {
        currentItems = newItems;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        Inventory.Instance.SetItemsInBox(currentItems, this);
        Inventory.Instance.SetIsOnBox(true);
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        Inventory.Instance.SetIsOnBox(false);
    }
}

[Serializable]
class IDAndCount
{
    [SerializeField] private int itemID;
    [SerializeField] private int count;
    public int ItemID => itemID;
    public int Count => count;
}
