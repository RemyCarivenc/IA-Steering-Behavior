using UnityEngine;

namespace ImmersiveFactory.Tools.AI.Steer
{
    public class Flee_Steering : Steering
    {
        [SerializeField]
        private Transform targetPoint = null;
        private Vector3 desiredVelocity;

        public Transform TargetPoint
        {
            get { return targetPoint;}
            set { targetPoint = value;}
        }
        
        protected override Vector3 CalculateForce()
        {
            if(targetPoint != null)
            {
                desiredVelocity = (Vehicle.Position - targetPoint.position).normalized * Vehicle.MaxSpeed;

                return desiredVelocity - Vehicle.Velocity;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }
}

