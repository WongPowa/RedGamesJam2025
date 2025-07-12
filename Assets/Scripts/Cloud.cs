using UnityEngine;

public class Cloud : MonoBehaviour
{
    [Header("Slowdown Settings")]
    public float slowdownFactor = 0.5f; // 0.5 = reduce to 50% speed
    public float slowdownDuration = 1f; // How long the slowdown lasts

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null && rb.linearVelocity.y > 0f) // Only slow if moving upward
            {
                StartCoroutine(SlowPlayer(rb));
            }
        }
    }

    private System.Collections.IEnumerator SlowPlayer(Rigidbody2D rb)
    {
        float originalY = rb.linearVelocity.y;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, originalY * slowdownFactor);

        yield return new WaitForSeconds(slowdownDuration);

        // Optionally restore original speed or keep new velocity
        // rb.velocity = new Vector2(rb.velocity.x, originalY);
    }
}
