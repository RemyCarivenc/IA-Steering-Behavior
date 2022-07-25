using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TickedPriorityQueue;
using ImmersiveFactory.Tools.AI.Entities;
using System;

namespace ImmersiveFactory.Tools.AI
{
    public class Radar : MonoBehaviour
    {
        #region Private static properties
        
        private static IDictionary<int, Entity> knownDetectableObjects = new SortedDictionary<int, Entity>();
        
        #endregion
        
        #region Private properties
        
        [SerializeField]
        private bool drawGizmos;

        private TickedObject tickedObject;
        private UnityTickedQueue steeringQueue;

        private string queueName = "Radar";

        /// <summary>
        /// The maximum number of radar update calls processed on the queue per update
        /// </summary>
        /// <remarks>
        /// Notice that this is a limit shared across queue items of the same name, at
        /// least until we have some queue settings, so whatever value is set last for 
        /// the queue will win.  Make sure your settings are consistent for objects of
        /// the same queue.
        /// </remarks>
        [SerializeField] 
        private int maxQueueProcessedPerUpdate = 20;
        [SerializeField]
        private float tickLength = 0.5f;
        [SerializeField]
        private float detectionRadius = 5;
        [SerializeField]
        private bool detectDisabledVehicles;
        [SerializeField]
        private LayerMask layersChecked;
        [SerializeField]
        private int preAllocateSize = 30;

        private Collider[] detectedColliders;
        private List<Entity> detectedObjects;
        private List<Vehicle> vehicles;
        private List<Entity> obstacles;

        #endregion
        
        #region Public properties

        /// <summary>
        /// List of obstacles detected by the radar
        /// </summary>
        public List<Entity> Obstacles
        {
            get { return obstacles ;}
        }

        /// <summary>
        /// Gets the vehicle this radar is attached to
        /// </summary>
        public Vehicle Vehicle { get; private set; }
        
        public Vector3 Position
        {
            get { return (Vehicle != null) ? Vehicle.Position : transform.position; }
        }

        public Action<Radar> OnDetected = delegate { };

        public List<Vehicle> Vehicles
        {
            get { return vehicles; }
        }
        
        #endregion

        #region Static Methods

        /// <summary>
        /// Must be called when a Entity is enabled so they can be easily identified
        /// </summary>
        /// <param name="obj">Entity</param>
        public static void AddDetectableObject(Entity _obj)
        {
            knownDetectableObjects[_obj.Collider.GetInstanceID()] = _obj;
        }

        /// <summary>
        /// Must be called when a Entity is disabled to remove it from the list of known objects
        /// </summary>
        /// <param name="obj">Entity</param>
        /// <returns>True if the call to Remove succeeded</returns>
        public static bool RemoveDetectableObject(Entity _obj)
        {
            return knownDetectableObjects.Remove(_obj.Collider.GetInstanceID());
        }

        #endregion

        #region Methods

        private void Awake()
        {
            Vehicle = GetComponent<Vehicle>();
            vehicles = new List<Vehicle>(preAllocateSize);
            obstacles = new List<Entity>(preAllocateSize);
            detectedObjects = new List<Entity>(preAllocateSize * 3);
        }

        private void OnEnable() {
            tickedObject = new TickedObject(OnUpdateRadar)
            {
                TickLength = tickLength
            };
            steeringQueue = UnityTickedQueue.GetInstance(queueName);
            steeringQueue.Add(tickedObject);
            steeringQueue.MaxProcessedPerUpdate = maxQueueProcessedPerUpdate;
        }

        private void OnDisable()
        {
            if (steeringQueue != null)
            {
                steeringQueue.Remove(tickedObject);
            }
        }

        private void OnUpdateRadar(object obj)
        {
            detectedColliders = Detect();
            FilterDetected();
            if(OnDetected !=null)
            {
                OnDetected(this);
            }
        }

        private Collider[] Detect()
        {
            return Physics.OverlapSphere(Position, detectionRadius, layersChecked);
        }

        private void FilterDetected()
        {
            vehicles.Clear();
            obstacles.Clear();
            detectedObjects.Clear();

            for(int i = 0; i < detectedColliders.Length; i++)
            {
                int id = detectedColliders[i].GetInstanceID();
                if(!knownDetectableObjects.ContainsKey(id))
                {
                    continue;
                }
                var detectable = knownDetectableObjects[id];
                if(detectable != null && detectable != Vehicle && !detectable.Equals(null))
                {
                    detectedObjects.Add(detectable);
                }
            }

            for (int i = 0; i < detectedObjects.Count; i++)
            {
                Entity d = detectedObjects[i];
                Vehicle v = d as Vehicle;
                if(v != null && (v.enabled || detectDisabledVehicles))
                {
                    vehicles.Add(v);
                }
                else
                {
                    obstacles.Add(d);
                }
            }
        } 

        private void OnDrawGizmos() {
            if(drawGizmos)
            {
                var pos = (Vehicle == null) ? transform.position : Vehicle.Position;

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(pos, detectionRadius);
            }
        }

        #endregion

    }
}
