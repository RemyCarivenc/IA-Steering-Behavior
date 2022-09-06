using UnityEngine;

public class Pursuit_Steering : Steering
{
    [SerializeField]
    private float acceptableDistance = .0f;

    [SerializeField]
    private float maxPredictionTime = 2f;

    [SerializeField]
    private ObjectAI target;

    [SerializeField]
    private bool arrival = false;

    private bool Arrival
    {
        get { return arrival; }
        set { arrival = value; }
    }

    private float AcceptableDistance
    {
        get { return acceptableDistance; }
        set { acceptableDistance = value; }
    }

    protected override Vector3 CalculateForce()
    {
        if (target != null)
        {
            Vector3 desiredVelocity = Vector3.zero;
            Vector3 offset = target.Position - ObjectAI.Position;
            float distance = offset.magnitude;

            float radius = ObjectAI.Radius + target.Radius + acceptableDistance;

            if (arrival && distance < radius) return desiredVelocity;

            Vector3 unitOffset = offset / distance;

            // how parallel are the paths of "this" and the quarry
            // (1 means parallel, 0 is pependicular, -1 is anti-parallel)
            var parallelness = Vector3.Dot(transform.forward, target.transform.forward);

            // how "forward" is the direction to the quarry
            // (1 means dead ahead, 0 is directly to the side, -1 is straight back)
            var forwardness = Vector3.Dot(transform.forward, unitOffset);

            // While we could parametrize this value, if we care about forward/backwards
            // these values are appropriate enough.
            var f = IntervalComparison(forwardness, -0.707f, 0.707f);
            var p = IntervalComparison(parallelness, -0.707f, 0.707f);

            float timeFactor = 0; // to be filled in below

            // Break the pursuit into nine cases, the cross product of the
            // quarry being [ahead, aside, or behind] us and heading
            // [parallel, perpendicular, or anti-parallel] to us.
            switch (f)
            {
                case +1:
                    switch (p)
                    {
                        case +1: // ahead, parallel
                            timeFactor = 4;
                            break;
                        case 0: // ahead, perpendicular
                            timeFactor = 1.8f;
                            break;
                        case -1: // ahead, anti-parallel
                            timeFactor = 0.85f;
                            break;
                    }
                    break;
                case 0:
                    switch (p)
                    {
                        case +1: // aside, parallel
                            timeFactor = 1;
                            break;
                        case 0: // aside, perpendicular
                            timeFactor = 0.8f;
                            break;
                        case -1: // aside, anti-parallel
                            timeFactor = 4;
                            break;
                    }
                    break;
                case -1:
                    switch (p)
                    {
                        case +1: // behind, parallel
                            timeFactor = 0.5f;
                            break;
                        case 0: // behind, perpendicular
                            timeFactor = 2;
                            break;
                        case -1: // behind, anti-parallel
                            timeFactor = 2;
                            break;
                    }
                    break;
            }
            float directTravelTime = distance / ObjectAI.Speed;


            // estimated time until intercept of target
            var et = directTravelTime * timeFactor;
            var etl = (et > maxPredictionTime) ? maxPredictionTime : et;


            Vector3 futurePosition = target.PredictFuturePosition(etl);

            desiredVelocity = futurePosition - ObjectAI.Position;

            return desiredVelocity- ObjectAI.Velocity;
        }
        else
        {
            return Vector3.zero;
        }
    }

    private int IntervalComparison(float _x, float _lowerBound, float _upperBound)
    {
        if (_x < _lowerBound) return -1;
        if (_x > _upperBound) return +1;
        return 0;
    }
}
