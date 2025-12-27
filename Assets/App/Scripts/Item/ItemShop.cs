using UnityEngine;

public class ItemShop : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        Inventory.Instance.SetIsOnShop(true);
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        Inventory.Instance.SetIsOnShop(false);
    }
}
