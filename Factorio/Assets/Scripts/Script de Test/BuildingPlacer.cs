using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingPlacer : MonoBehaviour
{
    [Header("Références")]
    public GridManager gridManager;
    public BuildingData buildingToPlace;
    public Camera cam;

    [Header("Ghost")]
    public Color validColor = new Color(0, 1, 0, 0.5f);
    public Color invalidColor = new Color(1, 0, 0, 0.5f);

    [Header("Placement continu")]
    public float placeInterval = 0.1f;

    private GameObject ghostInstance;
    private SpriteRenderer ghostRenderer;
    private Vector2Int lastGridPos = new Vector2Int(-999, -999);

    private bool isPlacing = false;
    private float placeTimer = 0f;

    private void Start()
    {
        if (cam == null) cam = Camera.main;
        if (!gridManager) Debug.LogError("GridManager non assigné !");
        if (!buildingToPlace) Debug.LogError("BuildingData non assigné !");

        // Création du ghost
        ghostInstance = new GameObject("Ghost");
        ghostRenderer = ghostInstance.AddComponent<SpriteRenderer>();
        ghostRenderer.sprite = buildingToPlace.ghostSprite;
        ghostRenderer.color = validColor;
        ghostRenderer.sortingOrder = 100;
        ghostInstance.SetActive(true);
    }

    private void Update()
    {
        UpdateGhost();

        if (isPlacing)
        {
            placeTimer += Time.deltaTime;
            if (placeTimer >= placeInterval)
            {
                TryPlaceBuilding();
                placeTimer = 0f;
            }
        }
        else
        {
            placeTimer = placeInterval; // permet placement immédiat au prochain clic
        }
    }

    public void OnPlaceBuilding(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
            isPlacing = true;
        else if (context.canceled)
            isPlacing = false;
    }

    void UpdateGhost()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, cam.nearClipPlane));
        worldPos.z = 0f;

        Vector2Int gridPos = WorldToGrid(worldPos);
        if (gridPos == lastGridPos) return;
        lastGridPos = gridPos;

        Vector3 ghostWorldPos = new Vector3(
            gridPos.x * gridManager.cellSize.x + (buildingToPlace.size.x * gridManager.cellSize.x) / 2f,
            gridPos.y * gridManager.cellSize.y + (buildingToPlace.size.y * gridManager.cellSize.y) / 2f,
            0f
        );
        ghostInstance.transform.position = ghostWorldPos;

        bool canBuild = gridManager.CanPlaceBuilding(gridPos, buildingToPlace);
        ghostRenderer.color = canBuild ? validColor : invalidColor;
    }

    void TryPlaceBuilding()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, cam.nearClipPlane));
        worldPos.z = 0f;
        Vector2Int gridPos = WorldToGrid(worldPos);

        if (gridManager.CanPlaceBuilding(gridPos, buildingToPlace))
        {
            gridManager.PlaceBuilding(buildingToPlace, gridPos);
        }
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / gridManager.cellSize.x);
        int y = Mathf.FloorToInt(worldPos.y / gridManager.cellSize.y);
        return new Vector2Int(x, y);
    }
}
