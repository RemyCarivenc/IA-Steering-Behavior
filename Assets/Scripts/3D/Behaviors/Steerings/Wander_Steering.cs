using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField]
    private Vector3 areaWander;

    private Vector3 wanderCirclePosition = Vector3.zero;
    private Vector3 randomPoint = Vector3.zero;
    private Vector3 desiredVelocity;

    private Vector3 startPos;
    private float minX, maxX, minY, maxY, minZ, maxZ;

    private float timer = .0f;

    public float WanderDistance
    {
        get { return wanderDistance; }
        set { wanderDistance = value; }
    }

    public Vector3 AreaWander
    {
        get { return AreaWander; }
        set
        {
            areaWander = value;
            SetMinMax();
        }
    }

    protected override void Start()
    {
        startPos = ObjectAI.Position;
        areaWander = new Vector3(Mathf.Abs(areaWander.x), Mathf.Abs(areaWander.y), Mathf.Abs(areaWander.z));
        SetMinMax();
    }

    protected override Vector3 CalculateForce()
    {
        wanderCirclePosition = ObjectAI.Position + ObjectAI.transform.forward * wanderDistance;

        desiredVelocity = (randomPoint - ObjectAI.Position).normalized * ObjectAI.MaxSpeed;

        UpdateTimer();

        if (areaWander != Vector3.zero)
        {
            if (ObjectAI.Position.x <= minX || ObjectAI.Position.x >= maxX)
                randomPoint = startPos;
            if (ObjectAI.Position.y <= minY || ObjectAI.Position.y >= maxY)
                randomPoint = startPos;
            if (ObjectAI.Position.z <= minZ || ObjectAI.Position.z >= maxZ)
                randomPoint = startPos;
        }

        return desiredVelocity;// - Vehicle.Velocity;
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

    private void SetMinMax()
    {
        minX = startPos.x - areaWander.x / 2;
        maxX = startPos.x + areaWander.x / 2;
        minY = startPos.y - areaWander.y / 2;
        maxY = startPos.y + areaWander.y / 2;
        minZ = startPos.z - areaWander.z / 2;
        maxZ = startPos.z + areaWander.z / 2;
    }

    void OnDrawGizmos()
    {
        if (drawGizmos && Application.isPlaying)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(ObjectAI.Position, transform.position + ObjectAI.Velocity.normalized * wanderDistance);
            Gizmos.DrawWireSphere(transform.position + ObjectAI.Velocity.normalized * wanderDistance, wanderRadius);
            Gizmos.DrawWireSphere(randomPoint, 0.33f);
            Gizmos.DrawLine(transform.position + ObjectAI.Velocity.normalized * wanderDistance, randomPoint);
        }

        if (drawGizmos)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            if (Application.isPlaying)
            {
                Gizmos.DrawCube(startPos, new Vector3(areaWander.x, areaWander.y, areaWander.z));
            }
            else
            {
                Gizmos.DrawCube(transform.position + GetComponent<ObjectAI>().Center, new Vector3(areaWander.x, areaWander.y, areaWander.z));
            }

        }
    }
}
