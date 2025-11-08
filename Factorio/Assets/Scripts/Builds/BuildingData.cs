using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Building/BuildingData")]
public class BuildingData : ScriptableObject
{
    [SerializeField] string buildingName;
    public string BuildingName => buildingName;
    public GameObject prefab;
    public Sprite iconSprite;
    public Sprite ghostSprite; // sprite to show before construct
    public Vector2Int size = Vector2Int.one; // width x height [Header("Placement")]
    public TileType[] allowedTiles = new TileType[] { TileType.Normal }; 
    public List<RessourceCount> cost;
    public bool canBeRefound = true;

}