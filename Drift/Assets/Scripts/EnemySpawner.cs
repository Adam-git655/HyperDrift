using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject dronePrefab;
    public float spawnInterval;
    public float spawnDistance;
    private float spawnTimer;
    private Camera mainCam;

    // Start is called before the first frame update
    void Start()
    {
        if (mainCam == null)
            mainCam = Camera.main;

        spawnTimer = spawnInterval;
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnTimer = spawnInterval;
        }
    }

    void SpawnEnemy()
    {
        Vector2 spawnPos = GetRandomPointOutsideCamView();
        Instantiate(dronePrefab, spawnPos, Quaternion.identity);
    }

    Vector2 GetRandomPointOutsideCamView()
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
                    camPos.y - camHeight / 2.0f - spawnDistance
                );
                break;
            case 1: //BOTTOM
                spawnPos = new Vector2(
                    camPos.x + Random.Range(-camWidth / 2.0f, camWidth / 2.0f),
                    camPos.y + camHeight / 2.0f + spawnDistance
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

        return spawnPos;
    }
}
