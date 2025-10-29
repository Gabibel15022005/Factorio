using System.Collections.Generic;
using UnityEngine;

public class Routeur : Conveyor
{
    private List<Conveyor> validNextConveyors = new List<Conveyor>();
    private int currentIndex = 0;

    public override void Refresh()
    {
        UpdateNextConveyors();
    }

    private void UpdateNextConveyors()
    {
        validNextConveyors.Clear();
        if (gridManagerRef == null) return;

        var convsNearby = GetAllValidConveyorNearby();

        foreach (var convDetected in convsNearby)
        {
            Conveyor conv = convDetected.Value;
            if (conv == null) continue;

            validNextConveyors.Add(conv);
        }

        nextConveyor = null;
    }

    protected override void MoveResources()
    {
        if (resources.Count == 0) return;

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
                    continue;
                }
                else
                {
                    r.transform.position = Center;
                    r.passedByCenter = true;
                }
            }

            if (validNextConveyors.Count > 0)
            {
                for (int attempt = 0; attempt < validNextConveyors.Count; attempt++)
                {
                    Conveyor target = validNextConveyors[currentIndex];
                    currentIndex = (currentIndex + 1) % validNextConveyors.Count;

                    if (target != null && target.resourcesCount < target.maxResources)
                    {
                        target.AddResource(r);
                        resources.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.magenta;
        if (validNextConveyors != null)
        {
            foreach (var conv in validNextConveyors)
            {
                if (conv != null)
                    Gizmos.DrawLine(transform.position, conv.transform.position);
            }
        }

        Gizmos.color = Color.yellow;
        foreach (var r in resources)
            Gizmos.DrawSphere(r.transform.position, 0.05f);
    }
#endif
}
