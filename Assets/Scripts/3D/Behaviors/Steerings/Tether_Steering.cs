using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Steers a vehicle to keep within a certain range of a point
/// </summary>
public class Tether_Steering : Steering
{
    [SerializeField]
    private bool drawGizmos = false;

    [SerializeField]
    private float maximumDistance = 30f;
    [SerializeField]
    private Vector3 tetherPosition;

    public override bool IsPostProcess
    {
        get { return true; }
    }

    public float MaximumDistance
    {
        get { return maximumDistance; }
        set { maximumDistance = Mathf.Clamp(value, 0, float.MaxValue); }
    }

    public Vector3 TetherPosition
    {
        get { return tetherPosition; }
        set { tetherPosition = value; }
    }

    protected override Vector3 CalculateForce()
    {
        Vector3 steering = Vector3.zero;

        Vector3 difference = tetherPosition - ObjectAI.Position;
        float distance = difference.magnitude;
        if (distance > maximumDistance)
            steering = (difference + ObjectAI.DesiredVelocity) / 2;
        return steering;
    }

    void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(tetherPosition,maximumDistance);
        }
    }
}
