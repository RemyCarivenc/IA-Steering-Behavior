using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeadFollowing_Steering : Steering
{
    [SerializeField]
    private ObjectAI leader;
    [SerializeField]
    private float distance = 6;
    [SerializeField]
    private float distanceFlee = 3;
    [SerializeField]
    private float distanceArrive = 1;

    protected override Vector3 CalculateForce()
    {
        Vector3 steering = Vector3.zero;

        if (leader == null)
            return steering;

        Vector3 entityToLeader = leader.Position - ObjectAI.Position;
        Vector3 leadDirection = leader.transform.forward;

        if (Vector3.Dot(leadDirection, entityToLeader) < 0 && entityToLeader.magnitude < distance)
        {
            Vector3 frontOfLeader = leader.Position + leadDirection * distanceFlee;
            Vector3 desiredVelocity = ObjectAI.Position - frontOfLeader;
            steering = desiredVelocity - ObjectAI.Velocity;
        }
        else
        {
            Vector3 backOfLeader = leader.Position - leadDirection * distanceArrive;
            float velocityLength = ObjectAI.Velocity.magnitude;
            float slowingDistance = velocityLength / ObjectAI.MaxForce / ObjectAI.Mass;
            Vector3 targetOffset = backOfLeader - ObjectAI.Position;

            float dist = targetOffset.magnitude;
            float rampedSpeed = ObjectAI.MaxSpeed * dist / slowingDistance;
            float clippedSpeed = Mathf.Min(rampedSpeed, ObjectAI.MaxSpeed);

            Vector3 desiredVelocity = targetOffset * clippedSpeed / dist;
            steering = desiredVelocity - ObjectAI.Velocity;
        }

        return steering;
    }
}
