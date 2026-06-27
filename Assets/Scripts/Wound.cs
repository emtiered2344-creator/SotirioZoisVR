using UnityEngine;

public class Wound : MonoBehaviour
{
    public enum WoundStage
    {
        Bleeding,
        NeedBandage,
        Healed
    }

    public WoundData woundData; // Reference to the wound data for this specific wound instance

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
    private int currentLoop = 1;
    private int woundSeverity = 1; // Number of loops required (set from woundData)
    private bool isFailed = false;
    private GameObject[] bandagingPoints;
    public bool[] pointsTouched;
    private float lastContactTime = 0f;
    private bool pointsGenerated = false;
    private Collider woundCollider;
    float bleedtimer;

    public WoundStage Stage => currentStage;
    public bool IsHealed => currentStage == WoundStage.Healed;
    public bool IsFailed => isFailed;
    public bool IsActive => WoundManager.Instance != null && WoundManager.Instance.ActiveWound == this;

    void Start()
    {
        woundCollider = GetComponent<Collider>();
        SetPointsVisibility(false);
        currentStage = WoundStage.Bleeding;
        
        // Set wound severity based on woundData
        if (woundData != null)
        {
            woundSeverity = woundData.woundSeverity switch
            {
                WoundData.WoundSeverity.Minor => 1,
                WoundData.WoundSeverity.Severe => 2,
                WoundData.WoundSeverity.Critical => 3,
                _ => 1
            };
        }
    }

    void Update()
    {
        if (IsHealed || isFailed || !IsActive) return;

        switch (currentStage)
        {
            case WoundStage.Bleeding:
                //gameobject particles turn on
                //currentStage = WoundStage.NeedBandage;
                break;
            
            case WoundStage.NeedBandage:
            //gameobject particles turn off
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

        Transform parent = transform.parent;
        if (parent == null)
        {
            Debug.LogWarning("Wound needs a parent body part!");
            return;
        }

        Mesh parentMesh = GetParentMesh(parent);
        if (parentMesh == null)
        {
            Debug.LogWarning("Parent object needs a MeshFilter or MeshCollider with a mesh!");
            return;
        }

        // Increase radius based on body part tag
        float effectiveRadius = radius;
        float prefabScale = 0.1f;

        if (parent.CompareTag("Head"))
        {
            effectiveRadius = radius * 4f;
            prefabScale = 0.25f;
        }
        else if (parent.CompareTag("Torso"))
        {
            effectiveRadius = radius * 6f;
            prefabScale = 0.3f;
        }
        else if (parent.CompareTag("Legs"))
        {
            effectiveRadius = radius * 3.5f;
            prefabScale = 0.25f;
        }
        else if (parent.CompareTag("Arms"))
        {
            effectiveRadius = radius * 2f;
            prefabScale = 0.15f;
        }

        Vector3 normal = CalculateSurfaceNormal(parent, parentMesh);

        if (Mathf.Abs(normal.y) < 0.3f)
        {
            float upDot = Mathf.Abs(Vector3.Dot(parent.up, Vector3.up));
            float rightDot = Mathf.Abs(Vector3.Dot(parent.right, Vector3.up));
            float forwardDot = Mathf.Abs(Vector3.Dot(parent.forward, Vector3.up));

            if (upDot >= rightDot && upDot >= forwardDot)
            {
                normal = parent.up;
            }
            else if (rightDot >= forwardDot)
            {
                normal = parent.right;
            }
            else
            {
                normal = parent.forward;
            }
        }

        Vector3 arbitrary = Vector3.right;
        if (Mathf.Abs(Vector3.Dot(normal, arbitrary)) > 0.99f)
            arbitrary = Vector3.forward;

        Vector3 tangent = Vector3.Cross(normal, arbitrary).normalized;
        Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

        for (int i = 0; i < pointCount; i++)
        {
            float angle = (360f / pointCount) * i * Mathf.Deg2Rad;
            Vector3 localTangent = transform.InverseTransformDirection(tangent);
            Vector3 localBitangent = transform.InverseTransformDirection(bitangent);
            Vector3 localPos = (Mathf.Cos(angle) * localTangent + Mathf.Sin(angle) * localBitangent) * effectiveRadius;
            Vector3 pointPosition = transform.TransformPoint(localPos);

            pointPosition = GetClosestPointOnMesh(pointPosition, parentMesh, parent);

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

    Mesh GetParentMesh(Transform parent)
    {
        if (parent == null) return null;

        MeshFilter meshFilter = parent.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
            return meshFilter.sharedMesh;

        MeshCollider meshCollider = parent.GetComponent<MeshCollider>();
        return meshCollider != null ? meshCollider.sharedMesh : null;
    }

    Vector3 CalculateSurfaceNormal(Transform parent, Mesh parentMesh)
    {
        if (parent == null) return transform.up;

        if (parentMesh != null && parentMesh.triangles.Length > 0)
        {
            int[] triangles = parentMesh.triangles;
            int triangleIndex = Random.Range(0, triangles.Length / 3) * 3;

            Vector3 v0 = parentMesh.vertices[triangles[triangleIndex]];
            Vector3 v1 = parentMesh.vertices[triangles[triangleIndex + 1]];
            Vector3 v2 = parentMesh.vertices[triangles[triangleIndex + 2]];

            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 localNormal = Vector3.Cross(edge1, edge2).normalized;
            return parent.TransformDirection(localNormal).normalized;
        }

        return parent.up;
    }

    Vector3 GetClosestPointOnMesh(Vector3 position, Mesh parentMesh, Transform parent)
    {
        if (parent == null) return position;

        MeshCollider meshCollider = parent.GetComponent<MeshCollider>();
        if (meshCollider != null)
            return meshCollider.ClosestPoint(position);

        return position;
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
            // Check if we've completed all required loops
            if (currentLoop >= woundSeverity)
            {
                OnHealed();
            }
            else
            {
                // Reset for next loop
                currentLoop++;
                pointsCompleted = 1;
                System.Array.Clear(pointsTouched, 0, pointsTouched.Length);
                SetPointsVisibility(true);
            }
        }
    }

    void OnHealed()
    {
        SetPointsVisibility(false);
        currentStage = WoundStage.Healed;
        if (WoundManager.Instance != null)
            WoundManager.Instance.WoundHealed(this);
        Destroy(gameObject); // TODO: change to decal bandaged
    }

    void OnFailed()
    {
        currentStage = WoundStage.Bleeding;
        pointsCompleted = 0;
        currentLoop = 1;
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
        
        if (transform.parent != null)
        {
            if (transform.parent.CompareTag("Head"))
            {
                displayRadius = radius;
                gizmoScale = 1f;
            }
            else if (transform.parent.CompareTag("Torso") || transform.parent.CompareTag("Legs"))
            {
                displayRadius = radius * 3.5f;
                gizmoScale = 2f;
            }
            else if (transform.parent.CompareTag("Arms"))
            {
                displayRadius = radius * 2.3f;
                gizmoScale = 1.5f;
            }
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
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Cloth") && currentStage == WoundStage.Bleeding)
        {
            if (other.bounds.Contains(woundCollider.bounds.max) && other.bounds.Contains(woundCollider.bounds.min))
            {
                bleedtimer += Time.deltaTime;
                if (bleedtimer >= 3f)
                {
                    currentStage = WoundStage.NeedBandage;
                    bleedtimer = 0;
                    Debug.Log("Wound stage changed to NeedBandage due to cloth contact.");
                }
                
            }
            else
            {
                bleedtimer = 0;
                Debug.Log("Cloth is not fully covering the wound.");
            }
        }
    }
}
