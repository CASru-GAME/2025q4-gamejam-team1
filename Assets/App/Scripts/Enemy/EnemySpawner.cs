using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int maxNearbyEnemies = 5;
    [SerializeField] private float detectionRadius = 5f;
    private float timer;

    private void Awake()
    {
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        int nearbyEnemyCount = 0;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (var collider in colliders)
            if (collider.CompareTag("Enemy"))
                nearbyEnemyCount++;

        if (nearbyEnemyCount < maxNearbyEnemies)
        {
            int newEnemyCount = maxNearbyEnemies - nearbyEnemyCount;
            for (int i = 0; i < newEnemyCount; i++)
            {
                var r = Random.Range(0f, 2f * Mathf.PI);
                var spawnPos = new Vector2(Mathf.Cos(r), Mathf.Sin(r)) * detectionRadius;
                Instantiate(enemyPrefab, (Vector2)transform.position + spawnPos, Quaternion.identity);
            } 
        }
    }
}
