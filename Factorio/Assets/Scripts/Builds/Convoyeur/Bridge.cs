using UnityEngine;

public class Bridge : Conveyor
{
    public enum BridgeState
    {
        Research, // Cherche un autre bridge
        Bridge,   // Sert de lien entre deux bridges
        Conveyor  // Se comporte comme un conveyor classique
    }

    [Header("Bridge Settings")]
    [Tooltip("Distance maximale en unités monde pour détecter un autre bridge")]
    public float maxBridgeDistance = 5f;

    [Header("Gizmo Colors")]
    [SerializeField] private Color rangeGizmoColor = Color.green;
    [SerializeField] private Color connectedGizmoColor = Color.cyan;
    [SerializeField] private Color detectedBridgeColor = Color.red;

    [Header("Debug")]
    [SerializeField] private BridgeState currentState = BridgeState.Research;
    private Bridge linkedBridge;
    private Bridge detectedBridge;

    void Awake()
    {
        currentState = BridgeState.Research;
    }

    protected override void Update()
    {
        base.Update();

        switch (currentState)
        {
            case BridgeState.Research:
                TryFindBridge();
                break;

            case BridgeState.Bridge:
                if (linkedBridge == null)
                    ResetBridgeConnection();
                else
                    nextConveyor = linkedBridge;
                break;

            case BridgeState.Conveyor:
                if (nextConveyor == null)
                    TryFindNextConveyor();
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
                // Calcule la direction vers le bridge
                Vector2Int dirToNeighbor = checkPos - currentPos;
                Direction dirToBridge = gridManagerRef.VectorToDirection(dirToNeighbor);

                // Vérifie s’il est orienté vers ce pont (donc opposé)
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

        // Continue plus loin sur la même ligne
        checkPos += forwardOffset;
    }
}

public void OnLinkedByBridge(Bridge sourceBridge)
{
    // Calcule la direction depuis CE bridge vers l’autre
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
        // Direction vers le voisin
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
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (linkedBridge != null)
            linkedBridge.ResetBridgeConnection();
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // Ne pas afficher les gizmos de recherche si on est Conveyor
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

        // Portée de recherche
        Gizmos.color = rangeGizmoColor;
        Gizmos.DrawLine(start, start + dir * maxBridgeDistance);

        // Bridge détecté
        if (detectedBridge != null)
        {
            Gizmos.color = detectedBridgeColor;
            Vector3 midpoint = (transform.position + detectedBridge.transform.position) / 2f;
            Gizmos.DrawLine(transform.position, detectedBridge.transform.position);
            Gizmos.DrawSphere(midpoint, 0.1f);
        }

        // Connexion active
        if (nextConveyor != null)
        {
            Gizmos.color = connectedGizmoColor;
            Gizmos.DrawLine(transform.position, nextConveyor.transform.position);
        }
    }
#endif
}
