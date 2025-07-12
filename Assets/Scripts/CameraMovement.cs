using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public GameObject player;

    private void Update()
    {
        if (player != null)
        {
            // Set the camera position to be above the player
            transform.position = new Vector3(transform.position.x, player.transform.position.y, -10f);
            // Make the camera look at the player
            //transform.LookAt(player.transform);
        }
    }
}
