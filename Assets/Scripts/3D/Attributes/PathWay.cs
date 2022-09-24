using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
