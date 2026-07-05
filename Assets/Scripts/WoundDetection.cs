using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class WoundDetection : MonoBehaviour
{
    public enum InputType
    {
        Select,
        Activate
    }

    [Header("Input Settings")]
    [Tooltip("Which button triggers wound selection.")]
    public InputType inputType = InputType.Select;

    [Header("Detection Settings")]
    [Tooltip("Radius for physics sphere detection around the controller.")]
    public float detectionRadius = 0.1f;

    [Tooltip("LayerMask for detecting wounds only.")]
    public LayerMask woundLayerMask;


    private XRBaseInputInteractor interactor;
    private Wound touchedWound = null;


    void Start()
    {
        interactor = GetComponent<XRBaseInputInteractor>();
        if (interactor == null)
            Debug.LogWarning("XRBaseInputInteractor not found on this GameObject.");
    }

    private void DetectNearbyWounds()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, detectionRadius, woundLayerMask);
        Wound nearestWound = null;
        float closestDistance = detectionRadius;

        foreach (Collider collider in nearbyColliders)
        {
            Wound wound = collider.GetComponent<Wound>();
            if (wound == null)
                wound = collider.GetComponentInParent<Wound>();

            if (wound != null && !wound.IsHealed)
            {
                float distance = Vector3.Distance(transform.position, wound.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestWound = wound;
                }
            }
        }
        touchedWound = nearestWound;
    }

    void Update()
    {
        if (interactor == null || WoundManager.Instance == null) return;

        DetectNearbyWounds();

        bool isPressed = inputType == InputType.Select 
            ? interactor.logicalSelectState.isPerformed 
            : interactor.logicalActivateState.isPerformed;

        if (isPressed && touchedWound != null && !touchedWound.IsHealed)
        {
            if (WoundManager.Instance.ActiveWound != touchedWound)
            {
                WoundManager.Instance.ActiveWound = touchedWound;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
