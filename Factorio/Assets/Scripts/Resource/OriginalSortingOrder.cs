using UnityEngine;

[DisallowMultipleComponent]
public class OriginalSortingOrder : MonoBehaviour
{
    private int _value;
    public int originalSortingOrder => _value;

    private void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            _value = sr.sortingOrder;
    }
}