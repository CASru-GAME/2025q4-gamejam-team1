using UnityEngine;

// 敵のステータスを管理するクラス
public class EnemyController2
{
    //変数 
    private EnemyHP hpSystem;
    private Transform transform; // 死亡時に自身を消すために保持
    private int itemID;
    private int itemCount;

    // HPを外部から確認できるようにする
    public int CurrentHP => hpSystem.CurrentHP;

    // コンストラクタ:生成時に体(transform)と最大HP、アイテムのIDと個数を受け取るようにする
    public EnemyController2(Transform selfTransform, int maxHP, int itemID, int itemCount)
    
    {
        
        this.transform = selfTransform;
        this.hpSystem = new EnemyHP(maxHP);
        this.itemID = itemID;
        this.itemCount = itemCount;
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
        hpSystem.Damage(damage);
        Debug.Log($"被ダメージ 現在のHP: {hpSystem.CurrentHP}");

        // HPが0以下になったら死亡処理
        if (hpSystem.CurrentHP <= 0)
        {
            Die();
        }
    }

    // 死亡時の処理
    private void Die()
    {
        //死亡時の座標を取得
        Vector2 dropPosition = transform.position;
        //static関数を呼び出してアイテムを生成
        ItemInstantiater.InstantiateItem(dropPosition, itemID, itemCount);

        Debug.Log($"敵が死亡。ID:{itemID}を{itemCount}個ドロップ");
        //敵を削除する
        Object.Destroy(transform.gameObject);
    }
}