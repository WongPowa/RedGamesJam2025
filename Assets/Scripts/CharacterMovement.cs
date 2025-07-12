using UnityEngine;
using TMPro;


public enum movementState
{
    Jumping,
    Falling,
    Neutral,
}

public class CharacterMovement : MonoBehaviour
{
    public static CharacterMovement Instance { get; private set; }

    const float FORCEMAGNITUDE = 5f;
    private Rigidbody2D rigidBody2D;
    [SerializeField] private Vector3 moveVelocity;
    public TextMeshProUGUI gyroText;
    public movementState currMovementState;
    
    private Vector2 originPos;
    private Vector3 spawnPoint;
    private TouchManager touchManager;
    private CharacterAnim charAnim;

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

    public void LaunchCharacter()
    {
        SetVelocity(Vector2.up * 10f); // Example velocity, adjust as needed
        charAnim.TriggerJumpAnim();
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
        
        Debug.Log("Player respawned at: " + spawnPoint);
    }
    
    public Vector3 GetSpawnPoint()
    {
        return spawnPoint;
    }

    private void Update()
    {
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
        if (touchManager != null)
        {
            Vector2 tiltVector;

            switch (touchManager.currentTilt.state)
            {
                case TiltingState.Left:
                    tiltVector = Vector2.left * touchManager.currentTilt.acceleration.magnitude * FORCEMAGNITUDE;
                    gyroText.text = $"tiltVecLeft: {tiltVector}";
                    rigidBody2D.AddForce(tiltVector);
                    break;
                case TiltingState.Right:
                    tiltVector = Vector2.right * touchManager.currentTilt.acceleration.magnitude * FORCEMAGNITUDE;
                    gyroText.text = $"tiltVecRight: {tiltVector}";
                    rigidBody2D.AddForce(tiltVector);
                    break;
                case TiltingState.Stable:
                    float toOriginDistanceX = -rigidBody2D.position.x; // Horizontal Direction toward origin
                    float absDistance = Mathf.Abs(toOriginDistanceX - originPos.x);

                    gyroText.text = $"toOriginDistanceX: {toOriginDistanceX} | absDistance: {absDistance}";

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
                        gyroText.text = $"At origin: No force";
                    }
                    break;
                default:
                    gyroText.text = "Unknown tilt state";
                    break;
            }
        }
        else
        {
            gyroText.text = "TouchManager not initialized";
        }
    }
}
