using UnityEngine;

public class EnemyEntity : MonoBehaviour
{
    // 敵の移動速度
    [Header("移動設定")]
    [SerializeField] private float speed = 2.0f;
    private EnemyController1 controller;
    [SerializeField] private float minTime = 1.0f;
    [SerializeField] private float maxTime = 4.0f;

    // 敵の最大HP
    [Header("体力設定")]
    [SerializeField] private int maxHP = 5;
    private EnemyController2 controller2;

    // 敵の攻撃設定
    [Header("攻撃設定")]
    [SerializeField] private float attackRange = 1.0f; //どのくらい近づいたら攻撃するか
    [SerializeField] private float attackInterval = 2.0f; //攻撃の間隔

    // Hpをデバッグ表示するための変数
    [SerializeField] private int debugHp;

    // ドロップアイテム設定
    [Header("ドロップアイテム設定")]
    [SerializeField] private int dropItemID;
    [SerializeField] private int dropItemCount;

    void Awake()
    {
        // 自分のtransformと速度、うろつきの間隔、攻撃範囲、攻撃間隔を渡してEnemyController1を生成
        controller = new EnemyController1(this.transform, speed, minTime, maxTime, attackRange, attackInterval);
        // EnemyController2を生成、同時に中でEnemyHPも生成される。加えてアイテムドロップ情報も渡す
        controller2 = new EnemyController2(this.transform, maxHP, dropItemID, dropItemCount);
    }
    
    
    // Update is called once per frame
    void Update()
    {
        //EnemyController1 の毎フレーム処理を呼び出す
        controller.OnUpdate();  

        // 現在のHPを、インスペクターで見える変数に毎フレームコピーする
        debugHp = controller2.CurrentHP;
    }
    
    // プレイヤーが範囲（Trigger）に入った瞬間に呼ばれる
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 相手のタグが「Player」ならそれをターゲットにする
        if (other.CompareTag("Player"))
        {
            controller.SetTarget(other.gameObject);
        }
        
        // もしぶつかった相手のタグが「PlayerAttack」なら
        if (other.CompareTag("PlayerAttack"))
        {
            // EnemyController2にダメージを伝える
            controller2.TakeDamage(1);
        }
    }

    // プレイヤーが範囲（Trigger）から出て行った瞬間に呼び出される関数
    private void OnTriggerExit2D(Collider2D other)
    {
        // 出ていったのが「Player」ならClearTarget()を呼ぶ
        if (other.CompareTag("Player"))
        {
            controller.ClearTarget();
        }
    }
}
