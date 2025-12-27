using UnityEngine;

public class CraftTable : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        Inventory.Instance.SetIsOnCraft(true);
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        Inventory.Instance.SetIsOnCraft(false);
    }
}
