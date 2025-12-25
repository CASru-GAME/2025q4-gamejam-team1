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
    [SerializeField] private Canvas quickCanvas;
    [SerializeField] private GameObject craftPanelPrefab;
    [SerializeField] private Canvas craftCanvas;
    private ItemModel selectedItemModel;
    [SerializeField] private SelectedItem selectedItem;
    [SerializeField] private Canvas selectedItemCanvas;
    private static Inventory instance;
    public static Inventory Instance => instance;

    private void Awake()
    {
        instance = this;
        itemModels = new ItemModel[capacity + 3];
        itemInInventory = new ItemInInventory[capacity + 3];
        
        for (int i = 0; i < capacity + 3; i++)
        {
            GameObject itemSpace = Instantiate(itemSpacePrefab, transform);
            if(i < quickSlotCount || i >= capacity)
                itemSpace.transform.SetParent(quickCanvas.transform);
            else
                itemSpace.transform.SetParent(inventoryCanvas.transform);
            if(i < quickSlotCount)
                itemSpace.GetComponent<RectTransform>().anchoredPosition = new Vector2(-400 + i * 50, -150);
            else if(i < capacity)
                itemSpace.GetComponent<RectTransform>().anchoredPosition = new Vector2(-400 + i % quickSlotCount * 50, 200 - i / quickSlotCount * 50);
            else
                itemSpace.GetComponent<RectTransform>().anchoredPosition = new Vector2(-400 + (i - capacity) * 50, -200);
            itemInInventory[i] = itemSpace.GetComponent<ItemInInventory>();
            itemInInventory[i].Initialize(i);
        }

        int craftCount = itemDatabase.GetCraftDataCount();
        for (int i = 0; i < craftCount; i++)
        {
            GameObject craftPanel = Instantiate(craftPanelPrefab, transform);
            craftPanel.transform.SetParent(craftCanvas.transform);
            craftPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-80 + i % 3 * 160, 150 - i / 3 * 50);
            craftPanel.GetComponent<CraftPanel>().Initialize(i + 1);
        }

        AddItem(1, 1);
        AddItem(2, 2);
        AddItem(3, 1);
        AddItem(4, 1);
        AddItem(5, 1);
        AddItem(6, 100);

        inventoryCanvas.enabled = false;
        selectedItemCanvas.enabled = false;
        craftCanvas.enabled = false;
    }

    private void Update()
    {
        var current = Keyboard.current;
        if (current == null) return;

        if (current.eKey.wasReleasedThisFrame)
            if (inventoryCanvas.enabled)
                HideInventory();
            else
                ShowInventory();

        UseItemInQuickSlot();
    }

    private void UseItemInQuickSlot()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame && itemModels[0] != null && itemDatabase.IsItemType(itemModels[0].ItemID, ItemData.ItemType.Medic) && quickSlotCount >= 1)
            RemoveItemInQuickSlot(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame && itemModels[1] != null && itemDatabase.IsItemType(itemModels[1].ItemID, ItemData.ItemType.Medic) && quickSlotCount >= 2)
            RemoveItemInQuickSlot(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame && itemModels[2] != null && itemDatabase.IsItemType(itemModels[2].ItemID, ItemData.ItemType.Medic) && quickSlotCount >= 3)
            RemoveItemInQuickSlot(2);
        else if (Keyboard.current.digit4Key.wasPressedThisFrame && itemModels[3] != null && itemDatabase.IsItemType(itemModels[3].ItemID, ItemData.ItemType.Medic) && quickSlotCount >= 4)
            RemoveItemInQuickSlot(3);
        else if (Keyboard.current.digit5Key.wasPressedThisFrame && itemModels[4] != null && itemDatabase.IsItemType(itemModels[4].ItemID, ItemData.ItemType.Medic) && quickSlotCount >= 5)
            RemoveItemInQuickSlot(4);
        else if (Keyboard.current.digit6Key.wasPressedThisFrame && itemModels[5] != null && itemDatabase.IsItemType(itemModels[5].ItemID, ItemData.ItemType.Medic) && quickSlotCount >= 6)
            RemoveItemInQuickSlot(5);
        else if (Keyboard.current.digit7Key.wasPressedThisFrame && itemModels[6] != null && itemDatabase.IsItemType(itemModels[6].ItemID, ItemData.ItemType.Medic) && quickSlotCount >= 7)
            RemoveItemInQuickSlot(6);
        else if (Keyboard.current.digit8Key.wasPressedThisFrame && itemModels[7] != null && itemDatabase.IsItemType(itemModels[7].ItemID, ItemData.ItemType.Medic) && quickSlotCount >= 8)
            RemoveItemInQuickSlot(7);
        else if (Keyboard.current.digit9Key.wasPressedThisFrame && itemModels[8] != null && itemDatabase.IsItemType(itemModels[8].ItemID, ItemData.ItemType.Medic) && quickSlotCount >= 9)
            RemoveItemInQuickSlot(8);
    }

    private void ShowInventory()
    {
        inventoryCanvas.enabled = true;
        selectedItemCanvas.enabled = true;
        craftCanvas.enabled = true;
    }

    private void HideInventory()
    {
        inventoryCanvas.enabled = false;
        selectedItemCanvas.enabled = false;
        craftCanvas.enabled = false;
        DropSelectedItem();
    }

    public void DropSelectedItem()
    {
        if (selectedItemModel != null)
        {
            Vector2 randomOffset = new(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(-0.2f, 0.2f));
            ItemInstantiater.InstantiateItem((Vector2)Camera.main.transform.position + randomOffset, selectedItemModel.ItemID, selectedItemModel.Count);
            selectedItemModel = null;
            selectedItem.SetSprite(-1);
        }
    }

    public void AddItem(int itemID, int count)
    {
        int tmpCount = count;
        for (int i = 0; i < capacity; i++)
            if (itemModels[i] != null && itemModels[i].ItemID == itemID && itemModels[i].Count < itemDatabase.GetMaxStack(itemID) && tmpCount > 0)
                if(tmpCount + itemModels[i].Count > itemDatabase.GetMaxStack(itemID))
                {
                    int diff = itemDatabase.GetMaxStack(itemID) - itemModels[i].Count;
                    tmpCount -= diff;
                    itemModels[i] = itemModels[i].AddCount(diff);
                    PlayerStatistics.instance.CollectItem(itemID, diff);
                    itemInInventory[i].UpdateCount(itemModels[i].Count);
                }
                else
                {
                    itemModels[i] = itemModels[i].AddCount(tmpCount);
                    PlayerStatistics.instance.CollectItem(itemID, tmpCount);
                    itemInInventory[i].UpdateCount(itemModels[i].Count);
                    tmpCount = 0;
                }

        for (int i = 0; i < capacity; i++)
            if (itemModels[i] == null && tmpCount > 0)
                if(tmpCount > itemDatabase.GetMaxStack(itemID))
                {
                    int diff = itemDatabase.GetMaxStack(itemID);
                    tmpCount -= diff;
                    itemModels[i] = ItemModel.AddNew(itemID, diff);
                    PlayerStatistics.instance.CollectItem(itemID, diff);
                    itemInInventory[i].AddNew(itemID, diff, i);
                }
                else
                {
                    itemModels[i] = ItemModel.AddNew(itemID, tmpCount);
                    PlayerStatistics.instance.CollectItem(itemID, tmpCount);
                    itemInInventory[i].AddNew(itemID, tmpCount, i);
                    return;
                }
        
        if (tmpCount == 0) return;
        Vector2 randomOffset = new(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(-0.2f, 0.2f));
        ItemInstantiater.InstantiateItem((Vector2)Camera.main.transform.position + randomOffset, itemID, tmpCount);
    }

    public bool CanRemoveItem(int itemID, int count)
    {
        int tmpCount = 0;
        for(int i = 0; i < itemModels.Length; i++)
            if (itemModels[i] != null && itemModels[i].ItemID == itemID)
                tmpCount += itemModels[i].Count;
        return tmpCount >= count;
    }

    public void RemoveItem(int itemID, int count)
    {
        int tmpCount = count;
        for (int i = 0; i < itemModels.Length; i++)
            if (itemModels[i] != null && itemModels[i].ItemID == itemID && tmpCount > 0)
                if(itemModels[i].Count <= tmpCount)
                {
                    tmpCount -= itemModels[i].Count;
                    itemModels[i] = itemModels[i].AddCount(-itemModels[i].Count);
                    itemModels[i] = null;
                    itemInInventory[i].GetEmpty();
                }
                else
                {
                    itemModels[i] = itemModels[i].AddCount(-tmpCount);
                    itemInInventory[i].UpdateCount(itemModels[i].Count);
                    return;
                }
    }

    private void RemoveItemInQuickSlot(int index)
    {
        itemModels[index] = itemModels[index].AddCount(-1);
        if (itemModels[index].Count == 0)
        {
            itemModels[index] = null;
            itemInInventory[index].GetEmpty();
        }
        else
            itemInInventory[index].UpdateCount(itemModels[index].Count);
    }

    public bool CanSwap(int index)
    {
        if(!inventoryCanvas.enabled) return false;
        if(index < capacity || selectedItemModel == null) return true;

        int specialIndex = index - capacity;
        if(specialIndex == 0 && itemDatabase.IsItemType(selectedItemModel.ItemID, ItemData.ItemType.Weapon)) return true;
        if(specialIndex == 1 && itemDatabase.IsItemType(selectedItemModel.ItemID, ItemData.ItemType.Armor)) return true;
        if(specialIndex == 2 && itemDatabase.IsItemType(selectedItemModel.ItemID, ItemData.ItemType.Foot)) return true;

        return false;
    }

    public void SwapItem(int index)
    {
        if(itemModels[index] != null && selectedItemModel != null && itemModels[index].ItemID == selectedItemModel.ItemID)
        {
            int sumCount = itemModels[index].Count + selectedItemModel.Count;
            if(sumCount <= itemDatabase.GetMaxStack(selectedItemModel.ItemID))
            {
                itemModels[index] = itemModels[index].AddCount(selectedItemModel.Count);
                itemInInventory[index].UpdateCount(itemModels[index].Count);
                selectedItemModel = null;
                selectedItem.SetSprite(-1);
                return;
            }
            else
            {
                int diff = sumCount - itemDatabase.GetMaxStack(selectedItemModel.ItemID);
                itemModels[index] = itemModels[index].AddCount(itemDatabase.GetMaxStack(selectedItemModel.ItemID) - itemModels[index].Count);
                itemInInventory[index].UpdateCount(itemModels[index].Count);
                selectedItemModel = ItemModel.AddNew(selectedItemModel.ItemID, diff);
            }
            return;
        }

        (itemModels[index], selectedItemModel) = (selectedItemModel, itemModels[index]);
        if (itemModels[index] == null)
            itemInInventory[index].GetEmpty();
        else
            itemInInventory[index].AddNew(itemModels[index].ItemID, itemModels[index].Count, index);
        selectedItem.SetSprite(selectedItemModel == null ? -1 : selectedItemModel.ItemID);
    }

    public int GetWeaponID()
    {
        return itemModels[capacity] != null ? itemModels[capacity].ItemID : -1;
    }

    public int GetArmorID()
    {
        return itemModels[capacity + 1] != null ? itemModels[capacity + 1].ItemID : -1;
    }

    public int GetFootID()
    {
        return itemModels[capacity + 2] != null ? itemModels[capacity + 2].ItemID : -1;
    }

    public bool CanCraft(int craftID)
    {
        ItemAndCount[] requiredItems = itemDatabase.GetCraftRequiredItems(craftID);
        foreach (var itemAndCount in requiredItems)
            if (!CanRemoveItem(itemAndCount.ItemData.ID, itemAndCount.Count))
                return false;
        return true;
    }

    public void CraftItem(int craftID)
    {
        ItemAndCount[] requiredItems = itemDatabase.GetCraftRequiredItems(craftID);
        foreach (var itemAndCount in requiredItems)
            RemoveItem(itemAndCount.ItemData.ID, itemAndCount.Count);
        ItemAndCount resultItem = itemDatabase.GetCraftResultItem(craftID);
        AddItem(resultItem.ItemData.ID, resultItem.Count);
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