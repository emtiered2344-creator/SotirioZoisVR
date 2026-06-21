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

    public GameObject bodyParts;
    public GameObject headPart;

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
                        int bodyRoll = Random.Range(0, 101);
                        if(bodyRoll <=80){
                            //int bodyPartRoll = Random.Range(0, bodyParts.Length);
                            //WoundRoll(bodyParts[bodyPartRoll].transform); //roll for injuries and wounds
                            WoundRoll(bodyParts.transform); //roll for injuries and wounds, instantiate on body parts
                        }
                        else
                        {
                            WoundRoll(headPart.transform); //roll for injuries and wounds, instantiate on head part
                        }
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
