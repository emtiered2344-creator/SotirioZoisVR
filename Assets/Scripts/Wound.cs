using UnityEngine;

public class Wound : MonoBehaviour
{
    public enum WoundStage
    {
        Bleeding,
        NeedBandage,
        Healed
    }

    [Header("Wound Properties")]
    public float radius = 0.05f;
    
    [Header("Bandaging Points")]
    public GameObject pointPrefab;
    public int pointCount = 8;

    [Header("Contact Timeout")]
    [Tooltip("How long (in seconds) the player can stop touching before the wound fails.")]
    public float contactTimeout = 3f;

    public WoundStage currentStage = WoundStage.Bleeding;
    private int pointsCompleted = 0;
    private bool isFailed = false;
    private GameObject[] bandagingPoints;
    private bool[] pointsTouched;
    private float lastContactTime = 0f;
    private bool pointsGenerated = false;

    public WoundStage Stage => currentStage;
    public bool IsHealed => currentStage == WoundStage.Healed;
    public bool IsFailed => isFailed;
    public bool IsActive => WoundManager.Instance != null && WoundManager.Instance.ActiveWound == this;

    void Start()
    {
        SetPointsVisibility(false);
        currentStage = WoundStage.Bleeding;
    }

    void Update()
    {
        if (IsHealed || isFailed || !IsActive) return;

        switch (currentStage)
        {
            case WoundStage.Bleeding:
                currentStage = WoundStage.NeedBandage;
                break;
            
            case WoundStage.NeedBandage:
                if (!pointsGenerated)
                {
                    GenerateBandagingPoints();
                    SetPointsVisibility(true);
                    pointsGenerated = true;
                    lastContactTime = Time.time;
                }
                
                if (Time.time - lastContactTime > contactTimeout)
                {
                    isFailed = true;
                    OnFailed();
                }
                break;
            
            case WoundStage.Healed:
                SetPointsVisibility(false);
                break;
        }
    }

    void GenerateBandagingPoints()
    {
        bandagingPoints = new GameObject[pointCount];
        pointsTouched = new bool[pointCount];

        MeshCollider parentMesh = transform.parent?.GetComponent<MeshCollider>();
        if (parentMesh == null)
        {
            Debug.LogWarning("Parent object needs a MeshCollider!");
            return;
        }

        // Increase radius if not a head
        float effectiveRadius = radius;
        float prefabScale = 0.1f;

        if (transform.parent != null && !transform.parent.name.ToLower().Contains("head"))
        {
            effectiveRadius = radius * 2.3f;
            prefabScale = 0.15f; // Scale the prefab size for non-head parts
        }

        // Get surface normal via raycast for robust positioning
        Vector3 normal = Vector3.up;
        Ray ray = new Ray(transform.position + transform.up * 0.01f, transform.up);

        RaycastHit hit;
        if (parentMesh.Raycast(ray, out hit, 1f))
            normal = hit.normal;
        else
            Debug.LogWarning("Could not determine surface normal, using transform.up");

        // Check if normal is near horizontal (vertical component is small)
        if (Mathf.Abs(normal.y) < 0.3f && transform.parent != null)
        {
            // Find the most vertical axis from parent transform
            float upDot = Mathf.Abs(Vector3.Dot(transform.parent.up, Vector3.up));
            float rightDot = Mathf.Abs(Vector3.Dot(transform.parent.right, Vector3.up));
            float forwardDot = Mathf.Abs(Vector3.Dot(transform.parent.forward, Vector3.up));

            if (upDot >= rightDot && upDot >= forwardDot)
            {
                // Use parent's up as the circle orientation
                normal = transform.parent.up;
            }
            else if (rightDot >= forwardDot)
            {
                // Use parent's right as the circle orientation
                normal = transform.parent.right;
            }
            else
            {
                // Use parent's forward as the circle orientation
                normal = transform.parent.forward;
            }
        }

        // Build orthonormal basis for the circle
        Vector3 arbitrary = Vector3.right;
        if (Mathf.Abs(Vector3.Dot(normal, arbitrary)) > 0.99f)
            arbitrary = Vector3.forward;

        Vector3 right = Vector3.Cross(normal, arbitrary).normalized;

        // Generate points around the circle (perpendicular to surface plane)
        for (int i = 0; i < pointCount; i++)
        {
            float angle = (360f / pointCount) * i * Mathf.Deg2Rad;
            // Calculate position in world space using right and normal basis vectors
            Vector3 localPos = (Mathf.Cos(angle) * right + Mathf.Sin(angle) * normal) * effectiveRadius;
            Vector3 pointPosition = transform.TransformPoint(localPos);

            // Project onto mesh surface to handle curvature
            pointPosition = GetClosestPointOnMesh(pointPosition, parentMesh);

            GameObject point = Instantiate(pointPrefab, pointPosition, Quaternion.identity, transform);
            point.name = $"BandagePoint_{i}";
            point.tag = "BandagePoint";
            point.transform.localScale = Vector3.one * prefabScale;

            Collider col = point.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            BandagingPoint bp = point.AddComponent<BandagingPoint>();
            bp.Initialize(i, this);
            bandagingPoints[i] = point;
        }
    }

    Vector3 GetClosestPointOnMesh(Vector3 position, MeshCollider meshCollider)
    {
        return meshCollider.ClosestPoint(position);
    }

    public void SetPointsVisibility(bool visible)
    {
        if (bandagingPoints == null) return;
        foreach (GameObject point in bandagingPoints)
            if (point != null)
                point.SetActive(visible);
    }

    public void TouchPoint(int pointIndex)
    {
        if (IsHealed || isFailed || !IsActive) return;
        if (pointsTouched[pointIndex]) return;

        lastContactTime = Time.time;
        pointsTouched[pointIndex] = true;
        pointsCompleted++;

        if (pointsCompleted >= pointCount)
        {
            OnHealed();
        }
    }

    void OnHealed()
    {
        SetPointsVisibility(false);
        currentStage = WoundStage.Healed;
        if (WoundManager.Instance != null)
            WoundManager.Instance.WoundHealed(this);
        Destroy(gameObject, 1f); // TODO: change to decal bandaged
    }

    void OnFailed()
    {
        //currentStage = WoundStage.Bleeding;
        pointsCompleted = 0;
        System.Array.Clear(pointsTouched, 0, pointsTouched.Length);
        isFailed = false;
        lastContactTime = Time.time;
        pointsGenerated = false;
        
        if (bandagingPoints != null)
        {
            foreach (GameObject point in bandagingPoints)
                if (point != null)
                    Destroy(point);
            bandagingPoints = null;
        }
        
        if (WoundManager.Instance != null)
            WoundManager.Instance.WoundFailed(this);
    }

    void OnDrawGizmosSelected()
    {
        float displayRadius = radius;
        float gizmoScale = 1f;
        if (transform.parent != null && !transform.parent.name.ToLower().Contains("head"))
        {
            displayRadius = radius * 2f;
            gizmoScale = 1.5f;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, displayRadius);

        // Draw gizmos for the circle spawn points
        if (pointsGenerated && bandagingPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (GameObject point in bandagingPoints)
            {
                if (point != null)
                    Gizmos.DrawSphere(point.transform.position, 0.02f * gizmoScale);
            }
        }
    }
}
