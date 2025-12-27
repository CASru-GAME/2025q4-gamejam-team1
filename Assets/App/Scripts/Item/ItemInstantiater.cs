using Unity.VisualScripting;
using UnityEngine;

public class ItemInstantiater : MonoBehaviour
{
    [SerializeField] private GameObject itemObjectPrefab;
    private static ItemInstantiater instance;

    private void Awake()
    {
        instance = this;
    }

    public static GameObject InstantiateItem(Vector2 position, int itemID, int count)
    {
        GameObject item = Instantiate(instance.itemObjectPrefab, position, Quaternion.identity);
        item.GetComponent<ItemObject>().SetStatus(itemID, count);
        return item;
    }
}
