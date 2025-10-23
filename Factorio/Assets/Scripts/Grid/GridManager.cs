using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum TileType
{
    Any,
    Normal,
    Water,
    Mountain,
    Sand
}

public class GridManager : MonoBehaviour
{
    [Header("Grille")]
    public Vector2 cellSize = Vector2.one;
    public int gridWidth = 20;
    public int gridHeight = 15;

    [Header("Tiles map")]
    public TileType[,] mapTiles;
    public TileType defaultTileType = TileType.Normal;

    [Header("Gizmos")]
    public Color freeCellColor = Color.green;
    public bool showFreeCellColor = true;
    public Color occupiedCellColor = Color.red;
    public bool showOccupiedCellColor = true;
    public Color forbiddenCellColor = Color.blue;
    public bool showForbiddenCellColorr = true;


    private Dictionary<Vector2Int, Building> grid = new();

    private void Awake()
    {
        if (mapTiles == null || mapTiles.Length == 0)
        {
            mapTiles = new TileType[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++)
                for (int y = 0; y < gridHeight; y++)
                    mapTiles[x, y] = defaultTileType;
        }
    }

    public bool CanPlaceBuilding(Vector2Int position, BuildingData data)
    {
        Vector2Int size = data.size;

        if (position.x < 0 || position.y < 0 || position.x + size.x > gridWidth || position.y + size.y > gridHeight)
            return false;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int pos = position + new Vector2Int(x, y);

                if (grid.ContainsKey(pos)) return false;

                TileType tile = mapTiles[pos.x, pos.y];
                if (!IsTileAllowed(tile, data.allowedTiles))
                    return false;
            }
        }
        return true;
    }

    private bool IsTileAllowed(TileType tile, TileType[] allowedTiles)
    {
        if (allowedTiles == null || allowedTiles.Length == 0) return false;
        if (System.Array.Exists(allowedTiles, t => t == TileType.Any)) return true;
        return System.Array.Exists(allowedTiles, t => t == tile);
    }

    public Building PlaceBuilding(BuildingData data, Vector2Int position, int rotation)
    {
        if (!CanPlaceBuilding(position, data))
            return null;

        GameObject obj = Instantiate(data.prefab);
        Building building = obj.GetComponent<Building>();
        if (!building)
        {
            Debug.LogError("Le prefab doit contenir un composant Building !");
            Destroy(obj);
            return null;
        }

        // Taille effective selon rotation
        Vector2Int effectiveSize = (rotation == 90 || rotation == 270)
            ? new Vector2Int(data.size.y, data.size.x)
            : data.size;

        building.SetBuildingSize(effectiveSize);
        building.SetGridManagerRef(this);

        // Positionnement centré
        Vector3 centeredPos = new Vector3(
            position.x * cellSize.x + effectiveSize.x * cellSize.x / 2f,
            position.y * cellSize.y + effectiveSize.y * cellSize.y / 2f,
            0f
        );

        building.transform.position = centeredPos;
        building.transform.rotation = Quaternion.Euler(0, 0, rotation);

        // Marque les cellules occupées
        for (int x = 0; x < effectiveSize.x; x++)
        {
            for (int y = 0; y < effectiveSize.y; y++)
            {
                Vector2Int pos = position + new Vector2Int(x, y);
                grid[pos] = building;
            }
        }


        building.StartCoroutine(DelayedRefresh(building));

        return building;
    }
    
    

    private IEnumerator DelayedRefresh(Building building)
    {
        yield return new WaitForSeconds(0.05f);
        building.RefreshNeighbors();
    }



    public Vector2Int GetGridPosition(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / cellSize.x);
        int y = Mathf.FloorToInt(worldPos.y / cellSize.y);

        // Clamp pour rester dans la grille
        x = Mathf.Clamp(x, 0, gridWidth - 1);
        y = Mathf.Clamp(y, 0, gridHeight - 1);

        return new Vector2Int(x, y);
    }

    public void RemoveBuilding(Building building, Vector2Int position, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int pos = position + new Vector2Int(x, y);
                if (grid.ContainsKey(pos)) grid.Remove(pos);
            }
        }
        Destroy(building.gameObject);
    }

    public Building GetBuildingAt(Vector2Int pos)
    {
        if (!IsInsideGrid(pos)) return null;
        grid.TryGetValue(pos, out Building building);
        return building;
    }

    public bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < gridWidth && pos.y < gridHeight;
    }

    public void RemoveBuilding(Building building)
    {
        if (building == null) return;

        List<Vector2Int> cellsToRemove = new List<Vector2Int>();

        foreach (var kvp in grid)
        {
            if (kvp.Value == building)
                cellsToRemove.Add(kvp.Key);
        }

        foreach (var pos in cellsToRemove)
        {
            grid.Remove(pos);
        }

        Destroy(building.gameObject);
}

    private void OnDrawGizmos()
    {
        if (gridWidth <= 0 || gridHeight <= 0) return;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Vector3 cellPos = new Vector3(
                    x * cellSize.x + cellSize.x / 2f,
                    y * cellSize.y + cellSize.y / 2f,
                    0
                );

                bool isOccupied = grid.ContainsKey(pos);
                bool isForbidden = mapTiles != null && mapTiles.Length > 0 && mapTiles[x, y] != TileType.Normal;

                if (isOccupied && showOccupiedCellColor)
                {
                    Gizmos.color = occupiedCellColor;
                    Gizmos.DrawWireCube(cellPos, new Vector3(cellSize.x, cellSize.y, 0.1f));
                }
                else if (isForbidden && showForbiddenCellColorr)
                {
                    Gizmos.color = forbiddenCellColor;
                    Gizmos.DrawWireCube(cellPos, new Vector3(cellSize.x, cellSize.y, 0.1f));
                }
                else if (!isOccupied && !isForbidden && showFreeCellColor)
                {
                    Gizmos.color = freeCellColor;
                    Gizmos.DrawWireCube(cellPos, new Vector3(cellSize.x, cellSize.y, 0.1f));
                }
            }
        }
    }
}