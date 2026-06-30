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
    private int currentLoop = 1;
    public int woundSeverityInt = 1; // Number of loops required (set from woundData)

    public enum WoundSeverity
    {
        Minor = 1,
        Severe = 2,
        Critical = 3
    }
    public WoundSeverity severityLevel = WoundSeverity.Minor;

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
        GenerateBandagingPoints();
        SetPointsVisibility(false);
        
        pointsGenerated = true;
        
        woundCollider = GetComponent<Collider>();
        currentStage = WoundStage.Bleeding;

        woundSeverityInt = severityLevel switch
        {
            WoundSeverity.Minor => 1,
            WoundSeverity.Severe => 2,
            WoundSeverity.Critical => 3,
            _ => 1
        };

        gameObject.SetActive(false); // Deactivate the wound until it is needed
    }

    void Update()
    {
        if (IsHealed || isFailed || !IsActive) return;

        switch (currentStage)
        {
            case WoundStage.Bleeding:
                //gameobject particles turn on
                currentStage = WoundStage.NeedBandage;
                break;
            
            case WoundStage.NeedBandage:
            //gameobject particles turn off

                lastContactTime = Time.time;
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

    public void GenerateBandagingPoints()
    {
        bandagingPoints = new GameObject[pointCount];
        pointsTouched = new bool[pointCount];

        Transform parent = transform.parent;
        if (parent == null)
        {
            Debug.LogWarning("Wound needs a parent body part!" + gameObject.name);
            return;
        }

        Collider parentCollider = GetParentCollider(parent);
        if (parentCollider == null)
        {
            Debug.LogWarning("Parent object needs a primitive collider for wound placement!");
            return;
        }

        // Increase radius based on body part tag
        float effectiveRadius = radius;
        float prefabScale = 0.07f;

        if (parent.CompareTag("Head"))
        {
            effectiveRadius = radius * 5.5f;
            //prefabScale = 0.1f;
        }
        else if (parent.CompareTag("Torso"))
        {
            effectiveRadius = radius * 6f;
            //prefabScale = 0.1f;
        }
        else if (parent.CompareTag("Legs"))
        {
            effectiveRadius = radius * 3.5f;
            //prefabScale = 0.1f;
        }
        else if (parent.CompareTag("Arms"))
        {
            effectiveRadius = radius * 4f;
            //prefabScale = 0.15f;
        }

        for (int i = 0; i < pointCount; i++)
        {
            float angle = (360f / pointCount) * i * Mathf.Deg2Rad;
            Vector3 localPos = new Vector3(
                0f,
                Mathf.Sin(angle) * effectiveRadius,
                Mathf.Cos(angle) * effectiveRadius);
            Vector3 pointPosition = transform.TransformPoint(localPos);

            pointPosition = GetClosestPointOnCollider(pointPosition, parentCollider, parent);

            GameObject point = Instantiate(pointPrefab, pointPosition, Quaternion.identity, transform);
            point.name = $"BandagePoint_{i}";
            point.tag = "BandagePoint";
            point.transform.localScale = Vector3.one * prefabScale;
            point.transform.localRotation = Quaternion.identity;

            Collider col = point.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            BandagingPoint bp = point.AddComponent<BandagingPoint>();
            bp.Initialize(i, this);
            bandagingPoints[i] = point;
        }
    }

    Collider GetParentCollider(Transform parent)
    {
        if (parent == null) return null;
        return parent.GetComponent<Collider>();
    }

    Vector3 GetClosestPointOnCollider(Vector3 position, Collider parentCollider, Transform parent)
    {
        if (parent == null || parentCollider == null) return position;
        return parentCollider.ClosestPoint(position);
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
            if (currentLoop >= woundSeverityInt)
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
        Injury injury = transform.root.GetComponent<Injury>();
        if (injury != null)
        {
            injury.currentWounds.Remove(gameObject);
        }
        else
        {
            Debug.LogWarning("Injury component not found on root object.");
        }
        
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
        float gizmoScale = radius;
        
        if (transform.parent != null)
        {
            if (transform.parent.CompareTag("Head"))
            {
                displayRadius = radius*5.5f;
                //gizmoScale = 1f;
            }
            else if (transform.parent.CompareTag("Torso"))
            {
                displayRadius = radius * 6f;
                //gizmoScale = 2f;
            }

            else if (transform.parent.CompareTag("Legs"))
            {
                displayRadius = radius * 3.5f;
                //gizmoScale = 1.5f;
            }
            else if (transform.parent.CompareTag("Arms"))
            {
                displayRadius = radius * 4f;
                //gizmoScale = 1.5f;
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
