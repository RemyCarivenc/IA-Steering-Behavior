using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImmersiveFactory.Tools.AI.Entities
{
    public class Entity : MonoBehaviour
    {
        [SerializeField]
        protected bool drawGizmos;
        [SerializeField] 
        private Vector3 center;
        [SerializeField]
        private float radius;

        public Collider Collider { get; private set; }    

        public Vector3 Center
        {
            get { return center; }
            set { center = value; }
        }

        /// <summary>
        /// Entity radius
        /// </summary>
        /// <remarks>
        /// This property's setter recalculates a temporary value, so it's
        /// advised you don't re-scale the entity's transform after it has been set
        /// </remarks>
        public float Radius
        {
            get { return radius; }
            set
            {
               radius = Mathf.Clamp(value, 0.01f, float.MaxValue);
               SquaredRadius = radius * radius;
            }
        }

        public Vector3 Position
        {
            get { return transform.position + center; }
        }

        public float SquaredRadius { get; private set; }

        #region Methods

        protected virtual void Awake()
        {
            Collider = GetComponent<Collider>();
            SquaredRadius = radius * radius;
        }

        protected virtual void OnEnable()
        {
            if (Collider)
            {
                Radar.AddDetectableObject(this);
            }
        }

        protected virtual void OnDisable()
        {
            if (Collider)
            {
                Radar.RemoveDetectableObject(this);
            }
        }

        public void ScaleRadiusWithTransform(float _baseRadius)
        {
            var scale = transform.lossyScale;
            radius = _baseRadius * Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
        }
        #endregion

        protected virtual void OnDrawGizmos(){
            if(drawGizmos)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(Position, radius);

            }
        }
    }
}
