using UnityEngine;

public class EnemySearch : MonoBehaviour
{
    private EnemyController1 controller1;

    public void Initialize(EnemyController1 controller1)
    {
        this.controller1 = controller1;
    }

    // プレイヤーが範囲（Trigger）に入った瞬間に呼ばれる
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 相手のタグが「Player」ならそれをターゲットにする
        if (other.CompareTag("Player"))
            controller1.SetTarget(other.gameObject);
    }

    // プレイヤーが範囲（Trigger）から出て行った瞬間に呼び出される関数
    private void OnTriggerExit2D(Collider2D other)
    {
        // 出ていったのが「Player」ならClearTarget()を呼ぶ
        if (other.CompareTag("Player"))
            controller1.ClearTarget();
    }
}
