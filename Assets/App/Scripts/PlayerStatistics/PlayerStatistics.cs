using UnityEngine;
using System.Collections.Generic;

public class PlayerStatistics : MonoBehaviour
{
    public static PlayerStatistics instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    [Header("書込み禁止")]
    [SerializeField] private List<CountById> colletedItems;
    [SerializeField] private List<CountById> defeatedEnemies;
    public void CollectItem(int itemID, int count)
    {
        for (int i = 0; i < colletedItems.Count; i++)
        {
            if (colletedItems[i].id == itemID)
            {
                colletedItems[i].count += count;
                return;
            }
        }
        // アイテムが見つからなかった場合、新しいエントリを追加
        colletedItems.Add(new CountById { id = itemID, count = count });
    }

    public void DefeatEnemy(int enemyID, int count)
    {
        for (int i = 0; i < defeatedEnemies.Count; i++)
        {
            if (defeatedEnemies[i].id == enemyID)
            {
                defeatedEnemies[i].count += count;
                return;
            }
        }
        // 敵が見つからなかった場合、新しいエントリを追加
        defeatedEnemies.Add(new CountById { id = enemyID, count = count });
    }

    public Dictionary<int, int> GetCollectedItemsDictionary()
    {
        Dictionary<int, int> dict = new Dictionary<int, int>();
        foreach (var item in colletedItems)
        {
            dict[item.id] = item.count;
        }
        return dict;
    }

    public Dictionary<int, int> GetDefeatedEnemiesDictionary()
    {
        Dictionary<int, int> dict = new Dictionary<int, int>();
        foreach (var enemy in defeatedEnemies)
        {
            dict[enemy.id] = enemy.count;
        }
        return dict;
    }

    public int GetCollectedItemCount(int itemID)
    {
        foreach (var item in colletedItems)
        {
            if (item.id == itemID)
            {
                return item.count;
            }
        }
        return 0;
    }

    public int GetDefeatedEnemyCount(int enemyID)
    {
        foreach (var enemy in defeatedEnemies)
        {
            if (enemy.id == enemyID)
            {
                return enemy.count;
            }
        }
        return 0;
    }

    [System.Serializable]
    public struct CountById
    {
        public int id;
        public int count;
    }
}

