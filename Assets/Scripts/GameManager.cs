using UnityEngine;

public class GameManager : MonoBehaviour
{
    private CharacterMovement charMovement;
    public bool isGameStarted = false;
    public AirplaneAnim airplaneAnim;

    private void Start()
    {
        charMovement = CharacterMovement.Instance;
    }

    public void StartGame()
    {
        airplaneAnim.TriggerTakeOffAnim();
        charMovement.charAnim.TriggerSleepWakeAnim();
        isGameStarted = true;
        if (ObstacleSpawn.Instance != null)
            ObstacleSpawn.Instance.StartSpawning();
    }
}
