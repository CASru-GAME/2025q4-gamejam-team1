using UnityEngine;

public class SetActivecontroller : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("DisableSelf", 1f);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

     void DisableSelf()
    {
        gameObject.SetActive(false);
    }

}
