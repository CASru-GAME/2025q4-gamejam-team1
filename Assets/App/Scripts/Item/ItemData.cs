using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    private static readonly Dictionary<ItemType, string> itemTypeNames = new()
    {
        { ItemType.Medic, "医療" },
        { ItemType.Armor, "装備-胴" },
        { ItemType.Foot, "装備-足" },
        { ItemType.Weapon, "武器" },
        { ItemType.Material, "素材" }
    };

    [SerializeField] private int id;
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;
    [SerializeField] private ItemType itemType;
    [SerializeField] private float mass;
    [SerializeField] private int maxStack;
    [SerializeField] private int healValue;
    [SerializeField] private int power;
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private int defence;
    [SerializeField] private int moveSpeed;
    [SerializeField] private string description;

    public int ID => id;
    public string ItemName => itemName;
    public Sprite Icon => icon;
    public ItemType IType => itemType;
    public float Mass => mass;
    public int MaxStack => maxStack;
    public int Power => power;
    public WeaponType WType => weaponType;
    public int Defence => defence;
    public int MoveSpeed => moveSpeed;
    public string Description => description;
    public string ITypeName => itemTypeNames[itemType];
    public int HealValue => healValue;
    public string SpecialStatusString 
    {
        get
        {
            return itemType switch
            {
                ItemType.Medic => $"回復量: {healValue}\n",
                ItemType.Weapon => $"攻撃力: {power}\n",
                ItemType.Armor => $"防御力: {defence}\n",
                ItemType.Foot => $"移動速度: {moveSpeed}\n",
                _ => ""
            };
        }
    }

    public enum ItemType
    {
        Medic,
        Armor,
        Foot,
        Weapon,
        Material
    }

    public enum WeaponType
    {
        Sword,
        Spear,
        Bow,
        Magic
    }
}