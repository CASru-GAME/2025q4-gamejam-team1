using UnityEngine;

// 敵の「ステータス（命）」を管理するクラス
public class EnemyController2
{
    //変数 
    private int hp;
    private Transform transform; // 死亡時に自身を消すために保持

    // HPを外部から確認できるようにする
    public int CurrentHP => hp;

    // コンストラクタ:生成時に体(transform)と最大HPをもらう
    public EnemyController2(Transform selfTransform, int maxHP)
    {
        this.transform = selfTransform;
        this.hp = maxHP;
    }

    // 毎フレームの処理（外部のUpdateから呼んでもらう）
    public void OnUpdate()
    {
        // 今のところ毎フレームやることはない
    }

    // ダメージを受ける処理
    public void TakeDamage(int damage)
    {
        // HPを減らす
        hp -= damage;
        Debug.Log($"被ダメージ 現在のHP: {hp}");

        // HPが0以下になったら死亡処理
        if (hp <= 0)
        {
            Die();
        }
    }

    // 死亡時の処理
    private void Die()
    {
        Debug.Log("敵が死亡");
        //敵を削除する
        Object.Destroy(transform.gameObject);
    }
}