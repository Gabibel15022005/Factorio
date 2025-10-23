using UnityEngine;

[System.Serializable]
[RequireComponent(typeof(SpriteRenderer))]
public class Resource : MonoBehaviour
{
    public ResourceData scriptable;
    public bool passedByCenter = false;
    [HideInInspector] public Conveyor CurrentConveyor { get; set; }
    public bool canBeTaken = true;
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = scriptable.sprite;
    }
}

public enum ResourceType  // CHANGE TYPE LATER
{
    Iron,
    Stone,
    Gold,
    Blood
}