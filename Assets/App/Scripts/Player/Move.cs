using UnityEngine;

public class Move : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int wAxis;
    public int aAxis;
    public int sAxis;
    public int dAxis;
    private Rigidbody2D rb;
    public float moveForce = 10f;     // 加える力の大きさ
    public float maxSpeed = 5f;  
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
       if (wAxis == 1)
        {
            rb.AddForce(Vector2.up * moveForce, ForceMode2D.Force);
        }
        if (aAxis == 1)
        {
            rb.AddForce(Vector2.left * moveForce, ForceMode2D.Force);
        }
        if (sAxis == 1)
        {
            rb.AddForce(Vector2.down * moveForce, ForceMode2D.Force);
        }
        if (dAxis == 1)
        {
            rb.AddForce(Vector2.right * moveForce, ForceMode2D.Force);
        }
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        Vector2 v = rb.velocity;
        if (wAxis == 0 && sAxis == 0)
        v.y = 0;
        if (aAxis == 0 && dAxis == 0)
        v.x = 0;
        
        rb.velocity = v;


    }
    
    public void DebugLog()
    {
        Debug.Log("参照成功");
    }
}
