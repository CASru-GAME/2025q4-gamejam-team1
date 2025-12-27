using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;



public class StatusContoroller : MonoBehaviour
{
    public Status CurrentStatus;
    [SerializeField] GameObject Player;
    [SerializeField] Transform AttackPos;
    [SerializeField] GameObject AttackPrefab;
    private Move moveScript;
    private PickUpItem pickUpItemScript;
    //private DoAttack attack;
    private HP hp;
    private Stamina stamina;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentStatus = Status.Born;
        hp = new HP(3);
        stamina = new Stamina(10);
        //attack = new DoAttack();
        moveScript = Player.GetComponent<Move>();
        pickUpItemScript = Player.GetComponent<PickUpItem>();
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
                    AttackPos.localPosition = new Vector3(0f, 0.5f, 0f);
                }
                if(Keyboard.current.aKey.isPressed)
                {
                    moveScript.aAxis = 1;
                    AttackPos.localPosition = new Vector3(-0.5f, 0f, 0f);
                }
                if(Keyboard.current.sKey.isPressed)
                {
                    moveScript.sAxis = 1;
                    AttackPos.localPosition = new Vector3(0f, -0.5f, 0f);
                }
                if(Keyboard.current.dKey.isPressed)
                {
                    moveScript.dAxis = 1;
                    AttackPos.localPosition = new Vector3(0.5f, 0f, 0f);
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

        else if(CurrentStatus == Status.Idle || CurrentStatus == Status.Move)
        {
            if(Keyboard.current.fKey.wasPressedThisFrame)
            {
                CurrentStatus = Status.Attack;
                Debug.Log("Attack");
                //Attack(); 攻撃用の関数は後で実装
                Instantiate(AttackPrefab, AttackPos.position, AttackPos.rotation);
                CurrentStatus = Status.Idle;
                //攻撃終了後Idleに戻す
            }
            if(Keyboard.current.eKey.wasPressedThisFrame)
            {
                CurrentStatus = Status.Inventory;
                Debug.Log("Inventory");
                Inventory.Instance.ShowInventory();
            }

            if(Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.digit2Key.wasPressedThisFrame ||Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.digit4Key.wasPressedThisFrame ||Keyboard.current.digit5Key.wasPressedThisFrame )
            {
                CurrentStatus = Status.ItemCunsume;
                Debug.Log("Item");
                CurrentStatus = Status.Idle;
            }

            if(Keyboard.current.rKey.wasPressedThisFrame)
            { 
                Inventory.Instance.AddItem(pickUpItemScript.ItemID, pickUpItemScript.Count);
            }
        }

        /*else if(CurrentStatus == Status.Inventory)
        {
            if(Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("Inventory to Idle");
                Inventory.Instance.ShowInventory();
                CurrentStatus = Status.Idle;
            }
        }*/

        else if(CurrentStatus == Status.Inventory)
        {
            if(Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("Inventory to Idle");
                CurrentStatus = Status.Idle;
                Inventory.Instance.HideInventory();
            }
        }

        else if(hp.CurrentHP == 0)
        {
            CurrentStatus = Status.Dead;
            Debug.Log("Dead");
            //死んだときの処理
            SceneManager.LoadScene("Title");
        }

    }

    public enum Status
    {    
        Born, Idle, Move, Attack, Damaged, Inventory, PickUpItem,ItemCunsume, Dead, 
    }
}
