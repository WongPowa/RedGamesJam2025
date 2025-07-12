using UnityEngine;
using UnityEngine.InputSystem;
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

public class TouchManager : MonoBehaviour
{
    //public InputAction touchAction;
    public static TouchManager Instance { get; private set; }
    public CharacterMovement charMovement;
    private MobileInput mobileInput;
    public CurrentTiltStateData currentTilt;

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
        //mobileInput.TouchScreen.TouchHold.performed += LaunchChar;
        InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
        InputSystem.EnableDevice(UnityEngine.InputSystem.Accelerometer.current);
        //mobileInput.TouchScreen.Gyroscope.performed += UpdateGyroInput;
        mobileInput.TouchScreen.Accelerometer.performed += UpdateAccelInput;
    }

    private void OnDestroy()
    {
        //mobileInput.TouchScreen.TouchHold.performed -= LaunchChar;
        //mobileInput.TouchScreen.Gyroscope.performed -= UpdateGyroInput;
        mobileInput.TouchScreen.Accelerometer.performed -= UpdateAccelInput;
        mobileInput.Dispose();
    }

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
