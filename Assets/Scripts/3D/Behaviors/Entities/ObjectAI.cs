using System.Linq;
using UnityEngine;

/// <summary>
/// Base class for objectAI. It does not move the objects, and instead
/// provides a set of basic functionality for its subclasses.
/// </summary>
public abstract class ObjectAI : Entity
{
    [Header("ObjectAI")]

    /// <summary>
    /// Minimum speed necessary for ths vehicle to apply a turn
    /// </summary>
    [SerializeField]
    private float minSpeedForTurning = 0.1f;

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
    /// axis, a 1 indicates it can
    /// </remarks>
    [SerializeField, Vector3Toggle]
    private Vector3 allowedMovementAxes = Vector3.one;

    /// <summary>
    /// The vehicle's maximum speed
    /// </summary>
    [SerializeField]
    private float maxSpeed = 1;

    /// <summary>
    /// Maximum force that can be applied to the vehicle.
    /// </summary>
    [SerializeField]
    private float maxForce = 10;

    /// <summary>
    /// Indicates if the behavior should move or not
    /// </summary>
    [SerializeField]
    private bool canMove = true;

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

    /// <summary>
    /// Radar assigned to this vehicle
    /// </summary>
    public Radar Radar { get; private set; }

    public Rigidbody Rigidbody { get; private set; }

    /// <summary>
    /// Current vehicle speed
    /// </summary>
    public abstract float Speed { get; }

    public float TurnTime
    {
        get { return turnTime; }
        set { turnTime = Mathf.Max(0, value); }
    }

    /// <summary>
    /// Array of steering behaviors
    /// </summary>
    public Steering[] Steerings { get; private set; }

    /// <summary>
    /// Array of steering post-processor behaviors
    /// </summary>
    public Steering[] SteeringPostprocessors { get; private set; }

    /// <summary>
    /// Current vehicle velocity.
    /// </summary>
    public abstract Vector3 Velocity { get; protected set; }

    /// <summary>
    /// Current magnitude for the ObjectAI's velocity.
    /// </summary>
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

    protected override void Awake()
    {
        base.Awake();
        Rigidbody = GetComponent<Rigidbody>();
        var allSteering = GetComponents<Steering>();
        Steerings = allSteering.Where(x => !x.IsPostProcess).ToArray();
        SteeringPostprocessors = allSteering.Where(x => x.IsPostProcess).ToArray();

        Radar = GetComponent<Radar>();
    }

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
    
    /// <summary>
    /// Avoidance of close neighbors
    /// </summary>
    /// <param name="_mindSeparationDistance">
    /// Minimum separation distance
    /// </param>
    /// <returns>
    /// Return the perpendicular if distance less than "_mindSeparationDistance"
    /// </returns>
    public Vector3 SteerToAvoidCloseNeighbors(float _mindSeparationDistance)
    {
        foreach (var other in Radar.Obstacles)
        {
            float sumOfRadius = Radius + other.Radius;
            float minCenterToCenter = _mindSeparationDistance + sumOfRadius;
            Vector3 offset = other.Position - Position;
            float currenDistance = offset.magnitude;
            if (currenDistance < minCenterToCenter)
            {
                float projection = Vector3.Dot(offset, transform.forward);
                Vector3 perpendicular = transform.forward * projection;
                perpendicular = -offset - perpendicular;

                return perpendicular;
            }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Determine the time until nearest approach
    /// </summary>
    /// <param name="_other">
    /// ObjectAI detect by radar
    /// </param>
    /// <returns>
    /// The nearest approach time
    /// </returns>
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

    /// <summary>
    /// Calculates if a vehicle is in the neighborhood of another
    /// </summary>
    /// <param name="_other">
    /// ObjectAI detect by radar
    /// </param>
    /// <param name="_minDistance">
    /// Minimum distance
    /// </param>
    /// <param name="_maxDistance">
    /// Maximum distance
    /// </param>
    /// <param name="_cosMaxAngle">
    /// Cosine of the maximum angle between vehicles
    /// </param>
    /// <returns>
    /// True if the other ObjectAI is considered to our neighbor
    /// False if otherwise
    /// </returns>
    public bool IsInNeighborhood(ObjectAI _other, float _minDistance, float _maxDistance, float _cosMaxAngle)
    {
        Vector3 offset = _other.Position - Position;
        float distanceSquared = offset.sqrMagnitude;

        if (distanceSquared < (_minDistance * _minDistance))
        {
            return true;
        }
        else
        {
            if (distanceSquared > (_maxDistance * _maxDistance))
            {
                return false;
            }
            else
            {
                Vector3 unitOffset = offset / Mathf.Sqrt(distanceSquared);
                float forwardness = Vector3.Dot(transform.forward, unitOffset);
                if (forwardness > _cosMaxAngle)
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
    #endregion

    protected override void OnDrawGizmos()
    {

        if (drawGizmos)
        {
            base.OnDrawGizmos();

            Debug.DrawLine(Position, DesiredVelocity + Position, Color.green);
        }
    }
}
