using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    public int ItemID;
    public int Count;
    public GameObject Item;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("アイテムに接触");
        ItemObject item = other.GetComponent<ItemObject>();
        if (item != null)
        {
            ItemID = item.ItemID;
            Count = item.Count;
            Item = other.gameObject;
            Debug.Log("アイテムにID: " + ItemID);
            Debug.Log("アイテムにCount: " + Count);
        }
    }

}
