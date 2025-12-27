using UnityEngine;

public class Move : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int wAxis;
    public int aAxis;
    public int sAxis;
    public int dAxis;
    private Rigidbody2D rb;
    void Start()
    {
        rb = this.transform.GetComponent<Rigidbody2D> ();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        Vector2 dir = new Vector2(dAxis - aAxis, wAxis - sAxis);
        rb.velocity = dir.normalized * 5f;
    }
    
    public void DebugLog()
    {
        Debug.Log("参照成功");
    }
}
