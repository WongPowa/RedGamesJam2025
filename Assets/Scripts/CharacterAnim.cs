using UnityEngine;

public class CharacterAnim : MonoBehaviour
{
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void TriggerJumpAnim()
    {
        anim.SetBool("isFalling", true);
        anim.SetTrigger("startJump");
    }

    public void TriggerSleepWakeAnim()
    {
        anim.SetTrigger("startGame");
    }
}
