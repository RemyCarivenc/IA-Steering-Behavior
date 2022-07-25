using UnityEngine;
using ImmersiveFactory.Tools.AI.Entities;

namespace ImmersiveFactory.Tools.AI.Steer
{
    public class Evasion_Steering : Steering
    {
        [SerializeField]
        private Vehicle target;
        [SerializeField] 
        private float predictionTime;
        [SerializeField] 
        private float safetyDistance = 2f;

        private float sqrSafetyDistance;

        private Vector3 targetOldPosition = Vector3.zero;

        public float SafetyDistance
        {
            get { return safetyDistance; }
            set
            {
                safetyDistance = value;
                sqrSafetyDistance = safetyDistance * safetyDistance;
            }
        }

        public Vehicle Target
        {
            get { return target;}
            set { target = value;}
        }

        protected override void Start() {
            sqrSafetyDistance = safetyDistance * safetyDistance;
        }

        protected override Vector3 CalculateForce()
        {
            if(target == null || (Vehicle.Position - target.Position).sqrMagnitude > sqrSafetyDistance)
            {
                return Vector3.zero;
               
            }
            else
            {
                Vector3 position = Vehicle.PredictFutureDesiredPosition(predictionTime);
                Vector3 offset = target.Position - Vehicle.Position;
                float distance = offset.magnitude;

                float roughTime = distance / target.Speed;
                float p;
                if(roughTime > predictionTime)
                    p = predictionTime;
                else
                    p = roughTime;

                Vector3 newTarget = target.PredictFuturePosition(p);

                Vector3 desiredVelocity = position - newTarget;
                return desiredVelocity - Vehicle.DesiredVelocity;
            }
        }
    }
}
