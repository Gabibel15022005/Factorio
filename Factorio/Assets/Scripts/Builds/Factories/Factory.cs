using UnityEngine;

public class Factory : CreatorBuilding
{
    [Header("Production Settings")]
    [SerializeField] private float produceInterval = 1f;
    private float produceTimer = 0f;

    void Update()
    {
        TickPullResources(Time.deltaTime);
        TickProduce(Time.deltaTime);
        TickTransfer(Time.deltaTime);
    }

    protected override void TickProduce(float deltaTime)
    {
        produceTimer += deltaTime;
        if (produceTimer < produceInterval) return;
        produceTimer = 0f;

        TryProduce();
    }

    public override void Refresh()
    {
        outputConveyors.Clear();
        foreach (CheckPos check in outputChecks)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(check.CheckArea.position, size, interactLayer);
            foreach (Collider2D collid in colliders)
            {
                if (collid.TryGetComponent(out Conveyor conveyor))
                {
                    conveyor.spawnPos = check.SpawnArea;
                    if (!outputConveyors.Contains(conveyor))
                        outputConveyors.Add(conveyor);
                }
            }
        }
    }

    public override void RefreshNeighbors()
    {
        base.RefreshNeighbors();
    }
}
