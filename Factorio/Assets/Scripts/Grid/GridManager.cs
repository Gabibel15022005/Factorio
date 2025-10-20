using UnityEngine;
using System.Collections.Generic;

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
    public TileType[,] mapTiles; // initialisé via Tilemap ou manuel
    public TileType defaultTileType = TileType.Normal;

    [Header("Gizmos")]
    public Color freeCellColor = Color.green;
    public Color occupiedCellColor = Color.red;
    public Color forbiddenCellColor = Color.blue;

    private Dictionary<Vector2Int, Building> grid = new();

    private void Awake()
    {
        // Initialisation de la map si nécessaire
        if (mapTiles == null || mapTiles.Length == 0)
        {
            mapTiles = new TileType[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++)
                for (int y = 0; y < gridHeight; y++)
                    mapTiles[x, y] = defaultTileType;
        }
    }

    // Vérifie si le bâtiment peut être placé à la position donnée
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

                // 1️⃣ Case déjà occupée
                if (grid.ContainsKey(pos)) return false;

                // 2️⃣ Vérification du type de tile
                TileType tile = mapTiles[pos.x, pos.y];
                if (!IsTileAllowed(tile, data.allowedTiles))
                    return false;
            }
        }
        return true;
    }

    // Vérifie si la tile est autorisée pour ce bâtiment
    private bool IsTileAllowed(TileType tile, TileType[] allowedTiles)
    {
        if (allowedTiles == null || allowedTiles.Length == 0) return false;
        if (System.Array.Exists(allowedTiles, t => t == TileType.Any)) return true;
        return System.Array.Exists(allowedTiles, t => t == tile);
    }

    // Place le bâtiment centré sur la grille
    public void PlaceBuilding(BuildingData data, Vector2Int position)
    {
        if (!CanPlaceBuilding(position, data))
            return;

        GameObject obj = Instantiate(data.prefab);
        Building building = obj.GetComponent<Building>();
        if (!building)
        {
            Debug.LogError("Le prefab doit contenir un composant Building !");
            Destroy(obj);
            return;
        }

        // ✅ Placement centré
        Vector3 centeredPos = new Vector3(
            position.x * cellSize.x + (data.size.x * cellSize.x) / 2f,
            position.y * cellSize.y + (data.size.y * cellSize.y) / 2f,
            0f
        );
        building.transform.position = centeredPos;

        // Marque les cellules comme occupées
        for (int x = 0; x < data.size.x; x++)
        {
            for (int y = 0; y < data.size.y; y++)
            {
                Vector2Int pos = position + new Vector2Int(x, y);
                grid[pos] = building;
            }
        }
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

                Color color = freeCellColor;

                if (grid.ContainsKey(pos))
                    color = occupiedCellColor;
                else if (mapTiles != null && mapTiles.Length > 0 && mapTiles[x, y] != TileType.Normal)
                    color = forbiddenCellColor;

                Gizmos.color = color;
                Gizmos.DrawWireCube(cellPos, new Vector3(cellSize.x, cellSize.y, 0.1f));
            }
        }
    }
}