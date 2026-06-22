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

    public List<WoundData> currentWounds = new List<WoundData>();//current wounds list, empty when no more injuries
    
    [Header("Wound Placement Settings")]
    public float woundSurfaceOffset = 0.01f; //offset to place wound on the surface of the body part

    public void RollInjury()
    {
        int roll = Random.Range(1, 101);
        if (roll <= 65)
        {
            Debug.Log("Minor Injury");
            injuryType = InjuryType.Minor;
            numOfInjuries = 2;
        }
        else if (roll > 65 && roll <= 75)
        {
            Debug.Log("Severe Injury");
            injuryType = InjuryType.Severe;
            numOfInjuries = 3;
        }
        else
        {
            Debug.Log("Critical Injury");
            injuryType = InjuryType.Critical;
            numOfInjuries = 4;
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
            currentWounds.Add(woundsArray[woundRoll]);

            Vector3 woundPosition = GetSurfacePosition(bodyPart);
            Quaternion woundRotation = GetWoundRotation(bodyPart);

            Instantiate(woundsArray[woundRoll].woundPrefab, woundPosition, woundRotation, bodyPart);
        }
    }

    /// <summary>
    /// Calculates the position for a wound on a random point on the mesh surface of a body part.
    /// </summary>
    private Vector3 GetSurfacePosition(Transform bodyPart)
    {
        Mesh mesh = bodyPart.GetComponent<MeshFilter>()?.sharedMesh;

        // Fallback to MeshCollider if MeshFilter doesn't have a mesh
        if (mesh == null)
            mesh = bodyPart.GetComponent<MeshCollider>()?.sharedMesh;

        if (mesh != null && mesh.triangles.Length > 0)
        {
            int[] triangles = mesh.triangles;
            int randomTriangleIndex = Random.Range(0, triangles.Length / 3) * 3;

            Vector3 v0 = mesh.vertices[triangles[randomTriangleIndex]];
            Vector3 v1 = mesh.vertices[triangles[randomTriangleIndex + 1]];
            Vector3 v2 = mesh.vertices[triangles[randomTriangleIndex + 2]];

            // Use barycentric coordinates for random point on triangle
            float r1 = Random.value;
            float r2 = Random.value;
            if (r1 + r2 > 1)
            {
                r1 = 1 - r1;
                r2 = 1 - r2;
            }

            Vector3 localPoint = v0 + r1 * (v1 - v0) + r2 * (v2 - v0);
            Vector3 worldPoint = bodyPart.TransformPoint(localPoint);

            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 localNormal = Vector3.Cross(edge1, edge2).normalized;
            Vector3 worldNormal = bodyPart.TransformDirection(localNormal);

            return worldPoint + worldNormal * woundSurfaceOffset;
        }

        // Fallback: random sphere direction
        return bodyPart.position + Random.onUnitSphere * woundSurfaceOffset;
    }

    /// <summary>
    /// Calculates the rotation for a wound to face the surface of the body part.
    /// Both the up (Y) and forward (Z) directions point outward along the mesh surface normal.
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
        Mesh mesh = bodyPart.GetComponent<MeshFilter>()?.sharedMesh;

        // Fallback to MeshCollider if MeshFilter doesn't have a mesh
        if (mesh == null)
            mesh = bodyPart.GetComponent<MeshCollider>()?.sharedMesh;

        if (mesh != null && mesh.triangles.Length > 0)
        {
            int[] triangles = mesh.triangles;
            int randomTriangleIndex = Random.Range(0, triangles.Length / 3) * 3;

            Vector3 v0 = mesh.vertices[triangles[randomTriangleIndex]];
            Vector3 v1 = mesh.vertices[triangles[randomTriangleIndex + 1]];
            Vector3 v2 = mesh.vertices[triangles[randomTriangleIndex + 2]];

            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 localNormal = Vector3.Cross(edge1, edge2).normalized;

            return bodyPart.TransformDirection(localNormal);
        }

        // Fallback: use the body's up direction
        return bodyPart.up;
    }
}
