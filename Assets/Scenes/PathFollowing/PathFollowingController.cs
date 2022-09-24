using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollowingController : MonoBehaviour
{
    [SerializeField] bool isCyclic = false;
    [SerializeField] Transform pathRoot;

    ToStayOnPath_Steering steering;

    private void Start()
    {
        steering = GetComponent<ToStayOnPath_Steering>();
        List<Vector3> pathPoints = new List<Vector3>();

        for(int i = 0; i < pathRoot.childCount; i++)
            pathPoints.Add(pathRoot.GetChild(i).position);

        steering.PathWay = new PathWay(pathPoints.ToArray(),1,isCyclic);
    }
}
