using UnityEngine;

public class HUB : Conveyor
{
    protected override void MoveResources()
    {
        for (int i = 0; i < resources.Count; i++)
        {
            Resource r = resources[i];

            if (!r.passedByCenter)
            {
                float distToCenter = Vector3.Distance(r.transform.position, Center);
                if (distToCenter > centerRange)
                {
                    r.transform.position = Vector3.MoveTowards(
                        r.transform.position,
                        Center,
                        speed * Time.deltaTime
                    );
                }
                else
                {
                    AddToInventory(r);
                    
                    Destroy(r.gameObject);
                    resources.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    private void AddToInventory(Resource r)
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager not found in scene!");
            return;
        }

        InventoryManager.Instance.AddResource(r.scriptable);
    }


    public override void Refresh()
    {
        nextConveyor = null;
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.red;
        foreach (var r in resources)
        {
            Gizmos.DrawSphere(r.transform.position, 0.05f);
        }
    }
#endif
    
}
