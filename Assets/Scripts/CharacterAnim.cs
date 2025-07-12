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
        Debug.Log("Triggering Jump Animation");
        anim.SetBool("isFalling", true);
        anim.SetTrigger("startJump");
    }
    
    public void ResetAnimations()
    {
        if (anim != null)
        {
            // Reset all animator parameters to default values
            anim.SetBool("isFalling", false);
            anim.ResetTrigger("startJump");
            
            // Force animator to idle state
            anim.Play("BiggieIdle", 0, 0f);
            
            Debug.Log("Player animations reset to idle state");
        }
    }
    
    public void SetIdleState()
    {
        if (anim != null)
        {
            anim.SetBool("isFalling", false);
            anim.ResetTrigger("startJump");
        }
    }
    
    public void SetFallingState()
    {
        if (anim != null)
        {
            anim.SetBool("isFalling", true);
        }
    }

    public void TriggerSleepWakeAnim()
    {
        if (anim != null)
        {
            anim.SetTrigger("startGame");
        }
    }
}
