using UnityEngine;

public class BandageRoll : MonoBehaviour
{
    [Header("Visual Effects")]
    [Tooltip("LineRenderer to show the trail of touched points.")]
    public LineRenderer trailRenderer;

    void Awake()
    {
        if (trailRenderer == null)
        {
            trailRenderer = GetComponent<LineRenderer>();
            if (trailRenderer == null)
                trailRenderer = gameObject.AddComponent<LineRenderer>();
        }
        trailRenderer.startWidth = 0.01f;
        trailRenderer.endWidth = 0.01f;
        trailRenderer.positionCount = 0;
    }

    void OnTriggerEnter(Collider other)
    {
        if (WoundManager.Instance == null || WoundManager.Instance.ActiveWound == null) return;
        if (!other.CompareTag("BandagePoint")) return;

        Wound wound = other.GetComponentInParent<Wound>();
        if (wound != WoundManager.Instance.ActiveWound) return;

        BandagingPoint bp = other.GetComponent<BandagingPoint>();
        if (bp != null)
            bp.OnTouched();

        trailRenderer.positionCount++;
        trailRenderer.SetPosition(trailRenderer.positionCount - 1, other.transform.position);
        other.gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector3 origin = transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + transform.forward * 0.2f);
    }
}
