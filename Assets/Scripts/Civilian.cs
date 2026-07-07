using System.Collections;
using UnityEngine;

public class Civilian : Injury
{
    public enum CivilianState
    {
        Calm,
        Panicked,
        Injured
    }
    public bool testSim;

    public CivilianState currentState;
    [Header("Civilian Attributes")]
    float speed = 2f;
    public float bloodLevel = 100f;
    public float bloodLossRate;
    public bool isInjured;

    public GameObject[] bodyParts;
    //public GameObject headPart;

    CivilianAI civilianAI;

    void Awake()
    {
        
    }

    void Start()
    {
        currentState = CivilianState.Calm;
        isInjured = false;
        civilianAI = GetComponent<CivilianAI>();
        //civilianAI.DisableRagdoll(bodyParts);
        if(testSim)
        {
            StartCoroutine(test());
        }
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
                    isInjured = true;
                    RollInjury();
                    WoundRoll();
                }

                bloodLevel -= bloodLossRate * Time.deltaTime;
                if(injuryCause == InjuryCause.Trip)
                {
                    civilianAI.EnableRagdoll(bodyParts);
                }

                if (bloodLevel <= 0)
                {
                    //Die();
                }
                break;
        }
    }

    private IEnumerator test()
    {
        yield return new WaitForSeconds(5f);
        currentState = CivilianState.Injured;
        StopCoroutine(test());
    }

    
}
