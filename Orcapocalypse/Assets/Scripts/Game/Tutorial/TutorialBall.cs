using UnityEngine;

public class TutorialBall : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float slackAmount = 1.5f; // How much it droops
    public int resolution = 15; // How many segments make up the line

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = resolution;
    }

    void Update()
    {
        DrawRope();
    }

    void DrawRope()
    {
        // Find the middle point between start and end
        Vector3 midPoint = (startPoint.position + endPoint.position) / 2f;

        // Pull the middle point down based on slack
        // Optional: decrease slack as the points get further apart to simulate tension
        float distance = Vector3.Distance(startPoint.position, endPoint.position);
        midPoint.y -= slackAmount / (distance * 0.5f + 1f);

        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);
            Vector3 position = CalculateQuadraticBezierPoint(t, startPoint.position, midPoint, endPoint.position);
            lineRenderer.SetPosition(i, position);
        }
    }

    // Standard Bezier math function
    Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0; // (1-t)^2 * P0
        p += 2 * u * t * p1; // 2(1-t)t * P1
        p += tt * p2;        // t^2 * P2
        return p;
    }
}
