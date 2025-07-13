using UnityEngine;

public class Springboard : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float bounceForce = 20f;          
    //public float bounceForceIncrement = 2f;  
    public float minYDifference = 0.1f;
    private Animator anim;

    private bool hasBounced = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        bool isAbove = collision.transform.position.y > transform.position.y + minYDifference;

        if (isAbove)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
            anim.SetTrigger("bounce");
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySpringboard();
            //if (!hasBounced)
            //{
            //    //bounceForce += bounceForceIncrement;
            //    hasBounced = true;
            //}
        }
        else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2f);
            CharacterMovement characterMovement = collision.GetComponent<CharacterMovement>();
            if (characterMovement != null)
            {
                characterMovement.Stun(1.0f); // 1 second stun, adjust as needed
            }
        }
    }

    public void SetBounceForce(float force)
    {
        bounceForce = force;
        hasBounced = false; // Reset when force is updated
    }

    public void ResetBounce()
    {
        hasBounced = false;
    }
}
