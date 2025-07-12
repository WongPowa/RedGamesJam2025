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
}
