using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    [SerializeField] private (int ItemID, int Count)[] initialItems;
    private (int ItemID, int Count)[] currentItems;

    public void UpdateItems((int ItemID, int Count)[] newItems)
    {
        currentItems = newItems;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            Inventory.Instance.SetItemsInBox(currentItems, this);
    }
}
