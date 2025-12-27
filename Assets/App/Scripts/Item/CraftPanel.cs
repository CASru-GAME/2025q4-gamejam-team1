using UnityEngine;
using UnityEngine.UI;

public class CraftPanel : MonoBehaviour
{
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private GameObject[] iconObjects;
    [SerializeField] private Image[] iconImages;
    [SerializeField] private Text[] countTexts;
    [SerializeField] private GameObject[] infoPanels;
    [SerializeField] private Text[] infoTexts;
    [SerializeField] private Image maskImage;
    private int craftIndex;
    private int shopIndex;
    private bool[] isEmpty;
    private bool isShop;

    public void ShowInfo(int itemIndex)
    {
        if (isEmpty[itemIndex]) return;
        infoPanels[itemIndex].SetActive(true);
    }

    public void HideInfo(int itemIndex)
    {
        infoPanels[itemIndex].SetActive(false);
    }

    public void InitializeAsCraft(int craftIndex)
    {
        isShop = false;
        isEmpty = new bool[3];
        this.craftIndex = craftIndex;
        ItemAndCount[] requiredItems = itemDatabase.GetCraftRequiredItems(craftIndex);
        ItemAndCount resultItem = itemDatabase.GetCraftResultItem(craftIndex);
        SetupIcons(requiredItems, resultItem);
    }

    public void InitializeAsShop(int shopIndex)
    {
        isShop = true;
        isEmpty = new bool[3];
        this.shopIndex = shopIndex;
        ItemAndCount requiredCoins = itemDatabase.GetShopRequiredItems(shopIndex);
        ItemAndCount resultItem = itemDatabase.GetShopResultItem(shopIndex);
        SetupIcons(new ItemAndCount[] { requiredCoins }, resultItem);
    }

    private void SetupIcons(ItemAndCount[] requiredItems, ItemAndCount resultItem)
    {
        for (int i = 0; i < 2; i++)
        {
            if(i >= requiredItems.Length)
            {
                iconImages[i].enabled = false;
                countTexts[i].text = "";
                infoTexts[i].text = "";
                iconObjects[i].SetActive(false);
                isEmpty[i] = true;
                continue;
            }
            iconImages[i].enabled = true;
            iconImages[i].sprite = itemDatabase.GetIcon(requiredItems[i].ItemData.ID);
            countTexts[i].text = requiredItems[i].Count <= 1 ? "" : requiredItems[i].Count.ToString();
            infoTexts[i].text = $"[{itemDatabase.GetName(requiredItems[i].ItemData.ID)}]\n{itemDatabase.GetItemTypeName(requiredItems[i].ItemData.ID)}\n重さ: {itemDatabase.GetMass(requiredItems[i].ItemData.ID)}\n{itemDatabase.GetSpecialStatusString(requiredItems[i].ItemData.ID)}\n{itemDatabase.GetDescription(requiredItems[i].ItemData.ID)}";
            iconObjects[i].SetActive(true);
            isEmpty[i] = false;
        }

        iconImages[2].enabled = true;
        iconImages[2].sprite = itemDatabase.GetIcon(resultItem.ItemData.ID);
        countTexts[2].text = resultItem.Count <= 1 ? "" : resultItem.Count.ToString();
        infoTexts[2].text = $"[{itemDatabase.GetName(resultItem.ItemData.ID)}]\n{itemDatabase.GetItemTypeName(resultItem.ItemData.ID)}\n重さ: {itemDatabase.GetMass(resultItem.ItemData.ID)}\n{itemDatabase.GetSpecialStatusString(resultItem.ItemData.ID)}\n{itemDatabase.GetDescription(resultItem.ItemData.ID)}";
        iconObjects[2].SetActive(true);
        isEmpty[2] = false;
    }

    private void Update()
    {
        if(isShop)
            maskImage.enabled = !Inventory.Instance.CanBuy(shopIndex);
        else
            maskImage.enabled = !Inventory.Instance.CanCraft(craftIndex);
    }

    public void CraftOrBuy()
    {
        if(isShop)
        {
            if(!Inventory.Instance.CanBuy(shopIndex)) return;
            Inventory.Instance.BuyItem(shopIndex);
        }
        else
        {
            if(!Inventory.Instance.CanCraft(craftIndex)) return;
            Inventory.Instance.CraftItem(craftIndex);
        }
    }
}
