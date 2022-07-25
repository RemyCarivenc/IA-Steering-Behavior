using System;
using System.Collections.Generic;
using UnityEngine;
using TickedPriorityQueue;

/// <summary>
/// A helper class to create Unity updated Ticked Priority Queues.
/// Instance will return the default instance, and CreateInstance will return a named instance.
/// MaxProcessedPerUpdate will get or set the max number of items to be processes per Unity update.
/// </summary>
public class UnityTickedQueue : MonoBehaviour
{
    #region Instances

    private static Dictionary<string, UnityTickedQueue> instances;
    private static UnityTickedQueue instance;

    /// <summary>
    /// Retrieves a default static instance for ease of use
    /// The name of the created GameObject will be Ticked Queue
    /// </summary>
    public static UnityTickedQueue Instance
    {
        get
        {
            if(instance == null)
            {
                instance = CreateInstance(null);
            }
            return instance;
        }
    }

    /// <summary>
    /// Retrives a named custom instance
    /// The queue's GameObject will be named Ticked Queue - name
    /// If the name already exists, it will retrieve the older named instance
    /// </summary>
    public static UnityTickedQueue GetInstance(string _name)
    {
        if(string.IsNullOrEmpty(_name)) return Instance;

        _name = _name.ToLower();

        UnityTickedQueue queue = null;
        if(instances == null)
            instances = new Dictionary<string,UnityTickedQueue>();
        else
        {
            instances.TryGetValue(_name, out queue);
        }

        if(queue == null)
        {
            queue = CreateInstance(_name);
            instances[_name] = queue;
        }

        return queue;
    }

    private static UnityTickedQueue CreateInstance(string _name)
    {
        if(string.IsNullOrEmpty(_name)) _name = "Ticked Queue";
        else _name = "Ticked Queue - " + _name;
        GameObject go = new GameObject(_name);
        return go.AddComponent<UnityTickedQueue>();
    }

    #endregion

    private TickedQueue queue = new TickedQueue();

    public bool IsPaused
    {
        get { return queue.IsPaused; }
        set
        {
            queue.IsPaused = value;
            enabled = !value;
        }
    }

    public TickedQueue Queue
    {
        get { return this.queue; }
    }

    private void OnEnable() {
        queue.TickExceptionHandler = LogException;
    }

    /// <summary>
    /// Adds an ITicked reference to the queue.
    /// </summary>
    /// <param name="_ticked">
    /// A <see cref = "ITicked"/> reference, which will be ticked periodically based on its properties.
    /// </param>
    public void Add(ITicked _ticked)
    {
        queue.Add(_ticked);
    }

    /// <summary>
    /// Removes an ITicked reference from the queue.
    /// </summary>
    /// <param name="_ticked">
    /// A <see cref="ITicked"/> reference, which will be ticked periodically based on its properties.
    /// </param>
    /// <returns>True if the item was successfully removed, false if otherwise</returns>
    public bool Remove(ITicked _ticked)
    {
        return queue.Remove(_ticked);
    }

    /// <summary>
	/// Sets the maximum number of items to be processed every time Unity updates.
	/// </summary>
    public int MaxProcessedPerUpdate
    {
        get { return queue.MaxProcessedPerUpdate; }
        set { queue.MaxProcessedPerUpdate = value; }
    }

    private void Update()
    {
        queue.Update();
    }

    void LogException(Exception e, ITicked _ticked)
    {
        Debug.Log(e, this);
    }
}
