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
        
    }
}
