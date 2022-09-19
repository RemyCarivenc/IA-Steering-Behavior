using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Alignment_Steering : Steering
{
    [SerializeField]
    private bool drawGizmos = false;
    [SerializeField]
    private float maxDistance;
    [SerializeField]
    private float minDistance;
    [SerializeField, Range(-1, 1)]
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
                steering += other.transform.forward;
                neighbors++;
            }
        }
        if (neighbors > 0)
            steering = ((steering / (float)neighbors) - ObjectAI.transform.forward).normalized;

        return steering;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && drawGizmos)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(ObjectAI.Position, ObjectAI.Position + steering);
        }
    }
}
