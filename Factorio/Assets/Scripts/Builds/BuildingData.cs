using UnityEngine;

[CreateAssetMenu(menuName = "Building/BuildingData")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public GameObject prefab;
    public Sprite iconSprite;
    public Sprite ghostSprite; // sprite to show before construct
    public Vector2Int size = Vector2Int.one; // width x height [Header("Placement")]
    public TileType[] allowedTiles = new TileType[] { TileType.Normal }; 

}