using UnityEngine;

public class Flee_Steering : Steering
{
    [SerializeField]
    private Transform targetPoint = null;

    public Transform TargetPoint
    {
        get { return targetPoint; }
        set { targetPoint = value; }
    }

    protected override Vector3 CalculateForce()
    {
        if (targetPoint != null)
        {
            Vector3 desiredVelocity = ObjectAI.Position - targetPoint.position;

            return desiredVelocity - ObjectAI.Velocity;
        }
        else
        {
            return Vector3.zero;
        }
    }
}

