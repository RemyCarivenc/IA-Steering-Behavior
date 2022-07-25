using ImmersiveFactory.Tools.AI.Attributes;
using ImmersiveFactory.Tools.AI.Steer;
using System.Linq;
using UnityEngine;

namespace ImmersiveFactory.Tools.AI.Entities
{
    /// <summary>
    /// Base class for vehicles.false It does not move the objects, and instead
    /// provides a set of basic functionality for its subclasses.
    /// </summary>
    public abstract class Vehicle : Entity
    {
        #region Private fields

        [SerializeField]
        private float minSpeedForTurning = 0.1f;

        /// <summary>
        /// The vehicle movement priority
        /// </summary>
        /// <remarks>
        /// Used only by some behaviors to determine if a vehicle should
        /// be given priority before another one.!-- You may disregard if you aren't
        /// using any behavior like that.
        /// </remarks>
        [SerializeField]
        private int movementPriority;

        /// <summary>
        /// Across how many seconds is the vehicle's forward orientation smoothed
        /// </summary>
        [SerializeField]
        private float turnTime = 0.25f;

        /// <summary>
        /// Vehicle's mass
        /// </summary>
        /// <remarks>
        /// The total force from the steering behaviors will be divided by the
        /// vehicle mass before applying
        /// </remarks>
        [SerializeField]
        private float mass = 1;

        /// <summary>
        /// Indicates which axes a vehicle is allowed to move on
        /// </summary>
        /// <remarks>
        /// A 0 on the X/Y/Z value means the vehicle is not allowed to move on that 
        /// axis, a 1 indicates it can. We use Vector3Toggle to set it on the 
        /// editor as a helper
        /// </remarks>
        [SerializeField, Vector3Toggle]
        private Vector3 allowedMovementAxes = Vector3.one;
        
        /// <summary>
        /// The vehicle's arrival radius
        /// </summary>
        /// <remarks>
        /// The difference between the radius and arrival raidus is that
        /// the first is used to determine the area the vehicle covers, whereas the
        /// second one is a value used to determine if a vehicle is close enough
        /// to a desired target. Unlike the radius, it is not scaled with the vehicle
        /// </remarks>
        [SerializeField]
        private float arrivalRadius = 0.25f;

        [SerializeField]
        private float maxSpeed =1;

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
        /// The velocity desired by this vehicle, likely calculated by means
        /// similar to what AutonomousVehicle
        /// </summary>
        public Vector3 DesiredVelocity { get; protected set; }
        
        public float Mass
        {
            get { return mass ;}
            set { mass = Mathf.Max(0, value); }
        }

        public float MaxForce
        {
            get { return maxForce ;}
            set { maxForce = Mathf.Clamp(value, 0, float.MaxValue); }
        }

        public float MaxSpeed
        {
            get { return maxSpeed ;}
            set { maxSpeed = Mathf.Clamp(value, 0, float.MaxValue); }
        }

        public float MinSpeedForTurning
        {
            get { return minSpeedForTurning; }
        }

        public int MovementPriority
        {
            get { return movementPriority; }
        }
        
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
        /// Last raw force applied to the vehicle. It is expected to be set 
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
        /// Current magnitude for the vehicle's velocity.
        /// </summary>
        /// <remarks>
        /// It is expected to be set at the same time that the Velocity is 
        /// assigned in one of the descendent classes.  It may or may not
        /// match the vehicle speed, depending on how that is calculated - 
        /// for example, some subclasses can use a Speedometer to calculate
        /// their speed.
        /// </remarks>
        public float TargetSpeed { get; protected set; }

        /// <summary>
        /// The delta time used by this vehicle.
        /// </summary>
        /// <value>The delta time.</value>
        /// <remarks>
        /// Vehicles aren't necessarily ticked every frame, so we keep a
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

            if(movementPriority == 0)
                movementPriority = gameObject.GetInstanceID();
            
            Radar = GetComponent<Radar>();
            // Speedometer = GetComponent<Speedometer>();
            SquaredArrivalRadius = ArrivalRadius * ArrivalRadius;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Predicts where the vehicle will be at a point in the future
        /// </summary>
        /// <param name="_predictionTime">
        /// A time in seconds for the prediction
        /// </param>
        /// <returns>
        /// Vehicle position
        /// </returns>
        public Vector3 PredictFuturePosition(float _predictionTime)
        {
            return Position + (Velocity * _predictionTime);
        }
        
        /// <summary>
        /// Predicts where the vehicle wants to be at a point in the future
        /// </summary>
        /// <param name="_predictionTime">
        /// A time in seconds for the prediction
        /// </param>
        /// <returns>
        /// Vehicle position
        /// </returns>
        public Vector3 PredictFutureDesiredPosition(float _predictionTime)
        {
            return Position + (DesiredVelocity * _predictionTime);
        }

        /// <summary>
        /// Calculates if a vehicle is in the neighborhood of another
        /// </summary>
        /// <param name="_other">
        /// Another vehicle to check against
        /// </param>
        /// <param name="_minDistance">
        /// Minimum distance
        /// </param>
        /// <param name="_maxDistance">
        /// Maximun distance
        /// </param>
        /// <param name="_cosMaxAngle">
        /// Cosine of the maximun angle between vehicles (for performance)
        /// </param>
        /// <returns>
        /// True if the other vehicle can be considered to our neighbor, or false if otherwise
        /// </returns>
        public bool IsInNeighborhood(Vehicle _other, float _minDistance, float _maxDistance, float _cosMaxAngle)
        {
            bool result = false;
            if(_other != this)
            {
                Vector3 offset = _other.Position - Position;
                float distanceSquared = offset.sqrMagnitude;

                // definitely in neighborhood if inside minDistance sphere
                if (distanceSquared < (_minDistance * _minDistance))
                {
                    result = true;
                }
                else
                {
                    // definitely not in neighborhood if outside maxDistance sphere
                    if (distanceSquared <= (_maxDistance * _maxDistance))
                    {
                        // otherwise, test angular offset from forward axis
                        Vector3 unitOffset = offset / Mathf.Sqrt(distanceSquared);
                        float forwardness = Vector3.Dot(transform.forward, unitOffset);
                        result = forwardness > _cosMaxAngle;
                    }
                }
            }
            return result;
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
        /// Returns the distance from this vehicle to another
        /// </summary>
        /// <param name="_other">
        /// Vehicle to compare against
        /// </param>
        /// <returns>
        /// The distance between both vehicles' positions. If negative, they are overlapping
        /// </returns>
        public float DistanceFromPerimeter(Vehicle _other)
        {
            Vector3 diff = Position - _other.Position;
            return diff.magnitude - Radius - _other.Radius;
        }
        
        /// <summary>
        /// Reset the vehicle's orientation
        /// </summary>
        public void ResetOrientation()
        {
            transform.up = Vector3.up;
            transform.forward = Vector3.forward;
        }

        /// <summary>
        /// Predicts the time until nearest approach between this and another vehicle
        /// </summary>
        /// <param name="_other">
        /// Other vehicle to compare against
        /// </param>
        /// <returns>
        /// The nearest approach time
        /// </returns>
        public float PredictNearestApproachTime(Vehicle _other)
        {
            /*
            * Imagine we are at the origin with no velocity,
            * compute the relative velocity of the other vehicle
            */
            Vector3 otherVelocity = _other.Velocity;
            Vector3 relVelocity = otherVelocity - Velocity;
            float relSpeed = relVelocity.magnitude;
            
            /*
            * For parallel paths, the vehicles will always be at the same distance,
            * so return 0 since "there is no time like the present"
            */
            if(Mathf.Approximately(relSpeed, 0))
            {
                return 0;
            }

            /*
            * Now consider the path of the other vehicle in this relative
            * space, a line defined by the relative position and velocity.
            * The distance from the origin (our vehicle) to that line is
            * the nearest approach.
            */
           
            //Take the unity tangent along the other vehicle's path
            Vector3 relTangent = relVelocity / relSpeed;

            // find distance from its path to origin (compute offset from
            // other to us, find legtn of projection onto path)
            Vector3 relPosition = Position - _other.Position;
            float projection = Vector3.Dot(relTangent, relPosition);

            return projection/relSpeed;
        }

        /// <summary>
        /// Given the time until nearest approach (predictNearestApproachTime)
        /// determine position of each vehicle at that time, and the distance
        /// between them
        /// </summary>
        /// <param name="_other">
        /// Other vehicle to compare against
        /// </param>
        /// <param name="_time">
        /// Time to estimate
        /// </param>
        /// <param name="_ourPosition">
        /// Our position
        /// </param>
        /// <param name="_hisPosition">
        /// The other vehicle's position.
        /// </param>
        /// <returns>
        /// Distance between positions
        /// </returns>
        public float ComputeNearestApproachPositions(Vehicle _other, float _time, ref Vector3 _ourPosition, ref Vector3 _hisPosition)
        {
            return ComputeNearestApproachPositions(_other, _time, ref _ourPosition, ref _hisPosition, Speed, transform.forward);
        }

        public float ComputeNearestApproachPositions(Vehicle _other, float _time, ref Vector3 _ourPosition, ref Vector3 _hisPosition, float _ourSpeed, Vector3 _ourForward)
        {
            Vector3 myTravel = _ourForward * _ourSpeed * _time;
            Vector3 otherTravel = _other.transform.forward * _other.Speed * _time;

            _ourPosition = Position + myTravel;
            _hisPosition = _other.Position + otherTravel;

            return Vector3.Distance(_ourPosition, _hisPosition);
        }
        #endregion

        protected override void OnDrawGizmos() {

            if(drawGizmos)
            {
                base.OnDrawGizmos();
                //DesiredVelocity
                Debug.DrawLine(Position, DesiredVelocity + Position, Color.green);
            }
        }
    }
}
