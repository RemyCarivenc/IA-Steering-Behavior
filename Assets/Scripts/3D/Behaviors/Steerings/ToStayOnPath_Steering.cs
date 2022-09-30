using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PathFollowing Behavior, force ObjectAI to stay on Path 
/// </summary>
public class ToStayOnPath_Steering : Steering
{
    [SerializeField]
    private bool drawGizmos = false;
    [SerializeField]
    private float predictionTime = 2;
    [SerializeField]
    private Transform pathRoot;

    private Vector3 onPath = Vector3.zero;

    private PathWay pathWay;

    private void OnEnable()
    {
        if (pathRoot != null)
        {
            List<Vector3> pathPoints = new List<Vector3>();

            for (int i = 0; i < pathRoot.childCount; i++)
                pathPoints.Add(pathRoot.GetChild(i).position);

            pathWay = new PathWay(pathPoints.ToArray(), 1, false);
        }
    }

    protected override Vector3 CalculateForce()
    {
        if (pathWay == null || pathWay.TotalPathLength < 2)
        {
            return Vector3.zero;
        }

        Vector3 futurePosition = ObjectAI.PredictFuturePosition(predictionTime);

        onPath = pathWay.MapPointToPath(futurePosition);

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

    void OnDrawGizmos()
    {
        if (drawGizmos && Application.isPlaying)
        {
            // draw line from our position to our predicted future position
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(ObjectAI.Position, ObjectAI.PredictFuturePosition(predictionTime));

            // draw line from our position to our steering target on the path
            Gizmos.color = new Color(1, 0.75f, 0);
            Gizmos.DrawLine(ObjectAI.Position, onPath);
        }
    }
}
