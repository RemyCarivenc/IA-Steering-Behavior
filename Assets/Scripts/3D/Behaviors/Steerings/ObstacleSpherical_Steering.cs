using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleSpherical_Steering : Steering
{
    public bool drawGizmos = false;

    Vector3 avoidance;

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
    protected override Vector3 CalculateForce()
    {
        avoidance = Vector3.zero;

        if (ObjectAI.Radar.Obstacles == null || !ObjectAI.Radar.Obstacles.Any())
            return avoidance;

        // first priority is to prevent immediate interpenetration
        Vector3 separation = ObjectAI.SteerToAvoidCloseNeighbors(0, ObjectAI);
        if (separation != Vector3.zero)
            return separation;

        PathIntersection next = new PathIntersection(null);
        PathIntersection nearest = new PathIntersection(null);

        foreach (var obstacle in ObjectAI.Radar.Obstacles)
        {
            if (obstacle == null || obstacle.Equals(null))
                continue; // In case the object was destroyed since we cached it
            next = FindNextIntersectionWithSphere(obstacle);

            if (!nearest.Intersect || (next.Intersect && next.Distance < nearest.Distance))
                nearest = next;
        }

        if (nearest.Intersect)
        {
            Vector3 offset = nearest.Obstacle.Position - ObjectAI.Position;
            float projection = Vector3.Dot(offset, ObjectAI.transform.forward);
            Vector3 perpendicular = ObjectAI.transform.forward * projection;
            avoidance = -offset - perpendicular;
            avoidance = avoidance.normalized;
            avoidance *= ObjectAI.MaxForce;
            avoidance += ObjectAI.transform.forward * 0.75f;
        }

        return avoidance;
        /* Vector3 avoidance = Vector3.zero;

         if (ObjectAI.Radar.Obstacles == null || !ObjectAI.Radar.Obstacles.Any())
             return avoidance;

         Vector3 futurePosition = ObjectAI.PredictFutureDesiredPosition(estimationTime);

         foreach (var sphere in ObjectAI.Radar.Obstacles)
         {
             if (sphere == null || sphere.Equals(null))
                 continue; // In case the object was destroyed since we cached it
             PathIntersection next = FindNextIntersectionWithSphere(sphere);
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

         return desiredVelocity;*/
    }

    private float Square(float _f)
    {
        return (_f * _f);
    }

    private PathIntersection FindNextIntersectionWithSphere(Entity _obstacle)
    {
        PathIntersection intersection = new PathIntersection(_obstacle);

        Vector3 futurePosition = ObjectAI.PredictFutureDesiredPosition(estimationTime);
        float combinedRadius = ObjectAI.Radius + _obstacle.Radius;

        Vector3 movement = futurePosition - ObjectAI.Position;
        Vector3 direction = movement.normalized;

        Vector3 objectAIToObstacle = _obstacle.Position - ObjectAI.Position;

        float projectionLength = Vector3.Dot(direction, objectAIToObstacle);

        if (projectionLength > (movement.magnitude + combinedRadius) || projectionLength < 0)
            return intersection;

        Vector3 obstacleToObjectAI = ObjectAI.Position - _obstacle.Position;

        float a = Square(movement.x) + Square(movement.y) + Square(movement.z);
        float b = 2.0f * (movement.x * obstacleToObjectAI.x + movement.y * obstacleToObjectAI.y + movement.z * obstacleToObjectAI.z);
        float c = Square(_obstacle.Position.x) + Square(_obstacle.Position.y) + Square(_obstacle.Position.z)
                + Square(ObjectAI.Position.x) + Square(ObjectAI.Position.y) + Square(ObjectAI.Position.z)
                - 2 * (_obstacle.Position.x * ObjectAI.Position.x + _obstacle.Position.y * ObjectAI.Position.y + _obstacle.Position.z * ObjectAI.Position.z)
                - Square(_obstacle.Radius + ObjectAI.Radius);
        float d = Square(b) - 4 * a * c;

        if (d < 0)
            return intersection;

        float t1 = (-b - Mathf.Sqrt(d)) / (2.0f * a);
        float t2 = (-b + Mathf.Sqrt(d)) / (2.0f * a);

        Vector3 point1 = new Vector3(ObjectAI.Position.x * (1 - t1) + t1 * futurePosition.x,
                                     ObjectAI.Position.y * (1 - t1) + t1 * futurePosition.y,
                                     ObjectAI.Position.z * (1 - t1) + t1 * futurePosition.z);


        Vector3 point2 = new Vector3(ObjectAI.Position.x * (1 - t2) + t2 * futurePosition.x,
                                     ObjectAI.Position.y * (1 - t2) + t2 * futurePosition.y,
                                     ObjectAI.Position.z * (1 - t2) + t2 * futurePosition.z);

        intersection.Intersect = true;
        intersection.Distance = (t1 <= t2) ? t1 : t2;

        return intersection;
    }

    /*public PathIntersection FindNextIntersectionWithSphere(ObjectAI _objectAI, Vector3 _futureObjectAIPosition, Entity _obstacle)
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
    }*/

    private void OnDrawGizmos()
    {
        if (ObjectAI == null || !drawGizmos) return;
        foreach (var o in ObjectAI.Radar.Obstacles.Where(x => x != null))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(o.Position, o.Radius);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(ObjectAI.Position, ObjectAI.PredictFutureDesiredPosition(estimationTime));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(ObjectAI.Position, ObjectAI.Position + avoidance);
    }
}

