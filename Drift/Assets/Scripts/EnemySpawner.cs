using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnType
{
    public string name;
    public GameObject enemyPrefab;
    public int baseAmountPerSpawn;
    public int maxExtraEnemiesPerSpawn;
    public float baseSpawnInterval;
    public float spawnDistance;
    public float spawnStartTime;

    [HideInInspector] public float spawnTimer;
    [HideInInspector] public int currentAmountPerSpawn;
    [HideInInspector] public float currentSpawnInterval;
}

public class EnemySpawner : MonoBehaviour
{
    public List<EnemySpawnType> enemyTypes = new List<EnemySpawnType>();
    public Rect spawnableArea;
    private Camera mainCam;

    [Header("Difficulty Scaling")]
    public float difficultyRampUpTime = 150f;
    public float minSpawnIntervalMultiplier = 0.3f;


    // Start is called before the first frame update
    void Start()
    {
        if (mainCam == null)
            mainCam = Camera.main;

        foreach(var enemy in enemyTypes)
        {
            enemy.spawnTimer = 0f;
            enemy.currentSpawnInterval = enemy.baseSpawnInterval;
            enemy.currentAmountPerSpawn = enemy.baseAmountPerSpawn;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float elapsedTime = Time.timeSinceLevelLoad;
        float difficultyPercent = Mathf.Clamp01(elapsedTime/ difficultyRampUpTime);

        foreach(var enemy in enemyTypes)
        {
            if (elapsedTime < enemy.spawnStartTime)
                continue;

            enemy.currentSpawnInterval = Mathf.Lerp(enemy.baseSpawnInterval, enemy.baseSpawnInterval * minSpawnIntervalMultiplier, difficultyPercent);
            enemy.currentAmountPerSpawn = enemy.baseAmountPerSpawn + Mathf.FloorToInt(enemy.maxExtraEnemiesPerSpawn * difficultyPercent);

            enemy.spawnTimer -= Time.deltaTime;

            if (enemy.spawnTimer <= 0f)
            {
                SpawnEnemy(enemy);
                enemy.spawnTimer = enemy.currentSpawnInterval;
            }
        }
    }

    void SpawnEnemy(EnemySpawnType enemy)
    {
        for (int i = 0; i < enemy.currentAmountPerSpawn; i++)
        {
            Vector2 spawnPos = GetRandomPointOutsideCamView(enemy.spawnDistance);
            Instantiate(enemy.enemyPrefab, spawnPos, Quaternion.identity);
        }
    }

    Vector2 GetRandomPointOutsideCamView(float spawnDistance)
    {
        Vector2 spawnPos = Vector2.zero;

        Vector2 camPos = mainCam.transform.position;
        float camHeight = 2.0f * mainCam.orthographicSize;
        float camWidth = camHeight * mainCam.aspect;

        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0: //TOP
                spawnPos = new Vector2(
                    camPos.x + Random.Range(-camWidth / 2.0f, camWidth / 2.0f),
                    camPos.y + camHeight / 2.0f + spawnDistance
                );
                break;

            case 1: //BOTTOM
                spawnPos = new Vector2(
                    camPos.x + Random.Range(-camWidth / 2.0f, camWidth / 2.0f),
                    camPos.y - camHeight / 2.0f - spawnDistance
                );
                break;

            case 2: //RIGHT
                spawnPos = new Vector2(
                    camPos.x + camWidth / 2.0f + spawnDistance,
                    camPos.y + Random.Range(-camHeight / 2.0f, camHeight / 2.0f)
                );
                break;
            case 3: //LEFT
                spawnPos = new Vector2(
                    camPos.x - camWidth / 2.0f - spawnDistance,
                    camPos.y + Random.Range(-camHeight / 2.0f, camHeight / 2.0f)
                );
                break;
            default:
                break;
        }

        if (spawnPos.x > spawnableArea.xMax)
            spawnPos.x = spawnableArea.xMax;
        if (spawnPos.x < spawnableArea.xMin)
            spawnPos.x = spawnableArea.xMin;

        if (spawnPos.y > spawnableArea.yMax)
            spawnPos.y = spawnableArea.yMax;
        if (spawnPos.y < spawnableArea.yMin)
            spawnPos.y = spawnableArea.yMin;

        return spawnPos;
    }

    //visual debug for spawnable area
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnableArea.center, spawnableArea.size);
    }
}
