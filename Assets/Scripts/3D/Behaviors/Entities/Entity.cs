using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Entity")]
    [SerializeField]
    protected bool drawGizmos;
    [SerializeField]
    private Vector3 center;
    [SerializeField]
    private float radius;

    #region Public properties

    public Collider Collider { get; private set; }

    /// <summary>
    /// Vehicle center on the transform
    /// </summary>
    public Vector3 Center
    {
        get { return center; }
        set { center = value; }
    }

    /// <summary>
    /// Entity radius
    /// </summary>
    public float Radius
    {
        get { return radius; }
        set
        {
            radius = Mathf.Clamp(value, 0.01f, float.MaxValue);
            SquaredRadius = radius * radius;
        }
    }

    /// <summary>
    /// Vehicle's position
    /// </summary>
    /// <remarks>
    /// The vehicle's position is the transform's position displaced 
    /// by the vehicle center
    /// </remarks>
    public Vector3 Position
    {
        get { return transform.position + center; }
    }

    /// <summary>
    /// Calculated squared object radius
    /// </summary>
    public float SquaredRadius { get; private set; }

    #endregion

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

    #endregion

    protected virtual void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(Position, radius);
        }
    }
}
