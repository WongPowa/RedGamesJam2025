using UnityEngine;

public class AirplaneAnim : MonoBehaviour
{
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void TriggerTakeOffAnim()
    {
        anim.SetTrigger("takeOff");
    }
}
