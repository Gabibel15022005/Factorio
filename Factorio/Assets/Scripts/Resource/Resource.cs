using UnityEngine;

[System.Serializable]
[RequireComponent(typeof(SpriteRenderer))]
public class Resource : MonoBehaviour
{
    public ResourceData scriptable;
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = scriptable.sprite;
    }
}

[CreateAssetMenu(menuName = "Resource/ResourceData")]
public class ResourceData : ScriptableObject
{
    public ResourceType resourceName;
    public GameObject resourcePrefab;
    public Sprite sprite;
}

public enum ResourceType  // CHANGE TYPE LATER
{
    Iron,
    Stone,
    Gold
}