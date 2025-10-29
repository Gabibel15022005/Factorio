using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public enum Direction
{
    Droite,
    Bas,
    Gauche,
    Haut,
    Any
}

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
    private bool isRemoving = false;
    private float placeTimer = 0f;
    private int rotation = 0;

    public static Action<BuildingData> ChangeBuildingToPlaceAction;

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
        HandlePlacingBuilding();
        HandleRemovingBuilding();
    }

    private void HandlePlacingBuilding()
    {
        if (isPlacing && !isRemoving)
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

    private void HandleRemovingBuilding()
    {
        if (isRemoving && !isPlacing)
        {
            placeTimer += Time.deltaTime;
            if (placeTimer >= placeInterval)
            {
                TryRemoveBuilding();
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
        if (context.started)
        {
            if (IsPointerOverUI())
            {
                isPlacing = false;
                return;
            }
            isPlacing = true;
        }
        else if (context.canceled)
        {
            isPlacing = false;
        }
    }

    public void OnRotateBuilding(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        //Debug.Log("Rotate building");

        // Rotation dans l'autre sens (horaire ou antihoraire selon ton choix)
        rotation = (rotation - 90 + 360) % 360;

        Vector2Int baseSize = buildingToPlace.size;
        Vector2Int rotatedSize = (rotation == 90 || rotation == 270)
            ? new Vector2Int(baseSize.y, baseSize.x)
            : baseSize;

        ghostInstance.transform.rotation = Quaternion.Euler(0, 0, rotation);

        lastGridPos = new Vector2Int(-999, -999);
        UpdateGhostWithSize(rotatedSize);
    }

    public void OnRemoveBuilding(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (IsPointerOverUI())
            {
                isRemoving = false;
                return;
            }
            isRemoving = true;
        }
        else if (context.canceled)
        {
            isRemoving = false;
        }


    }

    void UpdateGhostWithSize(Vector2Int customSize)
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, cam.nearClipPlane));
        worldPos.z = 0f;

        Vector2Int gridPos = WorldToGrid(worldPos);
        lastGridPos = gridPos;

        Vector3 ghostWorldPos = new Vector3(
            gridPos.x * gridManager.cellSize.x + (customSize.x * gridManager.cellSize.x) / 2f,
            gridPos.y * gridManager.cellSize.y + (customSize.y * gridManager.cellSize.y) / 2f,
            0f
        );
        ghostInstance.transform.position = ghostWorldPos;

        bool canBuild = gridManager.CanPlaceBuilding(gridPos, buildingToPlace);
        ghostRenderer.color = canBuild ? validColor : invalidColor;
    }

    void UpdateGhost()
    {
        // Récupère la position de la souris
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, cam.nearClipPlane));
        worldPos.z = 0f;

        // Convertit la position monde en coordonnées de grille
        Vector2Int gridPos = WorldToGrid(worldPos);

        // Si on n'a pas bougé de case, on sort (optimisation)
        if (gridPos == lastGridPos) return;
        lastGridPos = gridPos;

        // ✅ Taille effective selon la rotation, sans modifier le BuildingData
        Vector2Int baseSize = buildingToPlace.size;
        Vector2Int effectiveSize = (rotation == 90 || rotation == 270)
            ? new Vector2Int(baseSize.y, baseSize.x)
            : baseSize;

        // Calcul de la position centrée du ghost
        Vector3 ghostWorldPos = new Vector3(
            gridPos.x * gridManager.cellSize.x + (effectiveSize.x * gridManager.cellSize.x) / 2f,
            gridPos.y * gridManager.cellSize.y + (effectiveSize.y * gridManager.cellSize.y) / 2f,
            0f
        );

        ghostInstance.transform.position = ghostWorldPos;

        // Applique la rotation visuelle
        ghostInstance.transform.rotation = Quaternion.Euler(0, 0, rotation);

        // Vérifie si on peut construire à cette position
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
            Building building = gridManager.PlaceBuilding(buildingToPlace, gridPos, rotation);

            // 🧭 Définir la direction selon la rotation
            if (building != null && building.facingDirection != Direction.Any)
            {
                building.facingDirection = RotationToDirection(rotation);
            }
            else if (building != null && building.facingDirection == Direction.Any)
            {
                building.transform.rotation = new Quaternion(0,0,0,0);
            }
        }
    }

    Direction RotationToDirection(int rotation)
    {
        switch (rotation)
        {
            case 0: return Direction.Droite;
            case 90: return Direction.Haut;
            case 180: return Direction.Gauche;
            case 270: return Direction.Bas;
            default: return Direction.Droite;
        }
    }

    void TryRemoveBuilding()
    {
        // Récupère la position de la souris
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, cam.nearClipPlane));
        worldPos.z = 0f;
        Vector2Int gridPos = WorldToGrid(worldPos);

        // Vérifie s’il y a un bâtiment à cette case
        Building building = gridManager.GetBuildingAt(gridPos);
        if (building != null)
        {

            //Debug.Log($"Remove {building.gameObject.name}");
            gridManager.RemoveBuilding(building); // méthode à créer dans GridManager
        }
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / gridManager.cellSize.x);
        int y = Mathf.FloorToInt(worldPos.y / gridManager.cellSize.y);
        return new Vector2Int(x, y);
    }

    private bool IsPointerOverUI()
    {
        // On crée un event data basé sur la position du curseur
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Mouse.current.position.ReadValue();

        // On fait un raycast UI
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // Si on touche au moins un élément UI -> true
        return results.Count > 0;
    }

    void ChangeBuildingToPlace(BuildingData data)
    {
        buildingToPlace = data;
    }

    void OnEnable()
    {
        ChangeBuildingToPlaceAction += ChangeBuildingToPlace;
    }

    void OnDisable()
    {
        ChangeBuildingToPlaceAction -= ChangeBuildingToPlace;
    }

}
