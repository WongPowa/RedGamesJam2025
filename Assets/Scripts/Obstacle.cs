using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float fallSpeed;
    
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
        if (collision.gameObject.CompareTag("Player"))
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayObstacleHit();
            CharacterMovement characterMovement = collision.gameObject.GetComponent<CharacterMovement>();
            if (characterMovement != null)
            {
                characterMovement.Stun(1.0f); // 1 second stun, adjust as needed
            }
            // Enable dynamic physics and bounce obstacle away from player
            rb.bodyType = RigidbodyType2D.Dynamic;
            Vector2 bounceDir = (rb.position - (Vector2)collision.transform.position).normalized;
            rb.AddForce(bounceDir * 5f + Vector2.up * 2f, ForceMode2D.Impulse); // Adjust force as needed
        }
    }
}
