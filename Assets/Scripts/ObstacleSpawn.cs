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
    public float spawnYOffset = 2f;

    [Header("Springboard Settings")]
    public float approachThreshold = 20f;
    public float bounceForceIncrement = 2f;
    public float springboardDespawnDistance = 30f;
    public float initialSpringboardHeight = 100f;
    public float heightMultiplier = 1.05f;

    [Header("Cloud Settings")]
    public float cloudSpawnInterval = 3f;
    public float cloudDriftSpeed = 0.5f;

    [Header("Spawner Positions")]
    public float[] spawnXOffsets = new float[] { -6f, -2f, 2f, 6f }; // 4 predefined positions

    private enum PoolType { Obstacle, Springboard, Cloud }

    private Dictionary<PoolType, Queue<GameObject>> objectPools = new();
    private List<GameObject> activeSpringboards = new();
    private float cooldownTimer;
    private float cloudTimer;
    private float nextSpringboardY;
    private float currentSpringboardGap;
    private float globalBounceForce = 15f;
    private float screenTopY;
    private Rigidbody2D playerRb;
    private List<GameObject> activeObstacles = new();
    private List<GameObject> activeClouds = new();

    public static ObstacleSpawn Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerRb = player.GetComponent<Rigidbody2D>();
        //screenTopY = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;

        objectPools[PoolType.Obstacle] = new Queue<GameObject>();
        objectPools[PoolType.Springboard] = new Queue<GameObject>();
        objectPools[PoolType.Cloud] = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            objectPools[PoolType.Obstacle].Enqueue(CreateInactive(obstaclePrefab));
            objectPools[PoolType.Springboard].Enqueue(CreateInactive(springboardPrefab));
            objectPools[PoolType.Cloud].Enqueue(CreateInactive(cloudPrefab));
        }

        nextSpringboardY = player.position.y + initialSpringboardHeight;
        currentSpringboardGap = initialSpringboardHeight;
    }

    private void Update()
    {
        if (player == null || playerRb == null) return;

        cooldownTimer -= Time.deltaTime;
        cloudTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f)
        {
            SpawnObject(PoolType.Obstacle, obstaclePrefab);
            cooldownTimer = spawnCooldown;
        }

        if (player.position.y + approachThreshold >= nextSpringboardY)
        {
            SpawnSpringboard(nextSpringboardY);
            currentSpringboardGap *= heightMultiplier;
            nextSpringboardY += currentSpringboardGap;
        }

        if (cloudTimer <= 0f)
        {
            SpawnCloud();
            cloudTimer = cloudSpawnInterval;
        }

        DespawnOldSpringboards();
    }

    private void SpawnObject(PoolType type, GameObject prefab)
    {
        GameObject obj = GetFromPool(type, prefab);
        if (obj == null) return;

        float spawnX = spawnXOffsets[Random.Range(0, spawnXOffsets.Length)];
        obj.transform.position = new Vector2(spawnX, GetCurrentScreenTopY() + spawnYOffset);
        obj.SetActive(true);
        if (type == PoolType.Obstacle) activeObstacles.Add(obj);
        if (type == PoolType.Cloud) activeClouds.Add(obj);
    }

    private void SpawnSpringboard(float heightY)
    {
        GameObject springboard = GetFromPool(PoolType.Springboard, springboardPrefab);
        if (springboard == null) return;

        float spawnX = spawnXOffsets[Random.Range(0, spawnXOffsets.Length)];
        springboard.transform.position = new Vector2(spawnX, GetCurrentScreenTopY() + spawnYOffset);

        springboard.SetActive(true);
        if (springboard.TryGetComponent(out Springboard sb))
            sb.SetBounceForce(globalBounceForce);

        globalBounceForce += bounceForceIncrement;
        activeSpringboards.Add(springboard);
    }

    private void SpawnCloud()
    {
        GameObject cloud = GetFromPool(PoolType.Cloud, cloudPrefab);
        if (cloud == null) return;

        float spawnX = spawnXOffsets[Random.Range(0, spawnXOffsets.Length)];
        cloud.transform.position = new Vector2(spawnX, GetCurrentScreenTopY() + spawnYOffset);
        cloud.SetActive(true);

        if (cloud.TryGetComponent<Rigidbody2D>(out var rb))
            rb.linearVelocity = new Vector2(Random.Range(-cloudDriftSpeed, cloudDriftSpeed), 0f);
    }

    private void DespawnOldSpringboards()
    {
        for (int i = activeSpringboards.Count - 1; i >= 0; i--)
        {
            GameObject sb = activeSpringboards[i];
            if (sb.transform.position.y < player.position.y - springboardDespawnDistance)
            {
                sb.SetActive(false);
                objectPools[PoolType.Springboard].Enqueue(sb);
                activeSpringboards.RemoveAt(i);
            }
        }
    }

    private GameObject GetFromPool(PoolType type, GameObject prefab)
    {
        return objectPools[type].Count > 0 ? objectPools[type].Dequeue() : Instantiate(prefab);
    }

    private GameObject CreateInactive(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);
        obj.SetActive(false);
        return obj;
    }

    public void ReturnToPool(GameObject obj, string type)
    {
        obj.SetActive(false);
        switch (type.ToLower())
        {
            case "springboard":
                objectPools[PoolType.Springboard].Enqueue(obj);
                break;
            case "cloud":
                objectPools[PoolType.Cloud].Enqueue(obj);
                break;
            default:
                objectPools[PoolType.Obstacle].Enqueue(obj);
                break;
        }
    }
    private float GetCurrentScreenTopY()
    {
        return Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
    }


    public void ClearAllObstacles()
    {
        foreach (var obj in activeObstacles)
        {
            obj.SetActive(false);
            objectPools[PoolType.Obstacle].Enqueue(obj);
        }
        activeObstacles.Clear();
        foreach (var sb in activeSpringboards)
        {
            sb.SetActive(false);
            objectPools[PoolType.Springboard].Enqueue(sb);
        }
        activeSpringboards.Clear();
        foreach (var cloud in activeClouds)
        {
            cloud.SetActive(false);
            objectPools[PoolType.Cloud].Enqueue(cloud);
        }
        activeClouds.Clear();
    }
}
