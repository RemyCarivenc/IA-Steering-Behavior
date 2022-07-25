
using UnityEngine;

namespace ImmersiveFactory.Tools.AI.Steer
{
    public class Seek_Steering : Steering
    {
        [SerializeField]
        private Transform targetPoint = null;
        [SerializeField]
        private bool arrival = false;
        private Vector3 desiredVelocity;
        
        public bool Arrival
        {
            get { return arrival; }
            set { arrival = value; }
        }

        public Transform TargetPoint
        {
            get { return targetPoint;}
            set { targetPoint = value;}
        }
        
        protected override Vector3 CalculateForce()
        {
            if(targetPoint != null)
            {
                desiredVelocity = Vector3.zero;
                Vector3 difference = targetPoint.position - Vehicle.Position;
                
                if(arrival)
                {
                    float d = difference.sqrMagnitude;
                    if(d>Vehicle.SquaredArrivalRadius)
                    {
                        desiredVelocity = difference - Vehicle.Velocity;
                    }
                }
                else
                {
                    desiredVelocity = difference.normalized * Vehicle.MaxSpeed;
                    //desiredVelocity =  desiredVelocity - Vehicle.Velocity;
                }
                    

                return desiredVelocity;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }
}
