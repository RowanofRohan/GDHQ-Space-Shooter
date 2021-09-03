using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionController : MonoBehaviour
{
    [SerializeField]
    private int[] movementIDList;
    [SerializeField]
    private float[] moveDurationList;
    [SerializeField]
    private Vector3[] destinationList;
    [SerializeField]
    private float[] accelerationList;
    
    public int GetMovementID(int i)
    {
        return movementIDList[i];
    }

    public float GetMoveduration(int i)
    {
        return moveDurationList[i];
    }

    public Vector3 GetDestination(int i)
    {
        return destinationList[i];
    }

    public float GetAcceleration(int i)
    {
        return accelerationList[i];
    }

    public int GetLength()
    {
        return moveDurationList.Length;
    }
}
