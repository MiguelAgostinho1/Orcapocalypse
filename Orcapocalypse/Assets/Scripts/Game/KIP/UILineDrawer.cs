using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static FlickInputUI;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(CanvasRenderer))]
public class UILineDrawer : Graphic
{
    public float thickness = 3f;
    public float arrowSize = 10f;
    private List<Vector2> points = new List<Vector2>();
    private Vector2 arrowDirection = Vector2.zero;
    private bool showArrow = false;

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

        DrawJoint(points[0], vh);

        // If we are showing a success "Perfect Line", draw the arrowhead at the end
        if (showArrow && arrowDirection != Vector2.zero)
        {
            Vector2 finalPoint = points[points.Count - 1];
            Vector2 visualTip = finalPoint + (arrowDirection * (arrowSize * 0.8f));

            DrawArrowHead(visualTip, arrowDirection, vh);
        }
        else
        {
            DrawJoint(points[points.Count - 1], vh);
        }
    }

    public void DrawPerfectLine(Sectors[] sequence, float maxRadius, Vector2 finalDir)
    {
        points.Clear();
        showArrow = true;

        // 1. Start at Center
        points.Add(Vector2.zero);

        // Flip curveOffset to negative for "Regular" (Right-leaning)
        float curveOffset = -maxRadius * 0.4f;
        Vector2 perpendicular = new Vector2(-finalDir.y, finalDir.x).normalized;

        Vector2 startPos = GetPosFromSector(sequence[0], maxRadius);
        Vector2 endPos = GetPosFromSector(sequence[sequence.Length - 1], maxRadius);

        // 2. Add the straight pull-back
        points.Add(startPos);

        // 3. Draw the Curved Up-swing with higher resolution
        int curveResolution = 8; // Higher number = smoother line
        for (int i = 1; i <= curveResolution; i++)
        {
            float t = i / (float)curveResolution;

            // Sine-based bow
            float bowPower = Mathf.Sin(t * Mathf.PI) * curveOffset;
            Vector2 pathPoint = Vector2.Lerp(startPos, endPos, t);
            Vector2 offsetPoint = pathPoint + (perpendicular * bowPower);

            if (i == curveResolution)
            {
                // --- NEW: TILT CALCULATION ---
                // Calculate direction from the previous point to the last point
                // This ensures the arrow tilts with the curve
                Vector2 lastDir = (offsetPoint - points[points.Count - 1]).normalized;
                arrowDirection = lastDir;

                // Clip for the arrow head
                points.Add(offsetPoint - (lastDir * (arrowSize * 0.8f)));
            }
            else
            {
                points.Add(offsetPoint);
            }
        }

        SetAllDirty();
    }

    // Helper method for DrawPerfectLine
    private Vector2 GetPosFromSector(Sectors s, float radius)
    {
        float angle = (int)s * 45f;
        return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
    }

    void DrawArrowHead(Vector2 tip, Vector2 direction, VertexHelper vh)
    {
        int count = vh.currentVertCount;

        // Perpendicular vector for the base width
        Vector2 sideStep = new Vector2(-direction.y, direction.x) * arrowSize;
        // Backwards vector for the length
        Vector2 backStep = direction * arrowSize * 1.5f;

        Vector2 leftCorner = tip - backStep + sideStep;
        Vector2 rightCorner = tip - backStep - sideStep;

        vh.AddVert(tip, color, Vector2.zero);
        vh.AddVert(leftCorner, color, Vector2.zero);
        vh.AddVert(rightCorner, color, Vector2.zero);

        vh.AddTriangle(count, count + 1, count + 2);
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
        if (showArrow)
        {
            showArrow = false;
            arrowDirection = Vector2.zero;
            points.Clear();
        }

        if (points.Count == 0 || Vector2.Distance(points[points.Count - 1], point) > 2f)
        {
            points.Add(point);
            SetAllDirty();
        }
    }

    public void Clear()
    {
        showArrow = false;
        arrowDirection = Vector2.zero;
        points.Clear();
        SetAllDirty();
    }
}
