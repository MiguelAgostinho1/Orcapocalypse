using UnityEngine;

public class TutorialBall : MonoBehaviour
{
    [Tooltip("Drag the child 'RopeAttachPoint' object here!")]
    [SerializeField] private Transform _ropeAttachPoint;

    [Tooltip("Drag the Ceiling Anchor object here!")]
    [SerializeField] private Transform _connectedAnchorTransform;

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();

        _lineRenderer.positionCount = 2;
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;
        _lineRenderer.useWorldSpace = true;
    }

    private void Update()
    {
        // Safety check to prevent errors if you forgot to assign the slots
        if (_connectedAnchorTransform == null || _ropeAttachPoint == null) return;

        // Draw line from the static ceiling anchor to the top of the ball
        _lineRenderer.SetPosition(0, _connectedAnchorTransform.position);
        _lineRenderer.SetPosition(1, _ropeAttachPoint.position);
    }
}
