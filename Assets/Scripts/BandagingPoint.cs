using UnityEngine;

public class BandagingPoint : MonoBehaviour
{
    private int pointIndex;
    private Wound wound;

    public void Initialize(int index, Wound parentWound)
    {
        pointIndex = index;
        wound = parentWound;
    }

    public void OnTouched() => wound.TouchPoint(pointIndex);
}

