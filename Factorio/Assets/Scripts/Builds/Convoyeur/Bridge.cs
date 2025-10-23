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
    private Bridge linkedBridge;     // Le bridge lié si on en a un
    private Bridge detectedBridge;   // Bridge détecté (pour gizmo)

    void Awake()
    {
        currentState = BridgeState.Research;
    }

    protected override void Update()
    {
        // Déplacement des ressources
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
                    nextConveyor = linkedBridge; // Force le lien pour MoveResources
                break;

            case BridgeState.Conveyor:
                if (nextConveyor == null)
                    TryFindNextConveyor();
                break;
        }
    }

    /// <summary>
    /// Recherche un autre Bridge valide en ligne droite, en sautant les Convery invalide
    /// </summary>
    private void TryFindBridge()
    {
        if (gridManagerRef == null)
            return;

        detectedBridge = null;
        nextConveyor = null;
        linkedBridge = null;

        Vector2Int currentPos = gridManagerRef.GetGridPosition(transform.position);
        Vector2Int forwardOffset = GetForwardOffset();
        Vector2Int checkPos = currentPos + forwardOffset;

        for (float d = 1; d <= maxBridgeDistance; d++)
        {
            Building neighbor = gridManagerRef.GetBuildingAt(checkPos);
            if (neighbor != null)
            {
                // On ne se connecte que si c'est un Bridge
                if (neighbor is Bridge neighborBridge)
                {
                    if (neighborBridge.IsOpposingDirection(facingDirection))
                        break;

                    // Bridge valide trouvé
                    detectedBridge = neighborBridge;
                    linkedBridge = neighborBridge;
                    nextConveyor = neighborBridge;

                    currentState = BridgeState.Bridge;

                    // Informe l'autre bridge qu'il devient Conveyor
                    neighborBridge.OnLinkedByBridge(this);
                    break;
                }

                // Si c'est autre chose qu'un Bridge, on ignore et continue
            }

            checkPos += forwardOffset; // passe à la prochaine case
        }
    }

    /// <summary>
    /// Appelé lorsqu'un autre bridge nous détecte
    /// </summary>
    public void OnLinkedByBridge(Bridge sourceBridge)
    {
        if (IsOpposingDirection(sourceBridge.facingDirection))
            return;

        linkedBridge = sourceBridge;
        currentState = BridgeState.Conveyor;

        TryFindNextConveyor();
    }

    /// <summary>
    /// Cherche un conveyor valide devant le bridge
    /// </summary>
    private void TryFindNextConveyor()
    {
        Vector2Int currentPos = gridManagerRef.GetGridPosition(transform.position);
        Vector2Int forwardOffset = GetForwardOffset();
        Vector2Int nextPos = currentPos + forwardOffset;

        Building neighbor = gridManagerRef.GetBuildingAt(nextPos);
        Conveyor nextConv = neighbor as Conveyor;

        if (nextConv != null && !nextConv.IsOpposingDirection(facingDirection))
            nextConveyor = nextConv;
        else
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
