using UnityEngine;

public class EnemyEntity : MonoBehaviour
{
    // 敵の移動速度
    [SerializeField] private float speed = 2.0f;
    private EnemyController1 controller;

    // 敵の最大HP
    [SerializeField] private int maxHP = 5;
    private EnemyController2 controller2;

    // Hpをデバッグ表示するための変数
    [SerializeField] private int debugHp;

    void Awake()
    {
        // 自分のtransformと速度を渡してEnemyController1を生成
        controller = new EnemyController1(this.transform, speed);
        // 脳（EnemyController2）を生成
        controller2 = new EnemyController2(this.transform, maxHP);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //EnemyController1 の毎フレーム処理を呼び出す
        controller.OnUpdate();

        controller2.OnUpdate();

        // 脳の現在のHPを、インスペクターで見える変数に毎フレームコピーする
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
