using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawn : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject obstaclePrefab;
    public GameObject springboardPrefab;
    public GameObject cloudPrefab;

    [Header("Spawn Settings")]
    public float spawnCooldown = 2f;
    public int poolSize = 10;
    public float spawnYAbovePlayer = 10f;

    [Header("Springboard Settings")]
    public float approachThreshold = 20f;
    [Header("Springboard Spawn Height Progression")]
    public float initialSpringboardHeight = 100f;
    public float heightMultiplier = 1.05f;

    private float nextSpringboardY;
    private float currentSpringboardGap;
    private float lastSpringboardY = float.MinValue;

    [Header("Cloud Settings")]
    public float cloudSpawnInterval = 3f;
    public float cloudMinYOffset = 1f;
    public float cloudMaxYOffset = 5f;
    public float cloudDriftSpeed = 0.5f;

    private float cooldownTimer = 1f;
    private float cloudTimer = 0f;
    private float screenWidthWorldUnits;

    private Rigidbody2D playerRb;

    private Queue<GameObject> obstaclePool = new Queue<GameObject>();
    private Queue<GameObject> springboardPool = new Queue<GameObject>();
    private Queue<GameObject> cloudPool = new Queue<GameObject>();

    private void Start()
    {
        Vector3 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
        screenWidthWorldUnits = screenBounds.x;

        playerRb = player.GetComponent<Rigidbody2D>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(obstaclePrefab);
            obj.SetActive(false);
            obstaclePool.Enqueue(obj);

            GameObject spring = Instantiate(springboardPrefab);
            spring.SetActive(false);
            springboardPool.Enqueue(spring);

            GameObject cloud = Instantiate(cloudPrefab);
            cloud.SetActive(false);
            cloudPool.Enqueue(cloud);
        }

        nextSpringboardY = player.position.y + initialSpringboardHeight;
        currentSpringboardGap = initialSpringboardHeight;
    }

    private void Update()
    {
        if (player == null || playerRb == null) return;

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(player.position);

        // Timers
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        if (cloudTimer > 0f) cloudTimer -= Time.deltaTime;

        // Obstacle spawn
        if (viewportPos.y <= 0.5f && cooldownTimer <= 0f)
        {
            //SpawnObstacle(); // Optional: enable if needed
            cooldownTimer = spawnCooldown;
        }

        // Springboard spawn based on vertical height milestones
        if (player.position.y + approachThreshold >= nextSpringboardY)
        {
            SpawnSpringboardAtHeight(nextSpringboardY);
            lastSpringboardY = nextSpringboardY;
            currentSpringboardGap *= heightMultiplier;
            nextSpringboardY += currentSpringboardGap;
        }

        // Cloud spawn
        if (cloudTimer <= 0f)
        {
            SpawnCloud();
            cloudTimer = cloudSpawnInterval;
        }
    }

    private void SpawnObstacle()
    {
        GameObject obstacle = GetFromPool(obstaclePool, obstaclePrefab);
        if (obstacle == null) return;

        Vector2 spawnPos = new Vector2(
            Random.Range(-screenWidthWorldUnits, screenWidthWorldUnits),
            player.position.y + spawnYAbovePlayer
        );

        obstacle.transform.position = spawnPos;
        obstacle.SetActive(true);
    }

    private void SpawnSpringboardAtHeight(float height)
    {
        GameObject springboard = GetFromPool(springboardPool, springboardPrefab);
        if (springboard == null) return;

        Vector2 spawnPos = new Vector2(
            Random.Range(-screenWidthWorldUnits, screenWidthWorldUnits),
            height
        );

        springboard.transform.position = spawnPos;
        springboard.SetActive(true);
    }

    private void SpawnCloud()
    {
        GameObject cloud = GetFromPool(cloudPool, cloudPrefab);
        if (cloud == null) return;

        Vector2 spawnPos = new Vector2(
            Random.Range(-screenWidthWorldUnits, screenWidthWorldUnits),
            player.position.y + spawnYAbovePlayer + Random.Range(cloudMinYOffset, cloudMaxYOffset)
        );

        cloud.transform.position = spawnPos;
        cloud.SetActive(true);

        if (cloud.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = new Vector2(Random.Range(-cloudDriftSpeed, cloudDriftSpeed), 0f);
        }
    }

    private GameObject GetFromPool(Queue<GameObject> pool, GameObject prefab)
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }

        GameObject newObj = Instantiate(prefab);
        return newObj;
    }

    public void ReturnToPool(GameObject obj, string type = "obstacle")
    {
        obj.SetActive(false);

        switch (type)
        {
            case "springboard":
                springboardPool.Enqueue(obj);
                break;
            case "cloud":
                cloudPool.Enqueue(obj);
                break;
            default:
                obstaclePool.Enqueue(obj);
                break;
        }
    }
}
