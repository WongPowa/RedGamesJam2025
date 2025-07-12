using UnityEngine;

public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = 9.81f;
    
    [Header("Mobile Input")]
    [SerializeField] private bool useMobileInput = true;
    [SerializeField] private float touchSensitivity = 2f;
    
    private Rigidbody2D rb;
    private Vector2 velocity;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        rb.freezeRotation = true;
        rb.gravityScale = gravity / 9.81f;
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        float horizontalInput = 0f;
        bool jumpInput = false;
        
        if (useMobileInput && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            float screenWidth = Screen.width;
            float touchX = touch.position.x;
            
            if (touchX < screenWidth * 0.33f)
            {
                horizontalInput = -1f;
            }
            else if (touchX > screenWidth * 0.66f)
            {
                horizontalInput = 1f;
            }
            else if (touch.phase == TouchPhase.Began)
            {
                jumpInput = true;
            }
        }
        else
        {
            horizontalInput = Input.GetAxis("Horizontal");
            jumpInput = Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump");
        }
        
        velocity.x = horizontalInput * moveSpeed;
        
        if (jumpInput)
        {
            velocity.y = jumpForce;
            rb.linearVelocity = new Vector2(velocity.x, velocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(velocity.x, rb.linearVelocity.y);
        }
    }
    

    

} 