using UnityEngine;

[CreateAssetMenu(fileName = "WoundData", menuName = "ScriptableObjects/WoundData", order = 1)]
public class WoundData : ScriptableObject
{
    public enum WoundType
    {
        Cut,
        Bruise,
        Burn,
        Fracture
    }
    public WoundType woundType;
    public enum WoundSeverity
    {
        Minor,
        Severe,
        Critical
    }
    public WoundSeverity woundSeverity;

    public GameObject woundPrefab;
}
