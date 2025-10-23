using UnityEngine;

public class Miner : CreatorBuilding
{
    [Header("Mining Settings")]
    public float mineInterval = 1f;
    private float timer = 0f;

    void Update()
    {
        // Tick logique : permet d’étendre facilement à d’autres créateurs plus complexes
        TickProduce(Time.deltaTime);
        TickTransfer(Time.deltaTime);
    }

    protected override void TickProduce(float deltaTime)
    {
        timer += deltaTime;

        if (timer >= mineInterval)
        {
            timer = 0f;
            TryProduce();
        }
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
