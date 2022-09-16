using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof (Radar))]
public class Cohesion_Steering : Steering
{
    [SerializeField]
    private float maxDistance;
    [SerializeField]
    private float minDistance;
    [SerializeField,Range(-1,1)]
    private float cosMaxAngle = 0.7f;

    private int neighbors = 0;

    private Vector3 steering;

    protected override Vector3 CalculateForce()
    {
        steering = Vector3.zero;
        neighbors = 0;

        if (ObjectAI.Radar.ObjectAIs == null || !ObjectAI.Radar.ObjectAIs.Any())
            return steering;

        foreach (var other in ObjectAI.Radar.ObjectAIs)
        {
            if (ObjectAI.IsInNeighborhood(other, minDistance, maxDistance, cosMaxAngle))
            {
                steering += other.Position;
                neighbors++;
            }
        }

        if (neighbors > 0)
            steering = ((steering / neighbors) - ObjectAI.Position.normalized);

        return steering;
    }
}
