using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Unaligned collision avoidance behavior
/// </summary>
public class EntitiesAvoidance_Steering : Steering
{
    public bool drawGizmos = false;

    [SerializeField]
    private float predictionTime = 1;

    private Vector3 ourPositionAtNearestApproach;
    private Vector3 threatPositionAtNearestApproach;

    Vector3 avoidance;

    protected override Vector3 CalculateForce()
    {
        avoidance = Vector3.zero;
        if (ObjectAI.Radar.ObjectAIs == null || !ObjectAI.Radar.ObjectAIs.Any())
            return avoidance;

        // first priority is to prevent immediate interpenetration
        Vector3 separation = ObjectAI.SteerToAvoidCloseNeighbors(0);
        if (separation != Vector3.zero)
            return separation;

        // otherwise, go on to consider potential future collisions
        float steer = 0;
        ObjectAI threat = null;
        float minTime = predictionTime;

        foreach (var other in ObjectAI.Radar.ObjectAIs)
        {
            float collisionDangerThreshold = ObjectAI.Radius * 2;
            float time = ObjectAI.PredictNearestApproachTime(other);

            if ((time >= 0) && (time < minTime))
            {
                if (ComputeNearestApproachPositions(other, time) < collisionDangerThreshold)
                {
                    minTime = time;
                    threat = other;
                }
            }
        }

        // if a potential collision was found, compute steering to avoid
        if (threat != null)
        {
            // parallel: +1, perpendicular: 0, anti-parallel: -1
            float parallelness = Vector3.Dot(ObjectAI.transform.forward, threat.transform.forward);
            float angle = 0.707f;

            if (parallelness < -angle)
            {
                Vector3 offset = threatPositionAtNearestApproach - ObjectAI.Position;
                float sideDot = Vector3.Dot(offset, Vector3.left);
                steer = (sideDot > 0) ? -1.0f : 1.0f;
            }
            else
            {
                if (parallelness > angle)
                {
                    Vector3 offset = threat.Position - ObjectAI.Position;
                    float sideDot = Vector3.Dot(offset, Vector3.left);
                    steer = (sideDot > 0) ? -1.0f : 1.0f;
                }
                else
                {
                    if (threat.Speed <= ObjectAI.Speed)
                    {
                        float sideDot = Vector3.Dot(Vector3.left, threat.Velocity);
                        steer = (sideDot > 0) ? -1.0f : 1.0f;
                    }
                }
            }
        }

        avoidance = Vector3.left * steer;
        return avoidance;
    }

    private float ComputeNearestApproachPositions(ObjectAI _other, float _time)
    {
        Vector3 myTravel = ObjectAI.transform.forward * ObjectAI.Speed * _time;
        Vector3 otherTravel = _other.transform.forward * _other.Speed * _time;

        Vector3 myFinal = ObjectAI.Position + myTravel;
        Vector3 otherFinal = _other.Position + otherTravel;

        ourPositionAtNearestApproach = myFinal;
        threatPositionAtNearestApproach = otherFinal;

        return Vector3.Distance(myFinal, otherFinal);
    }

    private void OnDrawGizmos()
    {
        if (ObjectAI == null || !drawGizmos) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(ObjectAI.Position, ObjectAI.Position + avoidance);
    }
}

