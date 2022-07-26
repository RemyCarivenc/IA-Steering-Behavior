﻿using UnityEngine;
using TickedPriorityQueue;

/// <summary>
/// ObjectAI subclass oriented towards autonomous bipeds and vehicles, which 
/// will be ticked automatically to calculate their direction
/// </summary>
public abstract class TickedObjectAI : ObjectAI
{
    /// <summary>
    /// Priority queue for this objectAI's updates
    /// </summary>
    private UnityTickedQueue steeringQueue;

    [Header("TickedObjectAI")]
    /// <summary>
    /// The name of the steering queue for this ticked objectAI
    /// </summary>
    [SerializeField]
    private string queueName = "Steering";

    /// <summary>
    /// How often will this ObjectAI's steering calculations be ticked
    /// </summary>
    [SerializeField]
    private float tickLength = 0.01f;

    /// <summary>
    /// The maximum number of radar update calls processed on the queue per update
    /// </summary>
    /// <remarks>
    /// Notice that this is a limit shared across queue items of the same name, at
    /// least until we have some queue settings, so whatever value is set last for 
    /// the queue will win.  Make sure your settings are consistent for objects of
    /// the same queue
    /// </remarks>
    [SerializeField]
    private int maxQueueProcessedPerUpdate = 20;

    #region Public properties
    /// <summary>
    /// Last time the objectAI's tick was completed
    /// </summary>
    /// <value>The last tick time</value>
    public float PreviousTickTime { get; private set; }

    /// <summary>
    /// Current time that the tick was called
    /// </summary>
    /// <value>The current tick time</value>
    public float CurrentTickTime { get; private set; }

    /// <summary>
    /// The time delta between now and when the objectAI's previous tick time and the current one.
    /// </summary>
    /// <value>The delta time.</value>
    public override float DeltaTime
    {
        get { return CurrentTickTime - PreviousTickTime; }
    }

    /// <summary>
    /// Velocity vector used to orient the agent
    /// </summary>
    /// <remarks>
    /// This is expected to be set by the subclasses
    /// </remarks>
    public Vector3 OrientationVelocity { get; protected set; }

    public string QueueName
    {
        get { return queueName; }
        set { queueName = value; }
    }

    public UnityTickedQueue SteeringQueue
    {
        get { return steeringQueue; }
    }

    #endregion

    /// <summary>
    /// Ticked object for the objectAI, so that its owner can configure
    /// the priority as desired
    /// </summary>
    /// <value></value>
    public TickedObject TickedObject { get; private set; }

    private void Start()
    {
        PreviousTickTime = Time.time;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        TickedObject = new TickedObject(OnUpdateSteering);
        TickedObject.TickLength = tickLength;
        steeringQueue = UnityTickedQueue.GetInstance(QueueName);
        steeringQueue.Add(TickedObject);
        steeringQueue.MaxProcessedPerUpdate = maxQueueProcessedPerUpdate;
    }

    protected override void OnDisable()
    {
        DeQueue();
        base.OnDisable();
    }

    #region Private Methods

    private void DeQueue()
    {
        if (steeringQueue != null)
            steeringQueue.Remove(TickedObject);
    }

    protected void OnUpdateSteering(object obj)
    {
        if (enabled)
        {
            // We just calculate the forces, and expect the radar updates itself.
            CalculateForces();
        }
        else
        {
            /*
                This is an interesting edge case.

                Because of the way TickedQueue iterates through its items, we may have
                a case where:
                - The objectAI's OnUpdateSteering is enqueued into the work queue
                - An event previous to it being called causes it to be disabled, and de-queued
                - When the ticked queue gets to it, it executes and re-enqueues it

                Therefore we double check that we're not trying to tick it while disabled, and 
                if so we de-queue it.  Must review TickedQueue to see if there's a way we can 
                easily handle these sort of issues without a performance hit.
            */
            DeQueue();
        }
    }

    protected void CalculateForces()
    {
        PreviousTickTime = CurrentTickTime;
        CurrentTickTime = Time.time;

        if (!CanMove || Mathf.Approximately(MaxForce, 0) || Mathf.Approximately(MaxSpeed, 0))
        {
            return;
        }

        Vector3 force = Vector3.zero;

        for (var i = 0; i < Steerings.Length; i++)
        {
            var s = Steerings[i];
            if (s.enabled)
            {
                force += s.WeighedForce;
            }
        }

        // Enforce speed limit. Steering behaviors are expected to return a
        // final desired velocity, not a acceleration, so we apply them directly.
        var newVelocity = Vector3.ClampMagnitude(force / Mass, MaxForce);

        if (newVelocity.sqrMagnitude == 0)
        {
            ZeroVelocity();
            DesiredVelocity = Vector3.zero;
        }
        else
        {
            DesiredVelocity = newVelocity;
        }

        // Adjusts the velocity by applying the post-processing behaviors. 
        Vector3 adjustedVelocity = Vector3.zero;
        for (var i = 0; i < SteeringPostprocessors.Length; i++)
        {
            var s = SteeringPostprocessors[i];
            if (s.enabled)
                adjustedVelocity += s.WeighedForce;
        }

        if (adjustedVelocity != Vector3.zero)
        {
            adjustedVelocity = Vector3.ClampMagnitude(adjustedVelocity / Mass, MaxSpeed);
            newVelocity = adjustedVelocity;
        }

        // Update objectAI velocity
        SetCalculatedVelocity(newVelocity);
    }

    /// <summary>
    /// Applies a steering force to this objectAI
    /// </summary>
    /// <param name="elapsedTime">
    /// How long has elapsed since the last update
    /// </param>
    private void ApplySteeringForce(float elapsedTime)
    {
        // Euler integrate (per frame) velocity into position
        var acceleration = CalculatePositionDelta(elapsedTime);
        acceleration = Vector3.Scale(acceleration, AllowedMovementAxes);

        if (Rigidbody == null || Rigidbody.isKinematic)
        {
            transform.position += acceleration;
        }
        else
        {
            Rigidbody.MovePosition(Rigidbody.position + acceleration);
        }
    }

    /// <summary>
    /// Turns the objectAI towards his velocity vector
    /// </summary>
    /// <param name="deltaTime">
    /// Time delta to use for turn calculations
    /// </param>
    protected void AdjustOrientation(float deltaTime)
    {
        if (TargetSpeed > MinSpeedForTurning && Velocity != Vector3.zero)
        {
            var newForward = Vector3.Scale(OrientationVelocity, AllowedMovementAxes).normalized;
            if (TurnTime > 0)
            {
                newForward = Vector3.Slerp(transform.forward, newForward, deltaTime / TurnTime);
            }

            transform.forward = newForward;
        }
    }

    /// <summary>
    /// Records the velocity that was just calculated by CalculateForces in a
    /// manner that is specific to each subclass. 
    /// </summary>
    /// <param name="velocity">
    /// Newly calculated velocity
    /// </param>
    protected abstract void SetCalculatedVelocity(Vector3 velocity);

    /// <summary>
    /// Calculates how much the agent's position should change in a manner that
    /// is specific to the ObjectAI's implementation.
    /// </summary>
    /// <param name="deltaTime">
    /// Time delta to use in position calculations
    /// </param>
    protected abstract Vector3 CalculatePositionDelta(float deltaTime);

    /// <summary>
    /// Zeros this objectAI's velocity.
    /// </summary>
    protected abstract void ZeroVelocity();

    #endregion

    private void Update()
    {
        if (CanMove)
        {
            ApplySteeringForce(Time.deltaTime);
            AdjustOrientation(Time.deltaTime);
        }
    }

    #region Public Methods
    public void Stop()
    {
        CanMove = false;
        ZeroVelocity();
    }

    #endregion
}
