using UnityEngine;

public class Civilian : Injury
{
    public enum CivilianState
    {
        Calm,
        Panicked,
        Injured
    }
    public CivilianState currentState;
    [Header("Civilian Attributes")]
    float speed = 2f;
    public float bloodLevel = 100f;
    public float bloodLossRate;
    public bool isInjured;

    void Start()
    {
        currentState = CivilianState.Calm;
        isInjured = false;
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case CivilianState.Calm:
                //MovePath();
                break;
            case CivilianState.Panicked:
                //MoveRandom();
                break;
            case CivilianState.Injured:
                if (!isInjured)
                {
                    RollInjury();
                    isInjured = true;
                    for(int i = 0; i < numOfInjuries; i++)
                    {
                        //roll for body part.
                        WoundRoll();//roll for injuries and wounds, instantiate on body parts
                    }
                    
                }

                bloodLevel -= bloodLossRate * Time.deltaTime;
                if (bloodLevel <= 0)
                {
                    //Die();
                }
                break;
        }
    }
}
