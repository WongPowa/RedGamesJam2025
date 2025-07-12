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
        charMovement.charAnim.TriggerSleepWakeAnim();
        isGameStarted = true;
    }
}
