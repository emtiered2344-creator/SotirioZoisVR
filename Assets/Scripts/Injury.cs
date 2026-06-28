using UnityEngine;
using System.Collections.Generic;

public class Injury : MonoBehaviour
{
    public enum InjuryType
    {
        Minor,
        Severe,
        Critical
    }
    public InjuryType injuryType;
    public int numOfInjuries;
    public WoundData[] minorWounds;//data array of possible wounds to be applied to the civilian
    public WoundData[] severeWounds;//data array of possible wounds to be applied to the civilian
    public WoundData[] criticalWounds;//data array of possible wounds to be applied to the civilian

    public List<GameObject> currentWounds = new List<GameObject>();//current wounds list, empty when no more injuries
    
    [Header("Wound Placement Settings")]
    public float woundSurfaceOffset = 0.01f; //offset to place wound on the surface of the body part

    public void RollInjury()
    {
        int roll = Random.Range(1, 101);
        if (roll <= 65)
        {
            Debug.Log("Minor Injury");
            injuryType = InjuryType.Minor;
            numOfInjuries = 15;
        }
        else if (roll > 65 && roll <= 75)
        {
            Debug.Log("Severe Injury");
            injuryType = InjuryType.Severe;
            numOfInjuries = 15;
        }
        else
        {
            Debug.Log("Critical Injury");
            injuryType = InjuryType.Critical;
            numOfInjuries = 15;
        }
    }

    public void WoundRoll(Transform bodyPart)
    {
        WoundData[] woundsArray = injuryType switch
        {
            InjuryType.Minor => minorWounds,
            InjuryType.Severe => severeWounds,
            InjuryType.Critical => criticalWounds,
            _ => null
        };

        if (woundsArray != null)
        {
            int woundRoll = Random.Range(0, woundsArray.Length);

            Vector3 woundPosition = GetSurfacePosition(bodyPart);
            Quaternion woundRotation = GetWoundRotation(bodyPart);

            GameObject wound = Instantiate(woundsArray[woundRoll].woundPrefab, woundPosition, woundRotation, bodyPart);
            currentWounds.Add(wound);
            wound.GetComponent<Wound>().GenerateBandagingPoints();
        }
    }

    /// <summary>
    /// Calculates the position for a wound on a random point on the surface of a primitive collider.
    /// </summary>
    private Vector3 GetSurfacePosition(Transform bodyPart)
    {
        if (TryGetSurfacePoint(bodyPart, out Vector3 point, out Vector3 normal))
            return point + normal * woundSurfaceOffset;

        return bodyPart.position + Random.onUnitSphere * woundSurfaceOffset;
    }

    /// <summary>
    /// Calculates the rotation for a wound to face the surface of the body part.
    /// Both the up (Y) and forward (Z) directions point outward along the collider surface normal.
    /// </summary>
    private Quaternion GetWoundRotation(Transform bodyPart)
    {
        Vector3 surfaceNormal = CalculateSurfaceNormal(bodyPart);
        Vector3 forward = Vector3.Cross(surfaceNormal, Vector3.up).normalized;
        if (forward == Vector3.zero)
            forward = Vector3.Cross(surfaceNormal, Vector3.right).normalized;

        return Quaternion.LookRotation(forward, surfaceNormal);
    }

    private Vector3 CalculateSurfaceNormal(Transform bodyPart)
    {
        if (TryGetSurfacePoint(bodyPart, out _, out Vector3 normal))
            return normal;

        return bodyPart.up;
    }

    private bool TryGetSurfacePoint(Transform bodyPart, out Vector3 point, out Vector3 normal)
    {
        point = bodyPart.position;
        normal = bodyPart.up;

        Collider collider = bodyPart.GetComponent<Collider>();
        if (collider == null)
            return false;

        switch (collider)
        {
            case BoxCollider boxCollider:
                return GetBoxSurfacePoint(boxCollider, bodyPart, out point, out normal);
            case SphereCollider sphereCollider:
                return GetSphereSurfacePoint(sphereCollider, bodyPart, out point, out normal);
            case CapsuleCollider capsuleCollider:
                return GetCapsuleSurfacePoint(capsuleCollider, bodyPart, out point, out normal);
            default:
                return GetGenericColliderSurfacePoint(collider, bodyPart, out point, out normal);
        }
    }

    private bool GetBoxSurfacePoint(BoxCollider boxCollider, Transform bodyPart, out Vector3 point, out Vector3 normal)
    {
        Vector3 center = boxCollider.center;
        Vector3 halfExtents = boxCollider.size * 0.5f;

        int face = Random.Range(0, 6);
        Vector3 localPoint = center;
        Vector3 localNormal = Vector3.up;

        switch (face)
        {
            case 0:
                localPoint = new Vector3(center.x + halfExtents.x, center.y + Random.Range(-halfExtents.y, halfExtents.y), center.z + Random.Range(-halfExtents.z, halfExtents.z));
                localNormal = Vector3.right;
                break;
            case 1:
                localPoint = new Vector3(center.x - halfExtents.x, center.y + Random.Range(-halfExtents.y, halfExtents.y), center.z + Random.Range(-halfExtents.z, halfExtents.z));
                localNormal = Vector3.left;
                break;
            case 2:
                localPoint = new Vector3(center.x + Random.Range(-halfExtents.x, halfExtents.x), center.y + halfExtents.y, center.z + Random.Range(-halfExtents.z, halfExtents.z));
                localNormal = Vector3.up;
                break;
            case 3:
                localPoint = new Vector3(center.x + Random.Range(-halfExtents.x, halfExtents.x), center.y - halfExtents.y, center.z + Random.Range(-halfExtents.z, halfExtents.z));
                localNormal = Vector3.down;
                break;
            case 4:
                localPoint = new Vector3(center.x + Random.Range(-halfExtents.x, halfExtents.x), center.y + Random.Range(-halfExtents.y, halfExtents.y), center.z + halfExtents.z);
                localNormal = Vector3.forward;
                break;
            default:
                localPoint = new Vector3(center.x + Random.Range(-halfExtents.x, halfExtents.x), center.y + Random.Range(-halfExtents.y, halfExtents.y), center.z - halfExtents.z);
                localNormal = Vector3.back;
                break;
        }

        point = bodyPart.TransformPoint(localPoint);
        normal = bodyPart.TransformDirection(localNormal).normalized;
        return true;
    }

    private bool GetSphereSurfacePoint(SphereCollider sphereCollider, Transform bodyPart, out Vector3 point, out Vector3 normal)
    {
        Vector3 localNormal = Random.onUnitSphere.normalized;
        Vector3 localPoint = sphereCollider.center + localNormal * sphereCollider.radius;

        point = bodyPart.TransformPoint(localPoint);
        normal = bodyPart.TransformDirection(localNormal).normalized;
        return true;
    }

    private bool GetCapsuleSurfacePoint(CapsuleCollider capsuleCollider, Transform bodyPart, out Vector3 point, out Vector3 normal)
    {
        Vector3 center = capsuleCollider.center;
        float radius = capsuleCollider.radius;
        float height = Mathf.Max(capsuleCollider.height, radius * 2f);
        float halfHeight = height * 0.5f;
        float cylinderHalfHeight = halfHeight - radius;

        Vector3 axis = capsuleCollider.direction switch
        {
            0 => Vector3.right,
            2 => Vector3.forward,
            _ => Vector3.up
        };

        Vector3 tangentA = Vector3.Cross(axis, Vector3.forward).normalized;
        if (tangentA == Vector3.zero)
            tangentA = Vector3.Cross(axis, Vector3.right).normalized;
        Vector3 tangentB = Vector3.Cross(axis, tangentA).normalized;

        Vector3 localPoint;
        Vector3 localNormal;

        float axialOffset = Random.Range(-cylinderHalfHeight, cylinderHalfHeight);
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 radial = Mathf.Cos(angle) * tangentA + Mathf.Sin(angle) * tangentB;
        localPoint = center + axis * axialOffset + radial * radius;
        localNormal = radial.normalized;

        point = bodyPart.TransformPoint(localPoint);
        normal = bodyPart.TransformDirection(localNormal).normalized;
        return true;
    }

    private bool GetGenericColliderSurfacePoint(Collider collider, Transform bodyPart, out Vector3 point, out Vector3 normal)
    {
        Bounds bounds = collider.bounds;
        Vector3 samplePoint = bounds.center + Random.onUnitSphere.normalized * Mathf.Max(bounds.extents.magnitude, 0.001f);
        point = collider.ClosestPoint(samplePoint);

        Vector3 direction = (point - bounds.center).normalized;
        normal = direction == Vector3.zero ? bodyPart.up : direction;
        return true;
    }
}
