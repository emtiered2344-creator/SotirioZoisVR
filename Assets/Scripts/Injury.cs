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

    public void RollInjury()
    {
        int roll = Random.Range(1,101);
        if(roll <= 65)
        {
            Debug.Log("Minor Injury");
            injuryType = InjuryType.Minor;
            numOfInjuries = 1;
            
        }
        else if (roll > 65 && roll <= 75)
        {
            Debug.Log("Severe Injury");
            injuryType = InjuryType.Severe;
            numOfInjuries = 2;
            
        }
        else
        {
            Debug.Log("Critical Injury");
            injuryType = InjuryType.Critical;
            numOfInjuries = 3;
            
        }
    }

    public void WoundRoll()
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
            //instantiate wound prefabs
        }
    }
}
