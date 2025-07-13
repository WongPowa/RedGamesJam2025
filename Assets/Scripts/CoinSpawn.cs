using System.Collections.Generic;
using UnityEngine;

public class CoinSpawn : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject coinPrefab;

    [Header("Spawner Settings")]
    public int poolSize = 10;
    public float verticalSpawnGap = 8f;
    public float spawnYOffset = 2f; // Distance above top of screen to spawn
    public float preSpawnHeightBuffer = 20f; // How far above the player to keep spawning coins

    [Header("Lane X Positions")]
    public float[] laneXPositions = new float[] { -2.5f, -0.8f, 0.8f, 2.5f };

    private float nextCoinY;
    private float screenTopY;

    private Queue<GameObject> coinPool = new Queue<GameObject>();

    private void Start()
    {
        screenTopY = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
        nextCoinY = player.position.y + 5f;

        // Initialize coin pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject coin = Instantiate(coinPrefab);
            coin.SetActive(false);
            coinPool.Enqueue(coin);
        }
    }

    private void Update()
    {
        if (player == null) return;

        screenTopY = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;

        // Spawn coins ahead of the player as they go up
        while (nextCoinY < player.position.y + preSpawnHeightBuffer)
        {
            SpawnCoin(nextCoinY);
            nextCoinY += verticalSpawnGap;
        }
    }

    private void SpawnCoin(float yPos)
    {
        GameObject coin = GetCoinFromPool();
        if (coin == null) return;

        float spawnX = laneXPositions[Random.Range(0, laneXPositions.Length)];
        float spawnY = screenTopY + spawnYOffset;

        coin.transform.position = new Vector2(spawnX, yPos); // Use yPos for consistent spacing
        coin.SetActive(true);
    }

    private GameObject GetCoinFromPool()
    {
        if (coinPool.Count > 0)
        {
            return coinPool.Dequeue();
        }

        // Optional: Expand pool if needed
        GameObject coin = Instantiate(coinPrefab);
        coin.SetActive(false);
        return coin;
    }

    public void ReturnCoinToPool(GameObject coin)
    {
        coin.SetActive(false);
        coinPool.Enqueue(coin);
    }
}
