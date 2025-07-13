using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float fallSpeed;
    [SerializeField] private bool useGameSessionForGameOver = true;
    
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
    void Update()
    {
        if (transform.position.y < Camera.main.ViewportToWorldPoint(Vector3.zero).y - 1f)
        {
            FindAnyObjectByType<ObstacleSpawn>().ReturnToPool(gameObject, "Obstacle");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //if (collision.gameObject.CompareTag("Player"))
        //{
        //    // Trigger game over instead of immediate respawn
        //    if (useGameSessionForGameOver && GameSession.Instance != null)
        //    {
        //        GameSession.Instance.EndGame();
        //    }
        //    else
        //    {
        //        // Fallback: Direct respawn (old behavior)
        //        CharacterMovement characterMovement = collision.gameObject.GetComponent<CharacterMovement>();
        //        if (characterMovement != null)
        //        {
        //            characterMovement.RespawnPlayer();
        //        }
        //        else
        //        {
        //            Debug.LogWarning("No CharacterMovement component found on player!");
        //        }
        //    }
            
        //    Debug.Log("Player hit obstacle - game over!");
        //}
    }
}
