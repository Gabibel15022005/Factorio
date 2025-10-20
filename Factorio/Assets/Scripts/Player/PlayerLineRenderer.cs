using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlayerLineRenderer : MonoBehaviour
{
    [Header("Line Settings")]
    [SerializeField] private int pointCount = 10;
    [SerializeField] private float pointSpacing = 0.2f;
    [SerializeField] private float followSmoothness = 15f;
    [SerializeField] private float retractSpeed = 20f;

    [Header("References")]
    [SerializeField] private Player player;

    private LineRenderer lineRenderer;
    private Vector3[] points;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = pointCount;

        points = new Vector3[pointCount];
        for (int i = 0; i < pointCount; i++)
            points[i] = transform.position;
    }

    private void Update()
    {
        if (player == null) return;

        if (player.IsMoving)
            UpdateLine();
        else
            RetractLine();

        lineRenderer.SetPositions(points);
    }

    private void UpdateLine()
    {
        points[0] = transform.position;

        for (int i = 1; i < pointCount; i++)
        {
            Vector3 direction = points[i] - points[i - 1];
            float distance = direction.magnitude;

            if (distance > pointSpacing)
            {
                Vector3 targetPos = points[i - 1] + direction.normalized * pointSpacing;
                points[i] = Vector3.Lerp(points[i], targetPos, followSmoothness * Time.deltaTime);
            }
        }
    }

    private void RetractLine()
    {
        for (int i = 0; i < pointCount; i++)
        {
            points[i] = Vector3.Lerp(points[i], transform.position, retractSpeed * Time.deltaTime);
        }
    }

    public void SetPointCount(int newCount)
    {
        pointCount = Mathf.Max(2, newCount);
        points = new Vector3[pointCount];
        lineRenderer.positionCount = pointCount;

        for (int i = 0; i < pointCount; i++)
            points[i] = transform.position;
    }
}