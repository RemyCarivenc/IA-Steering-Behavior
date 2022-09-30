using UnityEngine;

/// <summary>
/// Behavior that will aim to achieve a constant speed along the vehicle's forward vector
/// </summary>
public class GoUpToSpeed_Steering : Steering
{
    [SerializeField]
    private float targetSpeed = 5;

    public float TargetSpeed
    {
        get { return targetSpeed; }
        set { targetSpeed = value; }
    }

    protected override Vector3 CalculateForce()
    {
        return ObjectAI.GetTargetSpeedVector(targetSpeed);
    }
}
