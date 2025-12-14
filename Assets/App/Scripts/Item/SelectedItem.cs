using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SelectedItem : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private ItemDatabase itemDatabase;
    private bool isEmpty;

    private void Awake()
    {
        isEmpty = true;
    }
    
    public void SetSprite(int itemID)
    {
        if (itemID == -1)
        {
            image.enabled = false;
            image.sprite = null;
            isEmpty = true;
            return;
        }
        image.enabled = true;
        image.sprite = itemDatabase.GetIcon(itemID);
        isEmpty = false;
    }

    private void Update()
    {
        if (isEmpty) return;
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        mousePosition.z = 0;
        transform.position = mousePosition;
    }
}
