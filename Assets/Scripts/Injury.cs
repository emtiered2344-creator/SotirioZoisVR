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

    public enum InjuryCause
    {
        Trip,
        Cut,
        OverExertion
    }
    
    public InjuryType injuryType;
    public InjuryCause injuryCause;

    public int numOfInjuries;

    public List<GameObject> TripMinorWounds = new List<GameObject>();
    public List<GameObject> TripSevereWounds = new List<GameObject>();
    public List<GameObject> CuttingMinorWounds = new List<GameObject>();
    public List<GameObject> CuttingSevereWounds = new List<GameObject>();

    public List<GameObject> OverExertionWounds = new List<GameObject>();

    public List<GameObject> currentWounds = new List<GameObject>();//current wounds list, empty when no more injuries
    
    [Header("Wound Placement Settings")]
    public float woundSurfaceOffset = 0.01f; //offset to place wound on the surface of the body part

    public void RollInjury()
    {
       int roll = Random.Range(0, 2);
            switch (roll)
            {
                case 0:
                    injuryType = InjuryType.Minor;  
                    numOfInjuries = 2;
                    break;
                case 1:
                    injuryType = InjuryType.Severe;
                    numOfInjuries = 3;
                    break;
            }
    }

    public void WoundRoll()
    {
        if(injuryCause == InjuryCause.Cut || injuryCause == InjuryCause.OverExertion) numOfInjuries = 1; // Cuts and overexertion only have one wound
        for(int i = 0; i<= numOfInjuries; i++)
        {
            if(injuryCause == InjuryCause.Trip)
            {
                int woundIndex;
                GameObject wound;
                
                Debug.Log("Injury Cause: " + injuryCause);

                if(injuryType == InjuryType.Minor) 
                {
                    woundIndex = Random.Range(0, TripMinorWounds.Count);
                    wound = TripMinorWounds[woundIndex];
                    
                    
                }
                else
                {
                    woundIndex = Random.Range(0, TripSevereWounds.Count);
                    wound = TripSevereWounds[woundIndex];
                    Debug.Log("Wound: " + wound.name);
                }

                currentWounds.Add(wound);
                wound.SetActive(true);
                Debug.Log("Wound: " + wound.name);

            }
            else if(injuryCause == InjuryCause.Cut)
            {
                int woundIndex;
                GameObject wound;
                
                Debug.Log("Injury Cause: " + injuryCause);

                if(injuryType == InjuryType.Minor)
                {
                    woundIndex = Random.Range(0, CuttingMinorWounds.Count);
                    wound = CuttingMinorWounds[woundIndex];
                }
                else
                {
                    woundIndex = Random.Range(0, CuttingSevereWounds.Count);
                    wound = CuttingSevereWounds[woundIndex];
                }

                currentWounds.Add(wound);
                wound.SetActive(true);
                Debug.Log("Wound: " + wound.name);
                
            }
            else if(injuryCause == InjuryCause.OverExertion)
            {
                int woundIndex = Random.Range(0, OverExertionWounds.Count);
                GameObject wound = OverExertionWounds[woundIndex];
                currentWounds.Add(wound);
                wound.SetActive(true);
                Debug.Log("Wound: " + wound.name);
            }
        }
    }
}
