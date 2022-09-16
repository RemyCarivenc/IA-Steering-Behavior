using System.Linq;
using UnityEngine;

/// <summary>
/// Base class for objectAI.false It does not move the objects, and instead
/// provides a set of basic functionality for its subclasses.
/// </summary>
public abstract class ObjectAI : Entity
{
    #region Private fields

    [Header("ObjectAI")]
    [SerializeField]
    private float minSpeedForTurning = 0.1f;

    /*/// <summary>
    /// The objectAI movement priority
    /// </summary>
    /// <remarks>
    /// Used only by some behaviors to determine if a objectAI should
    /// be given priority before another one.!-- You may disregard if you aren't
    /// using any behavior like that.
    /// </remarks>
    [SerializeField]
    private int movementPriority;*/

    /// <summary>
    /// Across how many seconds is the objectAI's forward orientation smoothed
    /// </summary>
    [SerializeField]
    private float turnTime = 0.25f;

    /// <summary>
    /// ObjectAI's mass
    /// </summary>
    /// <remarks>
    /// The total force from the steering behaviors will be divided by the
    /// ObjectAI mass before applying
    /// </remarks>
    [SerializeField]
    private float mass = 1;

    /// <summary>
    /// Indicates which axes a objectAI is allowed to move on
    /// </summary>
    /// <remarks>
    /// A 0 on the X/Y/Z value means the objectAI is not allowed to move on that 
    /// axis, a 1 indicates it can. We use Vector3Toggle to set it on the 
    /// editor as a helper
    /// </remarks>
    [SerializeField, Vector3Toggle]
    private Vector3 allowedMovementAxes = Vector3.one;

    /// <summary>
    /// The objectAI's arrival radius
    /// </summary>
    /// <remarks>
    /// The difference between the radius and arrival raidus is that
    /// the first is used to determine the area the objectAI covers, whereas the
    /// second one is a value used to determine if a objectAI is close enough
    /// to a desired target. Unlike the radius, it is not scaled with the objectAI
    /// </remarks>
    [SerializeField]
    private float arrivalRadius = 0.25f;

    [SerializeField]
    private float maxSpeed = 1;

    [SerializeField]
    private float maxForce = 10;

    /// <summary>
    /// Indicates if the behavior should move or not
    /// </summary>
    [SerializeField]
    private bool canMove = true;

    #endregion

    #region Public properties

    public Vector3 AllowedMovementAxes
    {
        get { return allowedMovementAxes; }
    }

    public bool CanMove
    {
        get { return canMove; }
        set { canMove = value; }
    }

    /// <summary>
    /// The velocity desired by this objectAI, likely calculated by means
    /// similar to what AutonomousVehicle
    /// </summary>
    public Vector3 DesiredVelocity { get; protected set; }

    public float Mass
    {
        get { return mass; }
        set { mass = Mathf.Max(0, value); }
    }

    public float MaxForce
    {
        get { return maxForce; }
        set { maxForce = Mathf.Clamp(value, 0, float.MaxValue); }
    }

    public float MaxSpeed
    {
        get { return maxSpeed; }
        set { maxSpeed = Mathf.Clamp(value, 0, float.MaxValue); }
    }

    public float MinSpeedForTurning
    {
        get { return minSpeedForTurning; }
    }

    /* public int MovementPriority
     {
         get { return movementPriority; }
     }*/

    public Radar Radar { get; private set; }

    public Rigidbody Rigidbody { get; private set; }

    // public Speedometer Speedometer { get; protected set; }

    public float ArrivalRadius
    {
        get { return arrivalRadius; }
        set
        {
            arrivalRadius = Mathf.Clamp(value, 0.01f, float.MaxValue);
            SquaredArrivalRadius = arrivalRadius * arrivalRadius;
        }
    }

    /// <summary>
    /// Squared arrival radius, for performance purposes
    /// </summary>
    public float SquaredArrivalRadius { get; private set; }

    /// <summary>
    /// Last raw force applied to the objectAI. It is expected to be set 
    /// by the subclases.
    /// </summary>
    public Vector3 LastRawForce { get; protected set; }

    public abstract float Speed { get; }

    public float TurnTime
    {
        get { return turnTime; }
        set { turnTime = Mathf.Max(0, value); }
    }

    public Steering[] Steerings { get; private set; }
    public Steering[] SteeringPostprocessors { get; private set; }

    public abstract Vector3 Velocity { get; protected set; }

    /// <summary>
    /// Current magnitude for the objectAI's velocity.
    /// </summary>
    /// <remarks>
    /// It is expected to be set at the same time that the Velocity is 
    /// assigned in one of the descendent classes.  It may or may not
    /// match the objectAI speed, depending on how that is calculated - 
    /// for example, some subclasses can use a Speedometer to calculate
    /// their speed.
    /// </remarks>
    public float TargetSpeed { get; protected set; }

    /// <summary>
    /// The delta time used by this objectAI.
    /// </summary>
    /// <value>The delta time.</value>
    /// <remarks>
    /// ObjectAI aren't necessarily ticked every frame, so we keep a
    /// DeltaTime property that steering behaviors can access when
    /// their CalculateForce is called.
    /// </remarks>
    public virtual float DeltaTime
    {
        get { return Time.deltaTime; }
    }

    #endregion

    #region Unity methods

    protected override void Awake()
    {
        base.Awake();
        Rigidbody = GetComponent<Rigidbody>();
        var allSteering = GetComponents<Steering>();
        Steerings = allSteering.Where(x => !x.IsPostProcess).ToArray();
        SteeringPostprocessors = allSteering.Where(x => x.IsPostProcess).ToArray();

        /* if(movementPriority == 0)
             movementPriority = gameObject.GetInstanceID();*/

        Radar = GetComponent<Radar>();
        // Speedometer = GetComponent<Speedometer>();
        SquaredArrivalRadius = ArrivalRadius * ArrivalRadius;
    }

    #endregion

    #region Methods
    /// <summary>
    /// Predicts where the objectAI will be at a point in the future
    /// </summary>
    /// <param name="_predictionTime">
    /// A time in seconds for the prediction
    /// </param>
    /// <returns>
    /// ObjectAI position
    /// </returns>
    public Vector3 PredictFuturePosition(float _predictionTime)
    {
        return Position + (Velocity * _predictionTime);
    }

    /// <summary>
    /// Predicts where the objectAI wants to be at a point in the future
    /// </summary>
    /// <param name="_predictionTime">
    /// A time in seconds for the prediction
    /// </param>
    /// <returns>
    /// ObjectAI position
    /// </returns>
    public Vector3 PredictFutureDesiredPosition(float _predictionTime)
    {
        return Position + (DesiredVelocity * _predictionTime);
    }

    public Vector3 SteerToAvoidCloseNeighbors(float _mindSeparationDistance, ObjectAI _object)
    {
        foreach (var other in _object.Radar.Obstacles)
        {
            float sumOfRadius = _object.Radius + other.Radius;
            float minCenterToCenter = _mindSeparationDistance + sumOfRadius;
            Vector3 offset = other.Position - _object.Position;
            float currenDistance = offset.magnitude;
            if (currenDistance < minCenterToCenter)
            {
                float projection = Vector3.Dot(offset, _object.transform.forward);
                Vector3 perpendicular = _object.transform.forward * projection;
                perpendicular = -offset - perpendicular;

                return perpendicular;
            }
        }

        return Vector3.zero;
    }

    public float PredictNearestApproachTime(ObjectAI _other)
    {
        Vector3 tempVelocity = _other.Velocity - Velocity;
        float tempSpeed = tempVelocity.magnitude;

        if (Mathf.Approximately(tempSpeed, 0))
        {
            return 0;
        }

        Vector3 tempTangent = tempVelocity / tempSpeed;

        Vector3 tempPosition = Position - _other.Position;
        float projection = Vector3.Dot(tempTangent, tempPosition);

        return projection / tempSpeed;
    }

    public bool IsInNeighborhood(ObjectAI _other, float _minDistance, float _maxDistance, float _cosMaxAngle)
    {
        Vector3 offset = _other.Position - Position;
        float distanceSquared = offset.sqrMagnitude;

        // definitely in neighborhood if inside minDistance sphere
        if (distanceSquared < (_minDistance * _minDistance))
        {
            return true;
        }
        else
        {
            // definitely not in neighborhood if outside maxDistance sphere
            if (distanceSquared > (_maxDistance * _maxDistance))
            {
                return false;
            }
            else
            {
                // otherwise, test angular offset from forward axis
                Vector3 unitOffset = offset / Mathf.Sqrt(distanceSquared);
                float forwardness = Vector3.Dot(transform.forward, unitOffset);
                if(forwardness > _cosMaxAngle)
                    return true;
                else
                    return false;
            }
        }
    }

    /// <summary>
    /// Returns a maxForce-clipped steering force along the
    /// forward vector that can be used to try to maintain a target speed
    /// </summary>
    /// <param name="_targetSpeed">
    /// Target speed to aim for
    /// </param>
    /// <returns>
    /// The target speed vector
    /// </returns>
    public Vector3 GetTargetSpeedVector(float _targetSpeed)
    {
        float speedError = _targetSpeed - Speed;
        return transform.forward * Mathf.Clamp(speedError, -MaxForce, +MaxForce);
    }

    /// <summary>
    /// Returns the distance from this objectAI to another
    /// </summary>
    /// <param name="_other">
    /// ObjectAI to compare against
    /// </param>
    /// <returns>
    /// The distance between both objectAI' positions. If negative, they are overlapping
    /// </returns>
    public float DistanceFromPerimeter(ObjectAI _other)
    {
        Vector3 diff = Position - _other.Position;
        return diff.magnitude - Radius - _other.Radius;
    }

    /// <summary>
    /// Reset the objectAI's orientation
    /// </summary>
    public void ResetOrientation()
    {
        transform.up = Vector3.up;
        transform.forward = Vector3.forward;
    }
    #endregion

    protected override void OnDrawGizmos()
    {

        if (drawGizmos)
        {
            base.OnDrawGizmos();
            //DesiredVelocity
            Debug.DrawLine(Position, DesiredVelocity + Position, Color.green);
        }
    }
}
