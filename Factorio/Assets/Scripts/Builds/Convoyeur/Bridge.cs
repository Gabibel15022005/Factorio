using UnityEngine;

public class Bridge : Conveyor
{
    public enum BridgeState
    {
        Research,
        Bridge,
        Conveyor
    }

    [Header("Bridge Settings")]
    [Tooltip("Distance maximale en unités monde pour détecter un autre bridge")]
    public float maxBridgeDistance = 5f;

    [Header("Visual Settings")]
    [Tooltip("LineRenderer utilisé pour visualiser le pont")]
    public LineRenderer bridgeLineRenderer;

    [Header("Gizmo Colors")]
    [SerializeField] private Color rangeGizmoColor = Color.green;
    [SerializeField] private Color connectedGizmoColor = Color.cyan;
    [SerializeField] private Color detectedBridgeColor = Color.red;
    
    [Header("Bridge Visual Settings")]
    [Tooltip("Order in layer temporaire appliqué aux ressources pendant la traversée du pont")]
    public int bridgeSortingOrder = 10;

    [Header("Debug")]
    [SerializeField] private BridgeState currentState = BridgeState.Research;
    private Bridge linkedBridge;
    private Bridge detectedBridge;
    

    void Awake()
    {
        currentState = BridgeState.Research;

        // S'assurer que le LineRenderer est désactivé au départ
        if (bridgeLineRenderer != null)
            bridgeLineRenderer.enabled = false;
    }

    protected override void Update()
    {
        base.Update();

        switch (currentState)
        {
            case BridgeState.Research:
                TryFindBridge();
                UpdateBridgeLine(false);
                break;

            case BridgeState.Bridge:
                if (linkedBridge == null)
                {
                    ResetBridgeConnection();
                    UpdateBridgeLine(false);
                }
                else
                {
                    nextConveyor = linkedBridge;
                    UpdateBridgeLine(true);
                }
                break;

            case BridgeState.Conveyor:
                if (nextConveyor == null)
                    TryFindNextConveyor();

                UpdateBridgeLine(false);
                break;
        }
    }

    private void TryFindBridge()
    {
        if (gridManagerRef == null)
            return;

        detectedBridge = null;
        nextConveyor = null;
        linkedBridge = null;

        Vector2Int currentPos = gridManagerRef.GetGridPosition(transform.position);
        Vector2Int forwardOffset = GetForwardOffset(facingDirection);
        Vector2Int checkPos = currentPos + forwardOffset;

        for (float d = 1; d <= maxBridgeDistance; d++)
        {
            Building neighbor = gridManagerRef.GetBuildingAt(checkPos);
            if (neighbor != null)
            {
                if (neighbor is Bridge neighborBridge)
                {
                    Vector2Int dirToNeighbor = checkPos - currentPos;
                    Direction dirToBridge = gridManagerRef.VectorToDirection(dirToNeighbor);

                    if (neighborBridge.IsOpposingDirection(facingDirection, dirToBridge))
                        break;

                    detectedBridge = neighborBridge;
                    linkedBridge = neighborBridge;
                    nextConveyor = neighborBridge;

                    currentState = BridgeState.Bridge;

                    neighborBridge.OnLinkedByBridge(this);
                    break;
                }
            }

            checkPos += forwardOffset;
        }
    }

    public void OnLinkedByBridge(Bridge sourceBridge)
    {
        Vector2Int currentPos = gridManagerRef.GetGridPosition(transform.position);
        Vector2Int sourcePos = gridManagerRef.GetGridPosition(sourceBridge.transform.position);
        Vector2Int dirToSource = sourcePos - currentPos;
        Direction dirToBridge = gridManagerRef.VectorToDirection(dirToSource);

        if (IsOpposingDirection(sourceBridge.facingDirection, dirToBridge))
            return;

        linkedBridge = sourceBridge;
        currentState = BridgeState.Conveyor;

        TryFindNextConveyor();
    }

    private void TryFindNextConveyor()
    {
        Vector2Int currentPos = gridManagerRef.GetGridPosition(transform.position);
        Vector2Int forwardOffset = GetForwardOffset(facingDirection);
        Vector2Int nextPos = currentPos + forwardOffset;

        Building neighbor = gridManagerRef.GetBuildingAt(nextPos);
        Conveyor nextConv = neighbor as Conveyor;

        if (nextConv != null)
        {
            Vector2Int dirToNeighbor = nextPos - currentPos;
            Direction dirToNext = gridManagerRef.VectorToDirection(dirToNeighbor);

            if (!nextConv.IsOpposingDirection(facingDirection, dirToNext))
            {
                nextConveyor = nextConv;
                return;
            }
        }

        nextConveyor = null;
    }

    private void ResetBridgeConnection()
    {
        linkedBridge = null;
        nextConveyor = null;
        detectedBridge = null;
        currentState = BridgeState.Research;
        UpdateBridgeLine(false);
    }

    public override bool AddResource(Resource resource)
    {
        if (resources.Count >= maxResources)
            return false;
        
        OnResourceExitBridge(resource.gameObject);
        resource.canBeTaken = canRessourceBeTaken;
        resource.CurrentConveyor = this;
        resource.passedByCenter = false;
        OnResourceEnterBridge(resource.gameObject);
        resources.Add(resource);

        needsUpdate = true;
        return true;
    }

    /// <summary>
    /// Appelé quand une ressource commence à traverser le pont.
    /// </summary>
    public void OnResourceEnterBridge(GameObject resource)
    {
        var sr = resource.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.sortingOrder = bridgeSortingOrder;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (linkedBridge != null)
            linkedBridge.ResetBridgeConnection();

        UpdateBridgeLine(false);
    }

    private void UpdateBridgeLine(bool active)
    {
        if (bridgeLineRenderer == null)
            return;

        if (active && linkedBridge != null)
        {
            bridgeLineRenderer.enabled = true;
            bridgeLineRenderer.positionCount = 2;
            bridgeLineRenderer.SetPosition(0, transform.position);
            bridgeLineRenderer.SetPosition(1, linkedBridge.transform.position);
        }
        else
        {
            bridgeLineRenderer.enabled = false;
        }
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (currentState == BridgeState.Conveyor)
        {
            if (nextConveyor != null)
            {
                Gizmos.color = connectedGizmoColor;
                Gizmos.DrawLine(transform.position, nextConveyor.transform.position);
            }
            return;
        }

        Vector3 start = transform.position;
        Vector3 dir = Vector3.zero;
        int rot = Mathf.RoundToInt(transform.eulerAngles.z) % 360;
        switch (rot)
        {
            case 0: dir = Vector3.right; break;
            case 90: dir = Vector3.up; break;
            case 180: dir = Vector3.left; break;
            case 270: dir = Vector3.down; break;
        }

        Gizmos.color = rangeGizmoColor;
        Gizmos.DrawLine(start, start + dir * maxBridgeDistance);

        if (detectedBridge != null)
        {
            Gizmos.color = detectedBridgeColor;
            Vector3 midpoint = (transform.position + detectedBridge.transform.position) / 2f;
            Gizmos.DrawLine(transform.position, detectedBridge.transform.position);
            Gizmos.DrawSphere(midpoint, 0.1f);
        }

        if (nextConveyor != null)
        {
            Gizmos.color = connectedGizmoColor;
            Gizmos.DrawLine(transform.position, nextConveyor.transform.position);
        }
    }
#endif
}
