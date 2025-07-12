using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawn : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject obstaclePrefab;
    public GameObject springboardPrefab;

    [Header("Spawn Settings")]
    public float spawnCooldown = 2f;
    public int poolSize = 10;
    public float spawnYAbovePlayer = 10f;

    [Header("Springboard Settings")]
    public float springboardCooldown = 4f;
    public float minSpringboardSpacing = 15f; // minimum Y distance between springboards

    private float cooldownTimer = 1f;
    private float springboardTimer = 0f;
    private float screenWidthWorldUnits;
    private float lastSpringboardY = float.MinValue;
    private float lastVerticalSpeed;

    private Rigidbody2D playerRb;

    private Queue<GameObject> obstaclePool = new Queue<GameObject>();
    private Queue<GameObject> springboardPool = new Queue<GameObject>();

    private void Start()
    {
        Vector3 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
        screenWidthWorldUnits = screenBounds.x;

        playerRb = player.GetComponent<Rigidbody2D>();

        // Pre-instantiate both obstacle and springboard pools
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(obstaclePrefab);
            obj.SetActive(false);
            obstaclePool.Enqueue(obj);

            GameObject spring = Instantiate(springboardPrefab);
            spring.SetActive(false);
            springboardPool.Enqueue(spring);
        }
    }

    private void Update()
    {
        if (player == null || playerRb == null) return;

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(player.position);
        float verticalSpeed = playerRb.linearVelocity.y;

        // Update timers
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        if (springboardTimer > 0f) springboardTimer -= Time.deltaTime;

        // 1. Normal obstacle spawn if in bottom half
        if (viewportPos.y <= 0.5f && cooldownTimer <= 0f)
        {
            SpawnObstacle();
            cooldownTimer = spawnCooldown;
        }

        // 2. Springboard spawn if:
        // - In middle of screen
        // - Moving upward AND slowing down
        // - Not too close to last springboard
        if (viewportPos.y > 0.4f && viewportPos.y < 0.6f &&
            verticalSpeed > 0f && verticalSpeed < lastVerticalSpeed &&
            springboardTimer <= 0f &&
            Mathf.Abs(player.position.y - lastSpringboardY) >= minSpringboardSpacing)
        {
            SpawnSpringboard();
            springboardTimer = springboardCooldown;
            lastSpringboardY = player.position.y;
        }

        lastVerticalSpeed = verticalSpeed;
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

    private void SpawnSpringboard()
    {
        GameObject springboard = GetFromPool(springboardPool, springboardPrefab);
        if (springboard == null) return;

        Vector2 spawnPos = new Vector2(
            Random.Range(-screenWidthWorldUnits, screenWidthWorldUnits),
            player.position.y + spawnYAbovePlayer
        );

        springboard.transform.position = spawnPos;
        springboard.SetActive(true);
    }

    private GameObject GetFromPool(Queue<GameObject> pool, GameObject prefab)
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }

        // Optional: expand pool if needed
        GameObject newObj = Instantiate(prefab);
        return newObj;
    }

    public void ReturnToPool(GameObject obj, bool isSpringboard = false)
    {
        obj.SetActive(false);

        if (isSpringboard)
            springboardPool.Enqueue(obj);
        else
            obstaclePool.Enqueue(obj);
    }
}
