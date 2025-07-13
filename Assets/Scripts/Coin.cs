using UnityEngine;
using UnityEngine.SceneManagement;

public class Coin : MonoBehaviour
{
    private float floatAmplitude = 0.25f; // Height of bobbing
    private float floatFrequency = 2f;     // Speed of bobbing
    private Vector3 startPos;

    private void OnEnable()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Floating animation
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Optional: Add to score system here
        

        // Return coin to pool
        FindAnyObjectByType<CoinSpawn>().ReturnCoinToPool(gameObject);
    }
}
