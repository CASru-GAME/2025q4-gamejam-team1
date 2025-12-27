using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Enemy/EnemyDatabase")]
public class EnemyDatabase : ScriptableObject
{
    [SerializeField] private GameObject[] enemyPrefabs;

    public string GetName(int enemyID)
    {
        foreach (var enemyPrefab in enemyPrefabs)
            if (enemyPrefab.GetComponent<EnemyEntity>().EnemyID == enemyID)
                return enemyPrefab.name;
        return null;
    }

    public Sprite GetIcon(int enemyID)
    {
        foreach (var enemyPrefab in enemyPrefabs)
            if (enemyPrefab.GetComponent<EnemyEntity>().EnemyID == enemyID)
                return enemyPrefab.GetComponent<SpriteRenderer>().sprite;
        return null;
    }
}