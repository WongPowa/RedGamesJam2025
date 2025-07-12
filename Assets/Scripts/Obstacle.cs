using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float fallSpeed;
    [SerializeField] private bool useGameSessionForRespawn = true;
    
    Rigidbody2D rb;
    void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.linearVelocityY = -fallSpeed * Time.deltaTime;
    }
    void OnBecameInvisible()
    {
        FindFirstObjectByType<ObstacleSpawn>().ReturnToPool(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Try to use GameSession for respawn first (provides game-specific behavior)
            if (useGameSessionForRespawn && GameSession.Instance != null)
            {
                GameSession.Instance.RespawnPlayer();
            }
            else
            {
                // Fallback to direct CharacterMovement respawn
                CharacterMovement characterMovement = collision.gameObject.GetComponent<CharacterMovement>();
                if (characterMovement != null)
                {
                    characterMovement.RespawnPlayer();
                }
                else
                {
                    Debug.LogWarning("No CharacterMovement component found on player!");
                }
            }
            
            Debug.Log("Player hit obstacle - respawning!");
        }
    }
}
