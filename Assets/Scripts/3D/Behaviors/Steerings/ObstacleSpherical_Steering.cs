using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleSpherical_Steering : Steering
{
    public bool drawGizmos = false;

    public struct PathIntersection
    {
        public bool Intersect;
        public float Distance;
        public Entity Obstacle;

        public PathIntersection(Entity _obstacle)
        {
            Obstacle = _obstacle;
            Intersect = false;
            Distance = float.MaxValue;
        }
    }

    [SerializeField]
    private float estimationTime = 2;

    public override bool IsPostProcess
    {
        get { return true; }
    }

    [SerializeField]
    private float maxSeeAhead = 1;

    protected override Vector3 CalculateForce()
    {
        /*Vector3 ahead = Vehicle.Position + Vehicle.Velocity.normalized * maxSeeAhead;*/
        Vector3 avoidance = Vector3.zero;

        if (ObjectAI.Radar.Obstacles == null || !ObjectAI.Radar.Obstacles.Any())
            return avoidance;

        Vector3 futurePosition = ObjectAI.PredictFutureDesiredPosition(estimationTime);

        foreach (var sphere in ObjectAI.Radar.Obstacles)
        {
            if (sphere == null || sphere.Equals(null))
                continue; // In case the object was destroyed since we cached it
            PathIntersection next = FindNextIntersectionWithSphere(ObjectAI, futurePosition, sphere);
            float avoidanceMultiplier = 0f;
            if (next.Intersect)
            {
                float timeToObstacle = next.Distance / ObjectAI.Speed;
                avoidanceMultiplier = 2 * (estimationTime / timeToObstacle);

            }
            Vector3 oppositeDirection = ObjectAI.Position - sphere.Position;
            avoidance += avoidanceMultiplier * oppositeDirection;
        }

        avoidance /= ObjectAI.Radar.Obstacles.Count;

        Vector3 desiredVelocity = Vector3.Reflect(ObjectAI.DesiredVelocity, avoidance);

        return desiredVelocity;
        //return Vector3.zero;
    }

    public static PathIntersection FindNextIntersectionWithSphere(ObjectAI _objectAI, Vector3 _futureObjectAIPosition, Entity _obstacle)
    {
        // this mainly follows http://www.lighthouse3d.com/tutorials/maths/ray-sphere-intersection/

        PathIntersection intersection = new PathIntersection(_obstacle);

        float combinedRadius = _objectAI.Radius + _obstacle.Radius;

        Vector3 movement = _futureObjectAIPosition - _objectAI.Position;
        Vector3 direction = movement.normalized;

        Vector3 objectAIToObstacle = _obstacle.Position - _objectAI.Position;

        // The length of objectAIToObstacle projected onto direction
        float projectionLength = Vector3.Dot(direction, objectAIToObstacle);

        // if the projected obstacle center lies further away than our movement + both radius, we're not going to collide
        if (projectionLength > movement.magnitude + combinedRadius)
        {
            return intersection;
        }

        Vector3 projectedObstacleCenter = _objectAI.Position + projectionLength * direction;

        // distance of the obstacle to the pathe the objectAI is going to take
        float obstacleDistanceToPath = (_obstacle.Position - projectedObstacleCenter).magnitude;

        // if the obstacle is further away from the movement, than both radius, there's no collision
        if (obstacleDistanceToPath > combinedRadius)
        {
            return intersection;
        }

        float halfChord = Mathf.Sqrt(combinedRadius * combinedRadius + obstacleDistanceToPath * obstacleDistanceToPath);

        // if the projected obstacle center lies opposite to the movement direction (aka "behind")
        if (projectionLength < 0)
        {
            // behind and further away than both radius -> no collision (we already passed)
            if (objectAIToObstacle.magnitude > combinedRadius)
                return intersection;

            Vector3 intersectionPoint = projectedObstacleCenter - direction * halfChord;
            intersection.Intersect = true;
            intersection.Distance = (intersectionPoint - _objectAI.Position).magnitude;
            return intersection;
        }

        // calculate both intersection points
        Vector3 intersectionPoint1 = projectedObstacleCenter - direction * halfChord;
        Vector3 intersectionPoint2 = projectedObstacleCenter + direction * halfChord;

        // pick the closest one
        float intersectionPoint1Distance = (intersectionPoint1 - _objectAI.Position).magnitude;
        float intersectionPoint2Distance = (intersectionPoint2 - _objectAI.Position).magnitude;

        intersection.Intersect = true;

        intersection.Distance = Mathf.Min(intersectionPoint1Distance, intersectionPoint2Distance);

        return intersection;
    }

    private void OnDrawGizmos()
    {
        if (ObjectAI == null || !drawGizmos) return;
        foreach (var o in ObjectAI.Radar.Obstacles.Where(x => x != null))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(o.Position, o.Radius);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, ObjectAI.PredictFutureDesiredPosition(estimationTime));
    }
}

