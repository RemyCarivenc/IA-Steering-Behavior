using System;
using UnityEngine;

public class AutonomousVehicle : TickedObjectAI
{
    #region Internal state values

    private float speed;

    #endregion
    [Header("AutonomousVehicle")]
    /// <summary>
    /// Acceleration rate - it'll be used as a multiplier for the speed
    /// at which the velocity is interpolated when accelerating. A rate
    /// of 1 means that we interpolate across 1 second; a rate of 5 means
    /// we do it five times as fast 
    /// </summary>
    [SerializeField]
    private float accelerationRate = 5;

    /// <summary>
    /// Deceleration rate - it'll be used as a multiplier for the speed
    /// at which the velocity is interpolated when decelerating. A rate
    /// of 1 means that we interpolate across 1 second; a rate of 5 means
    /// we do it five times as fast.
    /// </summary>
    [SerializeField]
    private float decelerationRate = 8;

    /// <summary>
    /// Current vehicle speed
    /// </summary>
    public override float Speed
    {
        get { return speed; }
    }

    /// <summary>
    /// Current vehicle velocity
    /// </summary>
    public override Vector3 Velocity
    {
        get { return transform.forward * Speed; }
        protected set { throw new NotSupportedException("Cannot set the velocity directly on AutonomousVehicle"); }
    }

    #region Speed-related methods

    /// <summary>
    /// Uses a desired velocity vector to adjust the vehicle's target speed and 
    /// orientation velocity.
    /// </summary>
    /// <param name="velocity">Newly calculated velocity</param>
    protected override void SetCalculatedVelocity(Vector3 _velocity)
    {
        TargetSpeed = _velocity.magnitude;
        OrientationVelocity = Mathf.Approximately(speed, 0) ? transform.forward : _velocity / TargetSpeed;
    }

    /// <summary>
    /// Calculates how much the agent's position should change in a manner that
    /// is specific to the vehicle's implementation.
    /// </summary>
    /// <param name="deltaTime">Time delta to use in position calculations</param>
    protected override Vector3 CalculatePositionDelta(float _deltaTime)
    {
        /*
     * Notice that we clamp the target speed and not the speed itself, 
     * because the vehicle's maximum speed might just have been lowered
     * and we don't want its actual speed to suddenly drop.
     */
        var targetSpeed = Mathf.Clamp(TargetSpeed, 0, MaxSpeed);
        if (Mathf.Approximately(speed, targetSpeed))
        {
            speed = targetSpeed;
        }
        else
        {
            var rate = TargetSpeed > speed ? accelerationRate : decelerationRate;
            speed = Mathf.Lerp(speed, targetSpeed, _deltaTime * rate);
        }
        return Velocity * _deltaTime;
    }

    /// <summary>
    /// Zeros this vehicle's target speed, which results on its desired velocity
    /// being zero.
    /// </summary>
    protected override void ZeroVelocity()
    {
        TargetSpeed = 0;
    }

    #endregion

}
