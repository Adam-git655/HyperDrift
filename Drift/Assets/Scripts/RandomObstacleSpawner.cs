using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObstacleSpawner : MonoBehaviour
{
    public List<GameObject> obstaclePrefabs;  // List of different obstacle prefabs
    public int obstacleCount = 20;
    public Rect spawnArea;

    public float minSpawnDistance = 1.0f;  // Minimum distance between obstacles

    private List<Vector2> usedPositions = new List<Vector2>();

    void Start()
    {
        SpawnObstacles();
    }

    void SpawnObstacles()
    {
        int attempts = 0;

        while (usedPositions.Count < obstacleCount && attempts < obstacleCount * 10)
        {
            Vector2 spawnPos = new Vector2(
                Random.Range(spawnArea.xMin, spawnArea.xMax),
                Random.Range(spawnArea.yMin, spawnArea.yMax)
            );

            if (IsPositionValid(spawnPos))
            {
                // Pick a random prefab
                GameObject prefabToSpawn = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];

                Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                usedPositions.Add(spawnPos);
            }

            attempts++;
        }
    }

    bool IsPositionValid(Vector2 newPos)
    {
        foreach (Vector2 pos in usedPositions)
        {
            if (Vector2.Distance(pos, newPos) < minSpawnDistance)
            {
                return false;  // Too close to existing obstacle
            }
        }
        return true;
    }

    // Visual Debug for Spawn Area
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnArea.center, spawnArea.size);
    }
}
