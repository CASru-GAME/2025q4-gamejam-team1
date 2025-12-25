using UnityEngine;

//　敵の動作を制御するクラス

public class EnemyController1
{
    // 変数
    private float enemySpeed;
    private Transform transform; // 自身の場所を保持
    private GameObject target;  // 追いかける対象（プレイヤー）

    // コンストラクタ：生成時に必要な情報をもらう
    public EnemyController1(Transform selfTransform, float speed)
    {
        this.transform = selfTransform;
        this.enemySpeed = speed;
    }


    // ターゲットをセットする関数 
    public void SetTarget(GameObject hitObject)
    {
        this.target = hitObject;
    }


    // 毎フレームの処理（外部のUpdateから呼んでもらう）
    public void OnUpdate()
    {
        Vector2 moveDirection = Vector2.zero;

        if (target != null)
        {    
            // 1. ターゲットへの方向を計算してnormalizedする
            Vector3 diff = target.transform.position - transform.position;
            moveDirection = diff.normalized;

            // 2. 見た目の向き（左右反転）だけを制御する
            // ターゲットが左にいたら画像を反転、右ならそのまま
            if (moveDirection.x > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (moveDirection.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        
        }
       else //ターゲットがいないときにうろつく処理
        {
            Debug.Log("ウロウロ");
        }
        // 移動を実行
        this.transform.Translate(moveDirection * enemySpeed * Time.deltaTime, Space.World);
    }

    // プレイヤーをターゲットから外すための関数
    public void ClearTarget()
    {
        this.target = null;
    }
}