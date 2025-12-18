using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;

public class StatusContoroller : MonoBehaviour
{
    public Status CurrentStatus;
    public GameObject Player;
    private HP hp;
    private Stamina stamina;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentStatus = Status.Born;
        hp = new HP(3);
        stamina = new Stamina(10);
        UnityEngine.Debug.Log("initialized");
        CurrentStatus = Status.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        if(CurrentStatus == Status.Idle)
        {
            if(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.D))
            {
                CurrentStatus = Status.Move;
                //if()
            }
        }
    }

    public enum Status
    {    
        Born, Idle, Move, Attack, Damaged, Inventory, Item, Dead, 
    }
}
