using System;
using UnityEngine;

/// <summary>
/// Base Steering class from which other steering behaviors derive
/// </summary>
public abstract class Steering : MonoBehaviour
{
    /// <summary>
    /// Last force calculated
    /// </summary>
    private Vector3 force = Vector3.zero;

    /// <summary>
    /// ObjectAI that this behavior will influence
    /// </summary>
    private ObjectAI objectAI;

    /// <summary>
    /// Weight assigned to this steering behavior
    /// </summary>
    [SerializeField]
    private float weight = 1;


    #region Public properties

    /// <summary>
    /// Calculates the force provided by this steering behavior
    /// </summary>
    public Vector3 Force
    {
        get
        {
            force = CalculateForce();
            return force;
        }
    }

    public virtual bool IsPostProcess
    {
        get { return false; }
    }

    /// <summary>
    /// Force vector modified by the assigned weight 
    /// </summary>
    public Vector3 WeighedForce
    {
        get { return Force * weight; }
    }

    public ObjectAI ObjectAI
    {
        get { return objectAI; }
    }

    public float Weight
    {
        get { return weight; }
        set { weight = value; }
    }

    #endregion


    protected virtual void Awake()
    {
        objectAI = GetComponent<ObjectAI>();
    }
    protected virtual void Start()
    {
    }

    #region Methods
    /// <summary>
    /// Calculates the force supplied by this behavior
    /// </summary>
    /// <returns>
    /// A vector with the supplied force
    /// </returns>
    protected abstract Vector3 CalculateForce();

    #endregion
}
