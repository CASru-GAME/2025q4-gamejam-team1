using UnityEngine;

public class ItemObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ItemDatabase itemDatabase;
    private int itemID;
    private int count;
    public int ItemID => itemID;
    public int Count => count;

    public void SetStatus(int id, int count)
    {
        itemID = id;
        this.count = count;
        spriteRenderer.sprite = itemDatabase.GetIcon(id);
    }

    public void PickUp()
    {
        Inventory.Instance.AddItem(itemID, count);
        Destroy(gameObject);
    }
}
