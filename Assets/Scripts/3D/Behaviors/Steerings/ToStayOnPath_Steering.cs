using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathFollowingController))]
public class ToStayOnPath_Steering : Steering
{
    [SerializeField]
    private float estimationTime = 2;

    private PathWay pathWay;

    public PathWay PathWay
    {
        set {pathWay = value;}
    }

    protected override Vector3 CalculateForce()
    {
        if (pathWay == null || pathWay.TotalPathLength < 2)
        {
            return Vector3.zero;
        }
   
        Vector3 futurePosition = ObjectAI.PredictFuturePosition(estimationTime);

        Vector3 onPath = pathWay.MapPointToPath(futurePosition);

        if (pathWay.Outside < 0)
        {
            return Vector3.zero;
        }
        else
        {
            Vector3 desiredVelocity = onPath - ObjectAI.Position;
            return desiredVelocity - ObjectAI.Velocity;
        }
    }
}
