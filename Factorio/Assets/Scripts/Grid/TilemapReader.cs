using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapReader : MonoBehaviour
{
    public Tilemap tilemap; // ta Tilemap Unity
    public GridManager gridManager;

    // Associer chaque TileBase à un TileType
    [System.Serializable]
    public struct TileToType
    {
        public TileBase[] tile;
        public TileType type;
    }
    public TileToType[] tilesMapping;

    private void Awake()
    {
        if (gridManager == null || tilemap == null) return;

        gridManager.mapTiles = new TileType[gridManager.gridWidth, gridManager.gridHeight];

        // Parcourt chaque cellule
        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                TileBase TileCell = tilemap.GetTile(cellPos);

                // Par défaut Normal
                TileType type = TileType.Normal;

                // Cherche dans le mapping
                foreach (var map in tilesMapping)
                {
                    bool breakForEach = false;

                    foreach (var tile in map.tile)
                    {
                        if (tile == TileCell)
                        {
                            type = map.type;
                            breakForEach = true;
                            break;
                        }
                    }

                    if (breakForEach) break;
                }

                gridManager.mapTiles[x, y] = type;
            }
        }
    }
}
