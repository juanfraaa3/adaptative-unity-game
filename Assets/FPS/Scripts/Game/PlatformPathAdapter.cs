using System.Collections.Generic;
using UnityEngine;

public class PlatformPathAdapter : MonoBehaviour
{
    public MovingPlatformMultiple platform;
    private List<Transform> points = new List<Transform>();

    private void Awake()
    {
        if (platform == null)
        {
            Debug.LogError("PlatformPathAdapter: platform not assigned");
            return;
        }

        foreach (var p in platform.points)
            if (p != null) points.Add(p);
    }

    public Vector3 GetClosestPoint(Vector3 pos)
    {
        float best = float.MaxValue;
        Vector3 bestPoint = pos;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 a = points[i].position;
            Vector3 b = points[(i + 1) % points.Count].position;
            Vector3 p = ClosestPointOnSegment(pos, a, b);
            float d = (pos - p).sqrMagnitude;

            if (d < best)
            {
                best = d;
                bestPoint = p;
            }
        }
        return bestPoint;
    }

    private Vector3 ClosestPointOnSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }
    public float GetProgress01(Vector3 pos)
    {
        if (platform == null || platform.points == null || platform.points.Length < 2)
            return 0f;

        float totalLength = 0f;
        float bestDistSqr = float.MaxValue;
        float lengthAtClosest = 0f;

        float accumulated = 0f;

        for (int i = 0; i < platform.points.Length; i++)
        {
            Vector3 a = platform.points[i].position;
            Vector3 b = platform.points[(i + 1) % platform.points.Length].position;

            float segmentLength = Vector3.Distance(a, b);
            Vector3 p = ClosestPointOnSegment(pos, a, b);
            float d = (pos - p).sqrMagnitude;

            if (d < bestDistSqr)
            {
                bestDistSqr = d;
                lengthAtClosest = accumulated + Vector3.Distance(a, p);
            }

            accumulated += segmentLength;
            totalLength += segmentLength;
        }

        return totalLength > 0f ? Mathf.Clamp01(lengthAtClosest / totalLength) : 0f;
    }

}
