using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EntitiesAvoidance_Steering : Steering
{
    public bool drawGizmos = false;

    [SerializeField]
    private float minTimeToCollision = 1;

    private Vector3 ourPositionAtNearestApproach;
    private Vector3 threatPositionAtNearestApproach;


    protected override Vector3 CalculateForce()
    {
        Vector3 avoidance = Vector3.zero;
        if (ObjectAI.Radar.ObjectAIs == null || !ObjectAI.Radar.ObjectAIs.Any())
            return avoidance;

        // first priority is to prevent immediate interpenetration
        Vector3 separation = SteerToAvoidCloseNeighbors(0, ObjectAI.Radar.ObjectAIs);
        if (separation != Vector3.zero)
            return separation;

        // otherwise, go on to consider potential future collisions
        float steer = 0;
        ObjectAI threat = null;
        float minTime = minTimeToCollision;

        foreach (var other in ObjectAI.Radar.ObjectAIs)
        {
            float collisionDangerThreshold = ObjectAI.Radius * 2;
            float time = PredictNearestApproachTime(other);

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

        /*
                Vector3 futurPosition = ObjectAI.Position + ObjectAI.Velocity;

                foreach(var otherObjectAI in ObjectAI.Radar.ObjectAIs)
                {
                    Vector3 futurePositionOther = otherObjectAI.Position + otherObjectAI.Velocity;
                    Vector3 diff = futurPosition - futurePositionOther;
                    text.text = diff.magnitude.ToString();
                    if(diff.magnitude < ObjectAI.Radius)
                        avoidance += diff;
                }*/
        /* Vector3 avoidance = Vector3.zero;

         if (ObjectAI.Radar.ObjectAIs == null || !ObjectAI.Radar.ObjectAIs.Any())
             return avoidance;

         float shortestTime = float.PositiveInfinity;

         ObjectAI firstTarget = null;
         float firstMinSeparation = 0, firstDistance = 0, firstRadius = 0;
         Vector3 firstRelativePos = Vector3.zero, firstRelativeVel = Vector3.zero;

         foreach (var otherObjectAI in ObjectAI.Radar.ObjectAIs)
         {
             Vector3 relativePos = ObjectAI.Position - otherObjectAI.Position;
             Vector3 relativeVel = ObjectAI.Velocity - otherObjectAI.Velocity;
             float distance = relativePos.magnitude;
             float relativeSpeed = relativeVel.magnitude;

             if (relativeSpeed == 0)
                 continue;

             float timeToCollision = -1 * Vector3.Dot(relativePos, relativeVel) / (relativeSpeed * relativeSpeed);


             Vector3 separation = relativePos + relativeVel * timeToCollision;
             float minSeparation = separation.magnitude;

             if (minSeparation > ObjectAI.Radius + otherObjectAI.Radius + ObjectAI.Radius)
             {
                 continue;
             }


             if (timeToCollision > 0 && timeToCollision < shortestTime)
             {
                 shortestTime = timeToCollision;
                 firstTarget = otherObjectAI;
                 firstMinSeparation = minSeparation;
                 firstDistance = distance;
                 firstRelativePos = relativePos;
                 firstRelativeVel = relativeVel;
                 firstRadius = otherObjectAI.Radius;
             }
         }

         if (firstTarget == null)
         {
             return avoidance;
         }

         if (firstMinSeparation <= 0 || firstDistance < ObjectAI.Radius + firstRadius + ObjectAI.Radius)
         {
             avoidance = ObjectAI.Position - firstTarget.Position;
         }

         else
         {
             avoidance = firstRelativePos + firstRelativeVel * shortestTime;
         }

        // avoidance /= ObjectAI.Radar.ObjectAIs.Count;

         Vector3 desiredVelocity = Vector3.Reflect(ObjectAI.DesiredVelocity, avoidance);
 */
    }
    
    private Vector3 SteerToAvoidCloseNeighbors(float _mindSeparationDistance, List<ObjectAI> _otherObjects)
    {
        foreach (var other in _otherObjects)
        {
            float sumOfRadius = ObjectAI.Radius + other.Radius;
            float minCenterToCenter = _mindSeparationDistance + sumOfRadius;
            Vector3 offset = other.Position - ObjectAI.Position;
            float currenDistance = offset.magnitude;
            if (currenDistance < minCenterToCenter)
            {
                float projection = Vector3.Dot(offset, ObjectAI.transform.forward);
                Vector3 perpendicular = ObjectAI.transform.forward * projection;
                perpendicular = -offset - perpendicular;

                return perpendicular;
            }
        }

        return Vector3.zero;

    }

    public float PredictNearestApproachTime(ObjectAI _other)
    {
        Vector3 tempVelocity = _other.Velocity - ObjectAI.Velocity;
        float tempSpeed = tempVelocity.magnitude;

        if (Mathf.Approximately(tempSpeed, 0))
        {
            return 0;
        }

        Vector3 tempTangent = tempVelocity / tempSpeed;

        Vector3 tempPosition = ObjectAI.Position - _other.Position;
        float projection = Vector3.Dot(tempTangent, tempPosition);

        return projection / tempSpeed;
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
        Gizmos.DrawLine(ObjectAI.Position, ObjectAI.Position + ObjectAI.Velocity * minTimeToCollision);
    }
}

