using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WallsAvoidance_Steering : Steering
{
    public bool drawGizmos = false;

    [SerializeField]
    private float estimationTime = 2;

    public override bool IsPostProcess
    {
        get { return true; }
    }

    protected override Vector3 CalculateForce()
    {
        Vector3 avoidance = Vector3.zero;

        if (ObjectAI.Radar.Obstacles == null || !ObjectAI.Radar.Obstacles.Any())
            return avoidance;

        Vector3 futurePosition = ObjectAI.PredictFutureDesiredPosition(estimationTime);

        RaycastHit hit;
        if(Physics.Linecast(ObjectAI.Position,futurePosition,out hit))
        {
            Debug.LogError(hit.transform.name);
        }
        /*foreach (var wall in ObjectAI.Radar.Obstacles)
        {
            if (wall == null || wall.Equals(null) || !wall.CompareTag("Wall"))
                continue; // In case the object was destroyed since we cached it
        }*/
        return avoidance;
    }

    private void OnDrawGizmos()
    {
        if (ObjectAI == null || !drawGizmos) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, ObjectAI.PredictFutureDesiredPosition(estimationTime));
    }
}
