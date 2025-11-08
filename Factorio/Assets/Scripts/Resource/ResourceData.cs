using UnityEngine;

[CreateAssetMenu(menuName = "Resource/ResourceData")]
public class ResourceData : ScriptableObject
{
    public ResourceType resourceName;
    public GameObject resourcePrefab;
    public Sprite sprite;
}