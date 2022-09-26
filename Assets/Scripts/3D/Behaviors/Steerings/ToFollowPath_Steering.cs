using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToFollowPath_Steering : Steering
{
    [SerializeField]
    private bool drawGizmos = false;
    [SerializeField]
    private float predictionTime = 2;
    [SerializeField]
    private Transform pathRoot;
    [SerializeField]
    private bool isCyclic;

    /// <summary>
    /// Minimum vehicle speed to consider when estimating future position.
    /// </summary>
    [SerializeField] private float minSpeedToConsider = 0.25f;

    private PathWay pathWay;

    private void OnEnable()
    {
        if (pathRoot != null)
        {
            List<Vector3> pathPoints = new List<Vector3>();

            for (int i = 0; i < pathRoot.childCount; i++)
                pathPoints.Add(pathRoot.GetChild(i).position);

            pathWay = new PathWay(pathPoints.ToArray(), 1, isCyclic);
        }
    }

    protected override Vector3 CalculateForce()
    {
        if (pathWay == null || pathWay.TotalPathLength < 2)
        {
            return Vector3.zero;
        }

        float speed = Mathf.Max(ObjectAI.Speed, minSpeedToConsider);

        float pathDistanceOffset = predictionTime * ObjectAI.Speed;

        float nowpathDistance = pathWay.MapPointToPathDistance(ObjectAI.Position);

        float targetPathDistance = nowpathDistance + pathDistanceOffset;
        Vector3 target = pathWay.MapPathDistanceToPoint(targetPathDistance);

        Vector3 seek = GetSeekVector(target);

        if(seek == Vector3.zero && targetPathDistance <= pathWay.TotalPathLength)
        {
            target = pathWay.MapPathDistanceToPoint(targetPathDistance + 2f * ObjectAI.ArrivalRadius);
            seek = GetSeekVector(target);
        }
        return seek;
    }

    private Vector3 GetSeekVector(Vector3 _target)
    {
        var force = Vector3.zero;

        var difference = _target - ObjectAI.Position;
        var d = difference.sqrMagnitude;
        if (d > ObjectAI.SquaredArrivalRadius)
        {
            force = difference;
        }
        return force;
    }

    private void OnDrawGizmos()
    {
        if (drawGizmos && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(ObjectAI.Position, ObjectAI.PredictFuturePosition(predictionTime));
        }
    }
}
