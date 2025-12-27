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
        Vector2 randomOffset = new(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(-0.2f, 0.2f));
        GameObject item = Instantiate(instance.itemObjectPrefab, position + randomOffset, Quaternion.identity);
        item.GetComponent<ItemObject>().SetStatus(itemID, count);
        return item;
    }
}
