using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Item/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private ItemData[] itemDatas;
    [SerializeField] private CraftData[] craftDatas;
    [SerializeField] private CraftData[] shopDatas;

    public string GetName(int itemID)
    {
        foreach (var itemData in itemDatas)
            if (itemData.ID == itemID)
                return itemData.ItemName;
        return null;
    }

    public float GetMass(int itemID)
    {
        foreach (var itemData in itemDatas)
            if (itemData.ID == itemID)
                return itemData.Mass;
        return -1f;
    }

    public string GetItemTypeName(int itemID)
    {
        foreach (var itemData in itemDatas)
            if (itemData.ID == itemID)
                return itemData.ITypeName;
        return null;
    }

    public string GetDescription(int itemID)
    {
        foreach (var itemData in itemDatas)
            if (itemData.ID == itemID)
                return itemData.Description;
        return null;
    }

    public Sprite GetIcon(int itemID)
    {
        foreach (var itemData in itemDatas)
            if (itemData.ID == itemID)
                return itemData.Icon;
        return null;
    }

    public int GetMaxStack(int itemID)
    {
        foreach (var itemData in itemDatas)
            if (itemData.ID == itemID)
                return itemData.MaxStack;
        return -1;
    }

    public string GetSpecialStatusString(int itemID)
    {
        foreach (var itemData in itemDatas)
            if (itemData.ID == itemID)
                return itemData.SpecialStatusString;
        return null;
    }

    public bool IsItemType(int itemID, ItemData.ItemType itemType)
    {
        foreach (var itemData in itemDatas)
            if (itemData.ID == itemID)
                return itemData.IType == itemType;
        return false;
    }

    public int GetPower(int itemID)
    {
        foreach (var itemData in itemDatas)
            if (itemData.ID == itemID)
                return itemData.Power;
        return 0;
    }

    public int GetDefense(int itemID)
    {
        foreach (var itemData in itemDatas)
            if (itemData.ID == itemID)
                return itemData.Defence;
        return 0;
    }

    public int GetMoveSpeed(int itemID)
    {
        foreach (var itemData in itemDatas)
            if (itemData.ID == itemID)
                return itemData.MoveSpeed;
        return 0;
    }

    public ItemAndCount[] GetCraftRequiredItems(int craftID)
    {
        foreach (var craftData in craftDatas)
            if (craftData.CraftID == craftID)
                return craftData.RequiredItems;
        return null;
    }

    public ItemAndCount GetCraftResultItem(int craftID)
    {
        foreach (var craftData in craftDatas)
            if (craftData.CraftID == craftID)
                return craftData.ResultItem;
        return null;
    }

    public int GetCraftDataCount()
    {
        return craftDatas.Length;
    }

    public ItemAndCount GetShopRequiredItems(int shopID)
    {
        foreach (var shopData in shopDatas)
            if (shopData.CraftID == shopID)
                return shopData.RequiredItems[0];
        return null;
    }


    public ItemAndCount GetShopResultItem(int shopID)
    {
        foreach (var shopData in shopDatas)
            if (shopData.CraftID == shopID)
                return shopData.ResultItem;
        return null;
    }

    public int GetShopDataCount()
    {
        return shopDatas.Length;
    }
}

[Serializable]
class CraftData
{
    [SerializeField] private int craftID;
    [SerializeField] private ItemAndCount resultItem;
    [SerializeField] private ItemAndCount[] requiredItems;

    public int CraftID => craftID;
    public ItemAndCount ResultItem => resultItem;
    public ItemAndCount[] RequiredItems => requiredItems;
}

[Serializable]
public class ItemAndCount
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int count;

    public ItemData ItemData => itemData;
    public int Count => count;

    public ItemAndCount(ItemData itemData, int count)
    {
        this.itemData = itemData;
        this.count = count;
    }
}