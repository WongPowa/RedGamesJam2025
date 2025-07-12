using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawn : MonoBehaviour
{
    public Transform player;
    public float spawnCooldown = 2f;
    public GameObject obstaclePrefab;
    public int poolSize = 10;

    private float cooldownTimer = 1f;
    private float screenWidthWorldUnits;

    private Queue<GameObject> obstaclePool = new Queue<GameObject>();

    private void Start()
    {
        Vector3 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
        screenWidthWorldUnits = screenBounds.x;

        // Pre-instantiate pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(obstaclePrefab);
            obj.SetActive(false);
            obstaclePool.Enqueue(obj);
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Check if player is in the bottom half of the screen
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(player.position);
        if (viewportPos.y > 0.5f) return;

        // Cooldown countdown
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        Spawn();
        cooldownTimer = spawnCooldown;
    }

    private void Spawn()
    {
        GameObject obstacle = GetPooledObstacle();
        if (obstacle == null) return;

        Vector2 spawnPos = new Vector2(
            Random.Range(-screenWidthWorldUnits, screenWidthWorldUnits),
            player.position.y + 10f // Adjust spawn height
        );

        obstacle.transform.position = spawnPos;
        obstacle.SetActive(true);
    }

    private GameObject GetPooledObstacle()
    {
        // If pool has available object, reuse it
        if (obstaclePool.Count > 0)
        {
            GameObject obj = obstaclePool.Dequeue();
            return obj;
        }

        // Optionally: expand pool if needed
        GameObject newObj = Instantiate(obstaclePrefab);
        return newObj;
    }

    public void ReturnToPool(GameObject obstacle)
    {
        obstacle.SetActive(false);
        obstaclePool.Enqueue(obstacle);
    }
}
