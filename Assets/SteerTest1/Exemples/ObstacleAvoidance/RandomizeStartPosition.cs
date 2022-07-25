using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImmersiveFactory.Tools.AI.Attributes;

public class RandomizeStartPosition : MonoBehaviour
{
    public Vector3 radius = Vector3.one;

    [Vector3Toggle]
    public Vector3 allowedAxes = Vector3.one;

    private void Start() 
    {
        var pos = Vector3.Scale(Random.insideUnitSphere, radius);
        pos = Vector3.Scale(pos, allowedAxes);
        transform.position += pos;
        
        var rot = Random.insideUnitSphere;

        if(allowedAxes.y == 0)
        {
            rot.x = 0;
            rot.z = 0;
        }

        if(allowedAxes.x ==0)
        {
            rot.y = 0;
            rot.z = 0;
        }

        if(allowedAxes.z == 0)
        {
            rot.x = 0;
            rot.y = 0;
        }

        transform.rotation = Quaternion.Euler(rot * 360);
        Destroy(this);
    }
}
