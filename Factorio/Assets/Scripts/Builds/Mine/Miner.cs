using UnityEngine;

public class Miner : CreatorBuilding
{
    public ResourceData resourceType;
    public float mineInterval = 1f;
    public int amountPerMine = 1;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= mineInterval)
        {
            timer = 0f;
            Mine();
        }
    }

    void Mine()
    {
        Resource resource = SpawnResource(resourceType, transform.position);
        Debug.Log($"Mined {amountPerMine} {resourceType}");
    }
}