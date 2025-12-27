using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;

public class StatusContoroller : MonoBehaviour
{
    public Status CurrentStatus;
    [SerializeField] GameObject Player;
    private Move moveScript;
    private DoAttack attack;
    private HP hp;
    private Stamina stamina;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentStatus = Status.Born;
        hp = new HP(3);
        stamina = new Stamina(10);
        attack = new DoAttack();
        moveScript = Player.GetComponent<Move>();
        moveScript.DebugLog();
        UnityEngine.Debug.Log("initialized");
        CurrentStatus = Status.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        if(CurrentStatus == Status.Idle)
        {
            if(Keyboard.current.wKey.isPressed || Keyboard.current.aKey.isPressed ||Keyboard.current.sKey.isPressed || Keyboard.current.dKey.isPressed )
            {
                CurrentStatus = Status.Move;
                Debug.Log("currentStatus = Move");
                
                if(Keyboard.current.wKey.isPressed)
                {
                    moveScript.wAxis = 1;
                }
                if(Keyboard.current.aKey.isPressed)
                {
                    moveScript.aAxis = 1;
                }
                if(Keyboard.current.sKey.isPressed)
                {
                    moveScript.sAxis = 1;
                }
                if(Keyboard.current.dKey.isPressed)
                {
                    moveScript.dAxis = 1;
                }
            }
        }

        if(CurrentStatus == Status.Move)
        {
            if(Keyboard.current.wKey.wasReleasedThisFrame || Keyboard.current.aKey.wasReleasedThisFrame ||Keyboard.current.sKey.wasReleasedThisFrame || Keyboard.current.dKey.wasReleasedThisFrame )
            {
                CurrentStatus = Status.Idle;
                Debug.Log("currentStatus = Idle");
                
                if(Keyboard.current.wKey.wasReleasedThisFrame)
                {
                    moveScript.wAxis = 0;
                }
                if(Keyboard.current.aKey.wasReleasedThisFrame)
                {
                    moveScript.aAxis = 0;
                }
                if(Keyboard.current.sKey.wasReleasedThisFrame)
                {
                    moveScript.sAxis = 0;
                }
                if(Keyboard.current.dKey.wasReleasedThisFrame)
                {
                    moveScript.dAxis = 0;
                }
            }
        }

        if(CurrentStatus == Status.Idle || CurrentStatus == Status.Move)
        {
            if(Keyboard.current.fKey.wasPressedThisFrame)
            {
                CurrentStatus = Status.Attack;
                Debug.Log("Attack");
                //Attack(); 攻撃用の関数は後で実装
                CurrentStatus = Status.Idle;
                //攻撃終了後Idleに戻す
            }
            if(Keyboard.current.tabKey.wasPressedThisFrame)
            {
                CurrentStatus = Status.Inventory;
                Debug.Log("Inventory");
                //Inventory(); インベントリを開く関数(後で実装)
            }
        }

        if(CurrentStatus == Status.Inventory)
        {
            if(Keyboard.current.tabKey.wasPressedThisFrame)
            {
                CurrentStatus = Status.Idle;
                Debug.Log("Inventory to Idle");
                //CloseInventory(); インベントリを閉じる関数(後で実装)
            }
        }

        if(hp.CurrentHP == 0)
        {
            CurrentStatus = Status.Dead;
            Debug.Log("Dead");
            //死んだときの処理
        }

    }

    public enum Status
    {    
        Born, Idle, Move, Attack, Damaged, Inventory, Item, Dead, 
    }
}
