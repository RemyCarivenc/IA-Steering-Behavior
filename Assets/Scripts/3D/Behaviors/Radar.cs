using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TickedPriorityQueue;
using System;

/// <summary>
/// Base class for Radar
/// </summary>
public class Radar : MonoBehaviour
{
    private static IDictionary<int, Entity> knownDetectableObjects = new SortedDictionary<int, Entity>();

    [SerializeField]
    private bool drawGizmos;

    private TickedObject tickedObject;
    private UnityTickedQueue steeringQueue;

    [SerializeField]
    private string queueName = "Radar";

    /// <summary>
    /// The maximum number of radar update calls processed on the queue per update
    /// </summary>
    [SerializeField]
    private int maxQueueProcessedPerUpdate = 20;

    /// <summary>
    /// How often is the radar updated
    /// </summary>
    [SerializeField]
    private float tickLength = 0.5f;

    /// <summary>
    /// Radar ping detection radius
    /// </summary>
    [SerializeField]
    private float detectionRadius = 5;

    /// <summary>
    /// Indicates if the radar will detect disabled vehicles. 
    /// </summary>
    [SerializeField]
    private bool detectDisabledObjectAI;

    /// <summary>
    /// Layer mask for the object layers checked
    /// </summary>
    [SerializeField]
    private LayerMask layersChecked;

    [SerializeField]
    private int preAllocateSize = 30;

    /// <summary>
    /// List of currently detected neighbors
    /// </summary>
    private Collider[] detectedColliders;

    private List<Entity> detectedObjects;

    /// <summary>
    /// List of ObjectAI detected among the colliders
    /// </summary>
    private List<ObjectAI> objectAIs;

    /// <summary>
    /// List of obstacles detected by the radar
    /// </summary>
    private List<Entity> obstacles;

    #region Public properties

    public List<Entity> Obstacles
    {
        get { return obstacles; }
    }

    /// <summary>
    /// Gets the ObjectAI this radar is attached to
    /// </summary>
    public ObjectAI ObjectAI { get; private set; }

    /// <summary>
    /// Returns the radars position
    /// </summary>
    public Vector3 Position
    {
        get { return (ObjectAI != null) ? ObjectAI.Position : transform.position; }
    }

    public List<ObjectAI> ObjectAIs
    {
        get { return objectAIs; }
    }

    #endregion

    #region Static Methods

    /// <summary>
    /// Must be called when a Entity is enabled so they can be easily identified
    /// </summary>
    /// <param name="obj">
    /// Entity
    /// </param>
    public static void AddDetectableObject(Entity _obj)
    {
        knownDetectableObjects[_obj.Collider.GetInstanceID()] = _obj;
    }

    /// <summary>
    /// Must be called when a Entity is disabled to remove it from the list of known objects
    /// </summary>
    /// <param name="obj">
    /// Entity
    /// </param>
    /// <returns>
    /// True if the call to Remove succeeded
    /// </returns>
    public static bool RemoveDetectableObject(Entity _obj)
    {
        return knownDetectableObjects.Remove(_obj.Collider.GetInstanceID());
    }

    #endregion



    private void Awake()
    {
        ObjectAI = GetComponent<ObjectAI>();
        objectAIs = new List<ObjectAI>(preAllocateSize);
        obstacles = new List<Entity>(preAllocateSize);
        detectedObjects = new List<Entity>(preAllocateSize * 3);
    }

    private void OnEnable()
    {
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
    
    #region Methods

    private void OnUpdateRadar(object obj)
    {
        detectedColliders = Detect();
        FilterDetected();
    }

    private Collider[] Detect()
    {
        return Physics.OverlapSphere(Position, detectionRadius, layersChecked);
    }

    private void FilterDetected()
    {
        objectAIs.Clear();
        obstacles.Clear();
        detectedObjects.Clear();

        for (int i = 0; i < detectedColliders.Length; i++)
        {
            int id = detectedColliders[i].GetInstanceID();
            if (!knownDetectableObjects.ContainsKey(id))
            {
                continue;
            }
            var detectable = knownDetectableObjects[id];
            if (detectable != null && detectable != ObjectAI && !detectable.Equals(null))
            {
                detectedObjects.Add(detectable);
            }
        }

        for (int i = 0; i < detectedObjects.Count; i++)
        {
            Entity d = detectedObjects[i];
            ObjectAI v = d as ObjectAI;
            if (v != null && (v.enabled || detectDisabledObjectAI))
            {
                objectAIs.Add(v);
            }
            else
            {
                obstacles.Add(d);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            var pos = (ObjectAI == null) ? transform.position : ObjectAI.Position;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pos, detectionRadius);
        }
    }

    #endregion

}
