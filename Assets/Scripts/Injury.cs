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
        int roll = Random.Range(1,101);
        if(roll <= 65)
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
            numOfInjuries = 5;
            
        }
    }

    public void WoundRoll(Transform bodyPart)
    {
        WoundData[] woundsArray = null;
        switch (injuryType)
        {
            case InjuryType.Minor:
                woundsArray = minorWounds;
                break;
            case InjuryType.Severe:
                woundsArray = severeWounds;
                break;
            case InjuryType.Critical:
                woundsArray = criticalWounds;
                break;
        }

        if (woundsArray != null)
        {
            int woundRoll = Random.Range(0, woundsArray.Length);
            currentWounds.Add(woundsArray[woundRoll]);
            
            // Calculate wound position and rotation on the surface of the body part
            Vector3 woundPosition = GetSurfacePosition(bodyPart);
            Quaternion woundRotation = GetWoundRotation(bodyPart);
            
            // Instantiate wound with position on surface and rotation facing the surface
            Instantiate(woundsArray[woundRoll].woundPrefab, woundPosition, woundRotation);
        }
    }

    /// <summary>
    /// Calculates the position for a wound on a random point on the mesh surface of a body part.
    /// Samples a random triangle and places the wound at a random point on it.
    /// </summary>
    private Vector3 GetSurfacePosition(Transform bodyPart)
    {
        // Try to get the mesh from MeshFilter
        MeshFilter meshFilter = bodyPart.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter?.sharedMesh;
        
        // Fallback to MeshCollider if MeshFilter doesn't have a mesh
        if (mesh == null)
        {
            MeshCollider meshCollider = bodyPart.GetComponent<MeshCollider>();
            mesh = meshCollider?.sharedMesh;
        }
        
        if (mesh != null && mesh.triangles.Length > 0)
        {
            // Get a random triangle from the mesh
            int[] triangles = mesh.triangles;
            int randomTriangleIndex = Random.Range(0, triangles.Length / 3) * 3;
            
            // Get the three vertices of the random triangle
            Vector3 v0 = mesh.vertices[triangles[randomTriangleIndex]];
            Vector3 v1 = mesh.vertices[triangles[randomTriangleIndex + 1]];
            Vector3 v2 = mesh.vertices[triangles[randomTriangleIndex + 2]];
            
            // Use barycentric coordinates to get a random point on the triangle
            float r1 = Random.value;
            float r2 = Random.value;
            if (r1 + r2 > 1)
            {
                r1 = 1 - r1;
                r2 = 1 - r2;
            }
            
            Vector3 localPoint = v0 + r1 * (v1 - v0) + r2 * (v2 - v0);
            Vector3 worldPoint = bodyPart.TransformPoint(localPoint);
            
            // Calculate the triangle normal in local space
            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 localNormal = Vector3.Cross(edge1, edge2).normalized;
            
            // Convert normal to world space
            Vector3 worldNormal = bodyPart.TransformDirection(localNormal);
            
            // Place wound at mesh point with offset along the normal
            return worldPoint + worldNormal * woundSurfaceOffset;
        }
        
        // Fallback if no mesh: random sphere direction
        Vector3 randomDirection = Random.onUnitSphere;
        return bodyPart.position + randomDirection * woundSurfaceOffset;
    }

    /// <summary>
    /// Calculates the rotation for a wound to face the surface of the body part.
    /// The wound will face outward along the surface normal.
    /// </summary>
    private Quaternion GetWoundRotation(Transform bodyPart)
    {
        MeshFilter meshFilter = bodyPart.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter?.sharedMesh;
        
        if (mesh == null)
        {
            MeshCollider meshCollider = bodyPart.GetComponent<MeshCollider>();
            mesh = meshCollider?.sharedMesh;
        }
        
        if (mesh != null && mesh.triangles.Length > 0)
        {
            // Get a random triangle from the mesh
            int[] triangles = mesh.triangles;
            int randomTriangleIndex = Random.Range(0, triangles.Length / 3) * 3;
            
            // Get the three vertices of the random triangle
            Vector3 v0 = mesh.vertices[triangles[randomTriangleIndex]];
            Vector3 v1 = mesh.vertices[triangles[randomTriangleIndex + 1]];
            Vector3 v2 = mesh.vertices[triangles[randomTriangleIndex + 2]];
            
            // Calculate the triangle normal in local space
            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 localNormal = Vector3.Cross(edge1, edge2).normalized;
            
            // Convert normal to world space
            Vector3 worldNormal = bodyPart.TransformDirection(localNormal);
            
            // Create rotation that faces along the surface normal
            return Quaternion.LookRotation(worldNormal, bodyPart.up);
        }
        
        // Fallback: return body part's rotation
        return bodyPart.rotation;
    }

    /*private void OnDrawGizmosSelected()
    {
        // Draw gizmos for all child body parts to show where wounds would spawn
        foreach (Transform child in transform)
        {
            DrawWoundSpawnGizmo(child);
        }
    }
    private void DrawWoundSpawnGizmo(Transform bodyPart)
    {
        MeshFilter meshFilter = bodyPart.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter?.sharedMesh;
        
        if (mesh == null)
        {
            MeshCollider meshCollider = bodyPart.GetComponent<MeshCollider>();
            mesh = meshCollider?.sharedMesh;
        }
        
        if (mesh != null && mesh.triangles.Length > 0)
        {
            // Sample a few random positions on the mesh to show potential spawn points
            for (int i = 0; i < 3; i++)
            {
                int[] triangles = mesh.triangles;
                int randomTriangleIndex = Random.Range(0, triangles.Length / 3) * 3;
                
                Vector3 v0 = mesh.vertices[triangles[randomTriangleIndex]];
                Vector3 v1 = mesh.vertices[triangles[randomTriangleIndex + 1]];
                Vector3 v2 = mesh.vertices[triangles[randomTriangleIndex + 2]];
                
                float r1 = Random.value;
                float r2 = Random.value;
                if (r1 + r2 > 1)
                {
                    r1 = 1 - r1;
                    r2 = 1 - r2;
                }
                
                Vector3 localPoint = v0 + r1 * (v1 - v0) + r2 * (v2 - v0);
                Vector3 worldPoint = bodyPart.TransformPoint(localPoint);
                
                // Draw a sphere at potential wound spawn location
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(worldPoint, 0.02f);
            }
        }
    }*/
}
