using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The path is a "polyline" a series of line segments between specified points.  
/// A radius defines a volume for the path which is the union of a sphere at each
/// point and a cylinder along each segment.
/// </summary>
public class PathWay
{
    public int pointCount;
    public Vector3[] points;
    public float radius;
    public bool cyclic;

    private float outside;

    float segmentLength;
    float segmentProjection;
    Vector3 local;
    Vector3 chosenPoint;
    Vector3 segmentNormal;

    float[] lengths;
    Vector3[] normals;
    float totalPathLength;

    public float TotalPathLength
    {
        get { return totalPathLength; }
    }

    public float Outside
    {
        get { return outside; }
    }

    public Vector3[] Points
    {
        get { return points; }
    }

    public PathWay(Vector3[] _path, float _radius, bool _cyclic)
    {
        radius = _radius;
        pointCount = _path.Length;

        Initialize(_path, _cyclic);
    }

    public void Initialize(Vector3[] _points, bool _cyclic)
    {
        cyclic = _cyclic;
        totalPathLength = 0;
        if (cyclic)
            pointCount++;

        lengths = new float[pointCount];
        points = new Vector3[pointCount];
        normals = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            bool closeCycle = false;
            int j;

            if (cyclic && (i == pointCount - 1))
                closeCycle = true;

            if (closeCycle)
                j = 0;
            else
                j = i;

            points[i] = _points[j];

            if (i > 0)
            {
                normals[i] = points[i] - points[i - 1];
                lengths[i] = Vector3.Magnitude(normals[i]);

                normals[i] *= 1 / lengths[i];

                totalPathLength += lengths[i];
            }
        }
    }

    /// <summary>
    /// Given an arbitrary point ("A"), returns the nearest point ("P") on this path.  
    /// Also returns, via output arguments, the path tangent at
    /// P and a measure of how far A is outside the Pathway's "tube".  
    /// Note that a negative distance indicates A is inside the Pathway.
    /// </summary>
    /// <param name="_point">
    /// Reference point
    /// </param>
    /// <returns>
    /// Point on path
    /// </returns>
    public Vector3 MapPointToPath(Vector3 _point)
    {
        float d;
        float minDistance = float.MaxValue;
        Vector3 onPath = Vector3.zero;

        for (int i = 1; i < pointCount; i++)
        {
            segmentLength = lengths[i];
            segmentNormal = normals[i];
            d = PointToSegmentDistance(_point, points[i - 1], points[i]);
            if (d < minDistance)
            {
                minDistance = d;
                onPath = chosenPoint;
            }
        }

        outside = (onPath - _point).magnitude - radius;

        return onPath;
    }

    /// <summary>
    /// Maps the reference point to a distance along the path.
    /// </summary>
    /// <param name="_point">
    /// Reference point
    /// </param>
    /// <returns>
    /// The distance along the path for the point
    /// </returns>
    public float MapPointToPathDistance(Vector3 _point)
    {
        float d;
        float minDistance = float.MaxValue;
        float segmentLengthTotal = 0;
        float pathDistance = 0;

        for (int i = 1; i < pointCount; i++)
        {
            segmentLength = lengths[i];
            segmentNormal = normals[i];
            d = PointToSegmentDistance(_point, points[i - 1], points[i]);
            if (d < minDistance)
            {
                minDistance = d;
                pathDistance = segmentLengthTotal + segmentProjection;
            }
            segmentLengthTotal += segmentLength;
        }

        return pathDistance;
    }

    /// <summary>
    /// Given a distance along the path, convert it to a point on the path
    /// </summary>
    /// <param name="_pathDistance">
    /// Path distance to calculate corresponding point for
    /// </param>
    /// <returns>
    /// Position on the path
    /// </returns>
    public Vector3 MapPathDistanceToPoint(float _pathDistance)
    {
        float remaining = _pathDistance;
        if (cyclic)
        {
            remaining = _pathDistance % totalPathLength;
        }
        else
        {
            if (_pathDistance < 0) return points[0];
            if (_pathDistance >= totalPathLength) return points[pointCount - 1];
        }

        Vector3 result = Vector3.zero;

        for (int i = 1; i < pointCount; i++)
        {
            segmentLength = lengths[i];
            if (segmentLength < remaining)
                remaining -= segmentLength;
            else
            {
                float ratio = remaining / segmentLength;
                result = Vector3.Lerp(points[i - 1], points[i], ratio);
                break;
            }
        }
        return result;
    }

    /// <summary>
    /// Compute minimum distance from a point to a line segment
    /// </summary>
    /// <param name="_point">
    /// Future position
    /// </param>
    /// <param name="_ep0">
    /// Start segment
    /// </param>
    /// <param name="_ep1">
    /// End segment
    /// </param>
    /// <returns>
    /// Distance between future posisiotn and segment
    /// </returns>
    private float PointToSegmentDistance(Vector3 _point, Vector3 _ep0, Vector3 _ep1)
    {
        local = _point - _ep0;

        segmentProjection = Vector3.Dot(segmentNormal, local);
        if (segmentProjection < 0)
        {
            chosenPoint = _ep0;
            segmentProjection = 0;
            return (_point - _ep0).magnitude;
        }

        if (segmentProjection > segmentLength)
        {
            chosenPoint = _ep1;
            segmentProjection = segmentLength;
            return (_point - _ep1).magnitude;
        }

        chosenPoint = segmentNormal * segmentProjection;
        chosenPoint += _ep0;
        return Vector3.Distance(_point, chosenPoint);
    }
}
