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
    public TextMeshProUGUI gyroText;
    public TextMeshProUGUI accelText;
    public TextMeshProUGUI turningText;
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
        mobileInput.TouchScreen.TouchHold.performed += LaunchChar;
        InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
        InputSystem.EnableDevice(UnityEngine.InputSystem.Accelerometer.current);
        //mobileInput.TouchScreen.Gyroscope.performed += UpdateGyroInput;
        mobileInput.TouchScreen.Accelerometer.performed += UpdateAccelInput;
    }

    private void OnDestroy()
    {
        mobileInput.TouchScreen.TouchHold.performed -= LaunchChar;
        //mobileInput.TouchScreen.Gyroscope.performed -= UpdateGyroInput;
        mobileInput.TouchScreen.Accelerometer.performed -= UpdateAccelInput;
        mobileInput.Dispose();
    }

    private void LaunchChar(InputAction.CallbackContext context)
    {
        charMovement.LaunchCharacter();
    }

    //private void Update()
    //{
    //    if (UnityEngine.InputSystem.Gyroscope.current != null)
    //    {
    //        Vector3 gyroInput = UnityEngine.InputSystem.Gyroscope.current.angularVelocity.ReadValue();
    //        gyroText.text = $"Gyro Input: {gyroInput}";
    //    }
    //    else
    //    {
    //        gyroText.text = "Gyroscope not available.";
    //    }
    //}

    //private void UpdateGyroInput(InputAction.CallbackContext context)
    //{
    //    Vector3 gyroInput = context.ReadValue<Vector3>();
    //    gyroText.text = $"Gyro Input: {gyroInput}";

    //    float tiltY = gyroInput.y;

    //    if (tiltY > 0.1f)
    //    {
    //        turningText.text = "Tilting Left";
    //    }
    //    else if (tiltY < -0.1f)
    //    {
    //        turningText.text = "Tilting Right";
    //    }
    //    else
    //    {
    //        turningText.text = "Stable";
    //    }
    //}

    private void UpdateAccelInput(InputAction.CallbackContext context)
    {
        Vector3 accelInput = context.ReadValue<Vector3>();
        currentTilt.acceleration = accelInput;

        if (accelText != null)
            accelText.text = $"Accel Input: {accelInput}";

        float tiltX = accelInput.x;

        if (turningText != null)
        {
            if (tiltX > 0.1f)
            {
                currentTilt.state = TiltingState.Right;
                turningText.text = "Tilting Right";
            }
            else if (tiltX < -0.1f)
            {
                currentTilt.state = TiltingState.Left;
                turningText.text = "Tilting Left";
            }
            else
            {
                currentTilt.state = TiltingState.Stable;
                turningText.text = "Stable";
            }
        }
    }


}
