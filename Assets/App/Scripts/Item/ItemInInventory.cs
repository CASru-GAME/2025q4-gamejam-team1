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

    public void ShowInfo()
    {
        infoPanel.SetActive(true);
    }

    public void HideInfo()
    {
        infoPanel.SetActive(false);
    }

    public void AddNew(int itemID, int newCount)
    {
        iconImage.sprite = itemDatabase.GetIcon(itemID);
        countText.text = newCount <= 1 ? "" : newCount.ToString();
        infoText.text = $"[{itemDatabase.GetName(itemID)}]\n{itemDatabase.GetItemTypeName(itemID)}\n重さ: {itemDatabase.GetMass(itemID)}\n{itemDatabase.GetSpecialStatusString(itemID)}\n{itemDatabase.GetDescription(itemID)}";
        iconObject.SetActive(true);
    }

    public void UpdateCount(int newCount)
    {
        countText.text = newCount.ToString();
    }

    public void GetEmpty()
    {
        iconImage.sprite = null;
        countText.text = "";
        infoText.text = "";
        iconObject.SetActive(false);
    }
}
