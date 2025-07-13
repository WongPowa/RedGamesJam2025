using UnityEngine;
using TMPro;
using System.Collections;

public enum movementState
{
    Jumping,
    Falling,
    Neutral,
    Dashing,
}

public enum characterBuds
{
    Biggie,
    Tappy,
    Bam,
    Ogu,
}

public class CharacterMovement : MonoBehaviour
{
    public static CharacterMovement Instance { get; private set; }

    const float FORCEMAGNITUDE = 7f;
    [SerializeField] private Vector3 moveVelocity;
    public movementState currMovementState;
    public CharacterAnim charAnim {  get; private set; }

    private characterBuds currBud = characterBuds.Biggie; // Default character bud, can be changed later
    private Vector2 originPos;
    private Vector3 spawnPoint;
    private TouchManager touchManager;
    private Rigidbody2D rigidBody2D;
    private bool isStunned = false;
    private float stunTimer = 0f;
    private float stunFallVelocity = -10f;

    // Dash cooldown
    [Header("Dash Settings")]
    [SerializeField] private float dashPower = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 2f;
    private bool canDash = true;
    private bool isDashing = false;

    public float launchSpeed = 20f;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        rigidBody2D.bodyType = RigidbodyType2D.Kinematic; // Temporarily Disables RigidBody on Start
        touchManager = TouchManager.Instance;
        originPos = rigidBody2D.position;
        spawnPoint = transform.position; // Store initial spawn point
        currMovementState = movementState.Neutral;
        charAnim = GetComponent<CharacterAnim>();
    }

    public void SetVelocity(Vector3 moveVelocity)
    {
        this.moveVelocity = moveVelocity;
        rigidBody2D.linearVelocity = moveVelocity;
    }

    public void EnableRigidBody()
    {
        rigidBody2D.bodyType = RigidbodyType2D.Dynamic;
        LaunchCharacter();
    }

    public void LaunchCharacter()
    {
        SetVelocity(Vector2.up * launchSpeed); // Example velocity, adjust as needed
        charAnim.TriggerJumpAnim();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayJump();
    }

    public void SetSpawnPoint(Vector3 newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
    }
    
    public void RespawnPlayer()
    {
        // Reset position to spawn point
        transform.position = spawnPoint;

        // Reset Camera Position
        CameraMovement.Instance.ResetCamera();

        // Reset velocity
        if (rigidBody2D != null)
        {
            rigidBody2D.linearVelocity = Vector2.zero;
            rigidBody2D.angularVelocity = 0f;
        }
        
        // Reset origin position for tilt mechanics
        originPos = new Vector2(spawnPoint.x, spawnPoint.y);
        
        // Reset animations
        if (charAnim != null)
        {
            charAnim.ResetAnimations();
            charAnim.SetIdleState();
        }
        
        Debug.Log("Player respawned at: " + spawnPoint);
    }
    
    public Vector3 GetSpawnPoint()
    {
        return spawnPoint;
    }

    public void UseAbility(SwipeDirection swipeDirection)
    {
        switch (currBud)
        {
            case characterBuds.Biggie:
                switch (swipeDirection)
                {
                    case SwipeDirection.Up:
                        Dash(Vector2.up);
                        break;
                    case SwipeDirection.Down:
                        Dash(Vector2.down);
                        break;
                    case SwipeDirection.Left:
                        Dash(Vector2.left);
                        break;
                    case SwipeDirection.Right:
                        break;
                    case SwipeDirection.UpRight:
                        Dash(new Vector2(1f, 1f));
                        break;
                    case SwipeDirection.UpLeft:
                        Dash(new Vector2(-1f, 1f));
                        break;
                    case SwipeDirection.DownRight:
                        Dash(new Vector2(1f, -1f));
                        break;
                    case SwipeDirection.DownLeft:
                        Dash(new Vector2(-1f, -1f));
                        break;
                }
                break;
            case characterBuds.Tappy:
                break;
            case characterBuds.Bam:
                break;
            case characterBuds.Ogu:
                break;
            default:
                break;
        }
    }

    private void Dash(Vector2 direction)
    {
        if (!canDash || isDashing || rigidBody2D == null) return;

        StartCoroutine(DashRoutine(direction));
    }

    private IEnumerator DashRoutine(Vector2 direction)
    {
        canDash = false;
        isDashing = true;
        currMovementState = movementState.Dashing;

        // Clear current velocity for consistency
        Vector2 cacheVelocity = rigidBody2D.linearVelocity;
        rigidBody2D.linearVelocity = Vector2.zero;

        // Apply dash force
        rigidBody2D.AddForce(direction.normalized * dashPower, ForceMode2D.Impulse);

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        currMovementState = movementState.Falling;
        //rigidBody2D.linearVelocity += cacheVelocity;

        // Begin cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void Stun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        ForceFall();
    }

    public void ForceFall()
    {
        if (rigidBody2D != null)
        {
            rigidBody2D.linearVelocity = new Vector2(0, stunFallVelocity);
        }
    }

    private void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
            }
            return;
        }

        if (isDashing) return;

        float currVelocityY = rigidBody2D.linearVelocityY;

        if (currVelocityY > 0.1f)
        {
            currMovementState = movementState.Jumping;
        }
        else if (currVelocityY < -0.1f)
        {
            currMovementState = movementState.Falling;
        }
        else
        {
            currMovementState = movementState.Neutral;
        }
        
    }

    private void FixedUpdate()
    {
        if (isStunned) return;
        if (touchManager != null && !isDashing)
        {
            Vector2 tiltVector;

            switch (touchManager.currentTilt.state)
            {
                case TiltingState.Left:
                    tiltVector = Vector2.left * touchManager.currentTilt.acceleration.magnitude * FORCEMAGNITUDE;
                    rigidBody2D.AddForce(tiltVector);
                    break;
                case TiltingState.Right:
                    tiltVector = Vector2.right * touchManager.currentTilt.acceleration.magnitude * FORCEMAGNITUDE;
                    rigidBody2D.AddForce(tiltVector);
                    break;
                case TiltingState.Stable:
                    float toOriginDistanceX = -rigidBody2D.position.x; // Horizontal Direction toward origin
                    float absDistance = Mathf.Abs(toOriginDistanceX - originPos.x);

                    if (absDistance > 0.1f) // Small threshold to avoid jitter at origin
                    {
                        float forceMagnitude = Mathf.Min(absDistance, 1f) * FORCEMAGNITUDE;
                        tiltVector = new Vector2(Mathf.Sign(toOriginDistanceX) * forceMagnitude, 0);
                        rigidBody2D.AddForce(tiltVector);
                        //gyroText.text = $"Returning to origin: {tiltVector}";
                    }
                    else
                    {
                        Vector2 tempVelocity = rigidBody2D.linearVelocity;
                        tempVelocity.x = 0f; // Stop horizontal movement
                        rigidBody2D.linearVelocity = tempVelocity;
                        this.transform.position = new Vector2(originPos.x, transform.position.y);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
