using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Go to random position in the world
/// </summary>
public class Wander_Steering : Steering
{
    [SerializeField]
    private bool drawGizmos = false;
    [SerializeField]
    private float wanderRadius = 5;
    [SerializeField]
    private float wanderDistance = 8;
    [SerializeField]
    private float randomPointFrequency = 1;
    [SerializeField]
    private float distanceMinArrival = 1;

    private Vector3 wanderCirclePosition = Vector3.zero;
    private Vector3 randomPoint = Vector3.zero;
    private Vector3 desiredVelocity;

    private float timer = .0f;

    public float WanderDistance
    {
        get { return wanderDistance; }
        set { wanderDistance = value; }
    }

    protected override Vector3 CalculateForce()
    {
        wanderCirclePosition = ObjectAI.Position + ObjectAI.transform.forward * wanderDistance;

        desiredVelocity = (randomPoint - ObjectAI.Position).normalized * ObjectAI.MaxSpeed;

        UpdateTimer();

        return desiredVelocity;
    }

    private void UpdateTimer()
    {
        timer += ObjectAI.DeltaTime;
        float distance = Vector3.Distance(ObjectAI.Position, randomPoint);

        if (distance < distanceMinArrival || timer > randomPointFrequency)
        {
            randomPoint = Random.insideUnitSphere * wanderRadius;
            randomPoint += wanderCirclePosition;
            timer = 0;
        }
    }

    void OnDrawGizmos()
    {
        if (drawGizmos && Application.isPlaying)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(ObjectAI.Position, ObjectAI.Position + ObjectAI.Velocity.normalized * wanderDistance);
            Gizmos.DrawWireSphere(transform.position + ObjectAI.Velocity.normalized * wanderDistance, wanderRadius);
            Gizmos.DrawWireSphere(randomPoint, 0.33f);
            Gizmos.DrawLine(transform.position + ObjectAI.Velocity.normalized * wanderDistance, randomPoint);
            
        }
    }
}
