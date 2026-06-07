using UnityEngine;
using UnityEngine.InputSystem;

public class AnimatorInput : MonoBehaviour
{
    public InputActionProperty triggerVal;
    public InputActionProperty gripVal;

    public Animator anim;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float trigger = triggerVal.action.ReadValue<float>();
        float grip = gripVal.action.ReadValue<float>();

        anim.SetFloat("Trigger",trigger);
        anim.SetFloat("Grip",grip);
    }
}
