using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static FlickInputUI;

[RequireComponent(typeof(CanvasRenderer))]
public class UILineDrawer : Graphic
{
    public float thickness = 3f;
    private List<Vector2> points = new List<Vector2>();

    // This override is what actually draws the shape on the UI Canvas
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (points.Count < 2) return;

        for (int i = 0; i < points.Count - 1; i++)
        {
            // Draw the straight rectangle
            DrawLineSegment(points[i], points[i + 1], vh);

            // Draw a patch at the connection point to cover the gap
            if (i < points.Count - 2)
            {
                DrawJoint(points[i + 1], vh);
            }
        }

        // Draw joints at the very start and end to give the line nice square "caps"
        DrawJoint(points[0], vh);
        DrawJoint(points[points.Count - 1], vh);
    }

    public void DrawPerfectLine(Sectors[] sequence, float maxRadius)
    {
        points.Clear();

        foreach (Sectors s in sequence)
        {
            // Neutral is the center (0,0)
            if (s == Sectors.Neutral)
            {
                points.Add(Vector2.zero);
            }
            else
            {
                // Convert the Sector enum back to a Vector2 coordinate
                float angle = (int)s * 45f;
                float rad = angle * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                points.Add(dir * maxRadius);
            }
        }
        SetAllDirty();
    }

    void DrawJoint(Vector2 point, VertexHelper vh)
    {
        int count = vh.currentVertCount;
        float halfThick = thickness / 2f;

        // Draw a flat square exactly the size of our line thickness
        vh.AddVert(new Vector3(point.x - halfThick, point.y - halfThick), color, Vector2.zero);
        vh.AddVert(new Vector3(point.x - halfThick, point.y + halfThick), color, Vector2.zero);
        vh.AddVert(new Vector3(point.x + halfThick, point.y + halfThick), color, Vector2.zero);
        vh.AddVert(new Vector3(point.x + halfThick, point.y - halfThick), color, Vector2.zero);

        vh.AddTriangle(count, count + 1, count + 2);
        vh.AddTriangle(count, count + 2, count + 3);
    }

    void DrawLineSegment(Vector2 start, Vector2 end, VertexHelper vh)
    {
        Vector2 dir = (end - start).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x) * (thickness / 2);

        int count = vh.currentVertCount;

        // 'color' is automatically grabbed from the standard Graphic component in the Inspector
        vh.AddVert(start - normal, color, Vector2.zero);
        vh.AddVert(start + normal, color, Vector2.zero);
        vh.AddVert(end + normal, color, Vector2.zero);
        vh.AddVert(end - normal, color, Vector2.zero);

        vh.AddTriangle(count, count + 1, count + 2);
        vh.AddTriangle(count, count + 2, count + 3);
    }

    public void AddPoint(Vector2 point)
    {
        // Only add a point if we've moved a little bit (saves memory and keeps the line smooth)
        if (points.Count == 0 || Vector2.Distance(points[points.Count - 1], point) > 2f)
        {
            points.Add(point);
            SetAllDirty(); // Tells Unity to redraw the canvas
        }
    }

    public void Clear()
    {
        points.Clear();
        SetAllDirty();
    }
}
