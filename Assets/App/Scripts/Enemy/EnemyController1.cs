using UnityEngine;

//　敵の動作を制御するクラス

public class EnemyController1
{
    // 変数
    private float enemySpeed;
    private Transform transform; // 自身の場所を保持
    private GameObject target;  // 追いかける対象（プレイヤー）
    
    private float timer; //うろつき用タイマー
    private Vector2 strollDirection; //うろつき用方向ベクトル
    private float minChangeTime; //うろつきの忙しさ最小値
    private float maxChangeTime; //うろつきの忙しさ最大値

    private float attackRange; //攻撃する範囲
    private float attackInterval; //攻撃の間隔
    private float attackTimer; //攻撃用タイマー

    // コンストラクタ：生成時に必要な情報をもらう
    public EnemyController1(Transform selfTransform, float speed, float minTime, float maxTime, float attackRange, float attackInterval)
    {
        this.transform = selfTransform;
        this.enemySpeed = speed;
        this.timer = 0f;
        this.strollDirection = Vector2.zero;
        //うろつきの忙しさを決める関数を取得
        this.minChangeTime = minTime;
        this.maxChangeTime = maxTime;
        //攻撃範囲と攻撃間隔を取得
        this.attackRange = attackRange;
        this.attackInterval = attackInterval;
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
            // ターゲットへの方向を計算してnormalizedする
            Vector3 diff = target.transform.position - transform.position;
            moveDirection = diff.normalized;
            // 記憶したattackRangeを使って距離を判定
            float distance = Vector3.Distance(target.transform.position, transform.position);

            // 攻撃範囲内に入ったら攻撃を試みる
            if (distance <= attackRange)
            {
                TryAttack();
                moveDirection = Vector2.zero; // 攻撃中は動かない
            }
        }
       else //ターゲットがいないときにうろつく処理
        {
           UpdateStroll();
            moveDirection = strollDirection;
        }
        
        // 攻撃タイマーを進める（攻撃していなくても常にカウントダウンしておく）
        if (attackTimer > 0) 
        {
            attackTimer -= Time.deltaTime;
        }

        if (moveDirection != Vector2.zero)
        {
            // 見た目の向き（左右反転）だけを制御する
            if (moveDirection.x > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (moveDirection.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        
            // 移動を実行
            this.transform.Translate(moveDirection * enemySpeed * Time.deltaTime, Space.World);
        }
    }

    // 攻撃を試みる関数
    private void TryAttack()
    {
        // タイマーが0以下（待ち時間が終了）なら攻撃可能
        if (attackTimer <= 0)
        {
            PerformAttack();
            attackTimer = attackInterval; // タイマーをリセット
        }
    }

    // 実際の攻撃処理
    private void PerformAttack()
    {
        Debug.Log("敵の攻撃");
    }

    private void UpdateStroll()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            // ランダムな方向を生成
            strollDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            timer = Random.Range(minChangeTime, maxChangeTime);
        }
    }
    // プレイヤーをターゲットから外すための関数
    public void ClearTarget()
    {
        this.target = null;
    }
}