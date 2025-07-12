using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public static CameraMovement Instance { get; private set; }

    public GameObject player;
    public Camera cam;
    private CharacterMovement charMovement;
    public Vector3 originCamPos;

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
        charMovement = CharacterMovement.Instance;
        cam = Camera.main;
        originCamPos = cam.transform.position;
    }

    private void Update()
    {
        if (player != null)
        {
            Debug.Log("current movement state " + charMovement.currMovementState);

            switch (charMovement.currMovementState)
            {
                case movementState.Jumping:
                    // Camera follows the player in jumping state
                    float cameraPosY = transform.position.y;
                    float playerPosY = player.transform.position.y;
                    if (playerPosY > cameraPosY)
                        transform.position = new Vector3(transform.position.x, playerPosY, -10f);
                    break;
                case movementState.Falling:
                    // Camera tilts left when the player is moving left
                    //transform.position = new Vector3(player.transform.position.x - 1f, player.transform.position.y, -10f);
                    break;
                default:
                    break;
            }
        }
    }

    public void ResetCamera()
    {
        if (cam != null)
        {
            cam.transform.position = originCamPos;
        }
    }
}
