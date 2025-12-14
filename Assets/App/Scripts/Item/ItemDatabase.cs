using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Item/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private ItemData[] itemDatas;

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
        return -1;
    }

    public int GetDefense(int itemID)
    {
        foreach (var itemData in itemDatas)
            if (itemData.ID == itemID)
                return itemData.Defence;
        return -1;
    }

    public int GetMoveSpeed(int itemID)
    {
        foreach (var itemData in itemDatas)
            if (itemData.ID == itemID)
                return itemData.MoveSpeed;
        return -1;
    }
}
