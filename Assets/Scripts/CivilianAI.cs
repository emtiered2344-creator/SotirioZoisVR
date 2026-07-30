using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CivilianAI : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    public void EnableRagdoll(GameObject[] bodyParts)
    {
        foreach (GameObject part in bodyParts)
        {
            Rigidbody rb = part.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            XRGrabInteractable grabInteractable = part.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.enabled = true;
            }
        }

        if (agent != null)
        {
            agent.enabled = false;
        }
        if (animator != null)
        {
            animator.enabled = false;
        }
    }
}
