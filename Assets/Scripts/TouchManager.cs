using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;
using UnityEngine.InputSystem.XR.Haptics;
using JetBrains.Annotations;


public struct CurrentTiltStateData
{
    public TiltingState state;
    public Vector3 acceleration;
}
public enum TiltingState
{
    Right,
    Left,
    Stable
}

public enum SwipeDirection
{
    Up,
    Down,
    Left,
    Right,
    UpRight,
    UpLeft,
    DownRight,
    DownLeft
}

public class TouchManager : MonoBehaviour
{
    //public InputAction touchAction;
    public static TouchManager Instance { get; private set; }
    public CharacterMovement charMovement;
    private MobileInput mobileInput;
    public CurrentTiltStateData currentTilt;

    [Header("Detect Swipe Direction")]
    public UnityEvent<SwipeDirection> e_SwipeCalled;
    //public TextMeshProUGUI swipeDirectionText;
    [SerializeField] private SwipeDirection swipeDirection;
    [SerializeField] private Vector2 startTouchPos;
    [SerializeField] private Vector2 endTouchPos;

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

    private void Start()
    {
        //touchAction = InputSystem.actions.FindAction("TouchHold");
        //touchAction.TouchHold.Enable();

        mobileInput = new MobileInput();
        mobileInput.TouchScreen.Enable();
        mobileInput.TouchScreen.TouchPress.started += OnTouchStart;
        mobileInput.TouchScreen.TouchPress.canceled += OnTouchEnd;
        InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
        InputSystem.EnableDevice(UnityEngine.InputSystem.Accelerometer.current);
        //mobileInput.TouchScreen.Gyroscope.performed += UpdateGyroInput;
        mobileInput.TouchScreen.Accelerometer.performed += UpdateAccelInput;
    }

    private void OnDestroy()
    {
        mobileInput.TouchScreen.TouchPress.started -= OnTouchStart;
        mobileInput.TouchScreen.TouchPress.canceled -= OnTouchEnd;
        //mobileInput.TouchScreen.Gyroscope.performed -= UpdateGyroInput;
        mobileInput.TouchScreen.Accelerometer.performed -= UpdateAccelInput;
        mobileInput.Dispose();
    }

    private void OnTouchStart(InputAction.CallbackContext context)
    {
        startTouchPos = mobileInput.TouchScreen.TouchPosition.ReadValue<Vector2>();
    }

    private void OnTouchEnd(InputAction.CallbackContext context)
    {
        endTouchPos = mobileInput.TouchScreen.TouchPosition.ReadValue<Vector2>();
        DetectSwipeDirection();
    }

    private void DetectSwipeDirection()
    {
        Vector2 swipe = endTouchPos - startTouchPos;
        if (swipe.magnitude < 50f) // Minimum swipe distance to detect
            return;

        float angle = Mathf.Atan2(swipe.y, swipe.x) * Mathf.Rad2Deg;

        if (angle >= -22.5f && angle < 22.5f)
            swipeDirection = SwipeDirection.Right;
        else if (angle >= 22.5f && angle < 67.5f)
            swipeDirection = SwipeDirection.UpRight;
        else if (angle >= 67.5f && angle < 112.5f)
            swipeDirection = SwipeDirection.Up;
        else if (angle >= 112.5f && angle < 157.5f)
            swipeDirection = SwipeDirection.UpLeft;
        else if ((angle >= 157.5f && angle <= 180f) || (angle >= -180f && angle < -157.5f))
            swipeDirection = SwipeDirection.Left;
        else if (angle >= -157.5f && angle < -112.5f)
            swipeDirection = SwipeDirection.DownLeft;
        else if (angle >= -112.5f && angle < -67.5f)
            swipeDirection = SwipeDirection.Down;
        else if (angle >= -67.5f && angle < -22.5f)
            swipeDirection = SwipeDirection.DownRight;

        //swipeDirectionText.text = swipeDirection.ToString();
        e_SwipeCalled.Invoke(swipeDirection);
    }

    // Used for Debugging Purposes
    private void LaunchChar(InputAction.CallbackContext context)
    {
        charMovement.LaunchCharacter();
    }
    private void UpdateAccelInput(InputAction.CallbackContext context)
    {
        Vector3 accelInput = context.ReadValue<Vector3>();
        currentTilt.acceleration = accelInput;

        float tiltX = accelInput.x;

        if (tiltX > 0.1f)
        {
            currentTilt.state = TiltingState.Right;
        }
        else if (tiltX < -0.1f)
        {
            currentTilt.state = TiltingState.Left;
        }
        else
        {
            currentTilt.state = TiltingState.Stable;
        }
    }
}
