using UnityEngine;
using UnityEngine.UI;

public class ItemInInventory : MonoBehaviour
{
    [SerializeField]private ItemDatabase itemDatabase;
    [SerializeField] private GameObject iconObject;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text countText;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Text infoText;
    private int index;
    private Inventory inventory;
    private bool isEmpty;

    public void ShowInfo()
    {
        if (isEmpty) return;
        infoPanel.SetActive(true);
    }

    public void HideInfo()
    {
        infoPanel.SetActive(false);
    }

    public void AddNew(int itemID, int newCount, int newIndex)
    {
        iconImage.enabled = true;
        iconImage.sprite = itemDatabase.GetIcon(itemID);
        countText.text = newCount <= 1 ? "" : newCount.ToString();
        infoText.text = $"[{itemDatabase.GetName(itemID)}]\n{itemDatabase.GetItemTypeName(itemID)}\n重さ: {itemDatabase.GetMass(itemID)}\n{itemDatabase.GetSpecialStatusString(itemID)}\n{itemDatabase.GetDescription(itemID)}";
        iconObject.SetActive(true);
        index = newIndex;
        isEmpty = false;
    }

    public void UpdateCount(int newCount)
    {
        countText.text = newCount.ToString();
    }

    public void Initialize(Inventory inventory, int index)
    {
        this.inventory = inventory;
        this.index = index;
        GetEmpty();
    }

    public void GetEmpty()
    {
        iconImage.enabled = false;
        countText.text = "";
        infoText.text = "";
        isEmpty = true;
    }

    public void SwapItem()
    {
        inventory.SwapItem(index);
        if(isEmpty) HideInfo();
        else ShowInfo();
    }
}
