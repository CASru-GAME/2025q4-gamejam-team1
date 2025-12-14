using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    
    [SerializeField] private int capacity;
    [SerializeField] private int quickSlotCount;
    private ItemModel[] itemModels;
    private ItemInInventory[] itemInInventory;
    [SerializeField] private GameObject itemSpacePrefab;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private Canvas inventoryCanvas;

    private void Awake()
    {
        itemModels = new ItemModel[capacity];
        itemInInventory = new ItemInInventory[capacity];
        for (int i = 0; i < capacity; i++)
        {
            GameObject itemSpace = Instantiate(itemSpacePrefab, transform);
            itemSpace.transform.SetParent(inventoryCanvas.transform);
            if(i < quickSlotCount)
                itemSpace.GetComponent<RectTransform>().anchoredPosition = new Vector2(-400 + i * 50, -150);
            else
                itemSpace.GetComponent<RectTransform>().anchoredPosition = new Vector2(-400 + i % quickSlotCount * 50, 200 - i / quickSlotCount * 50);
            
            itemInInventory[i] = itemSpace.GetComponent<ItemInInventory>();
            itemInInventory[i].GetEmpty();
        }

        AddItem(1, 1);
        AddItem(2, 2);
        AddItem(3, 1);
        AddItem(4, 1);
        AddItem(5, 1);
        AddItem(6, 1);
    }

    private void Update()
    {
        var current = Keyboard.current;
        if (current == null) return;

        if (current.eKey.wasReleasedThisFrame)
            inventoryCanvas.enabled = !inventoryCanvas.enabled;
    }

    public void AddItem(int itemID, int count)
    {
        int tmpCount = count;
        for (int i = 0; i < itemModels.Length; i++)
            if (itemModels[i] != null && itemModels[i].ItemID == itemID && itemModels[i].Count < itemDatabase.GetMaxStack(itemID) && tmpCount > 0)
            {
                if(tmpCount + itemModels[i].Count > itemDatabase.GetMaxStack(itemID))
                {
                    int diff = itemDatabase.GetMaxStack(itemID) - itemModels[i].Count;
                    tmpCount -= diff;
                    itemModels[i] = itemModels[i].AddCount(diff);
                    itemInInventory[i].UpdateCount(itemModels[i].Count);
                }
                else
                {
                    itemModels[i] = itemModels[i].AddCount(tmpCount);
                    itemInInventory[i].UpdateCount(itemModels[i].Count);
                    tmpCount = 0;
                }
            }

        for (int i = 0; i < itemModels.Length; i++)
            if (itemModels[i] == null && tmpCount > 0)
            {
                if(tmpCount > itemDatabase.GetMaxStack(itemID))
                {
                    int diff = itemDatabase.GetMaxStack(itemID);
                    tmpCount -= diff;
                    itemModels[i] = ItemModel.AddNew(itemID, diff);
                    itemInInventory[i].AddNew(itemID, diff);
                }
                else
                {
                    itemModels[i] = ItemModel.AddNew(itemID, tmpCount);
                    itemInInventory[i].AddNew(itemID, tmpCount);
                    return;
                }
            }
    }
}

class ItemModel
{
    private readonly int itemID;
    private readonly int count;
    public int ItemID => itemID;
    public int Count => count;

    public ItemModel(int itemID, int count)
    {
        this.itemID = itemID;
        this.count = count;
    }

    public static ItemModel AddNew(int itemID, int count)
    {
        return new ItemModel(itemID, count);
    }

    public ItemModel AddCount(int diff)
    {
        return new ItemModel(itemID, count + diff);
    }
}