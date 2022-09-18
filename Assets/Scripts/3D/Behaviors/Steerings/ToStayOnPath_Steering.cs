using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToStayOnPath_Steering : Steering
{
    [SerializeField]
    private float estimationTime = 2;

    protected override Vector3 CalculateForce()
    {
        Vector3 futurePosition = ObjectAI.PredictFuturePosition(estimationTime);

       /* Vector3 tangent;
        float outside;
        
        Vector3 onPath = 

        if(outside < 0)
            return Vector3.zero;
        else
        {
            return Vector3.zero;
        }*/
        return Vector3.zero;
    }
}
