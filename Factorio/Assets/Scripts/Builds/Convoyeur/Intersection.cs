using System.Collections.Generic;
using UnityEngine;

public class Intersection : Conveyor
{
    private List<Resource> horizontalResources = new();
    private List<Resource> verticalResources = new();

    // Retourne le total des ressources dans les deux axes
    public override int resourcesCount
    {
        get
        {
            return horizontalResources.Count + verticalResources.Count;
        }
    }
    protected override void Update()
    {
        if (!needsUpdate) return;
        MoveResources();

        if (horizontalResources.Count == 0 && verticalResources.Count == 0)
            needsUpdate = false;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach (Resource resource in horizontalResources)
        {
            if (resource != null)
                Destroy(resource.gameObject);
        }
        foreach (Resource resource in verticalResources)
        {
            if (resource != null)
                Destroy(resource.gameObject);
        }
        
    }

    public override bool AddResource(Resource resource)
    {
        if (resource == null) return false;

        Vector2Int currentPos = gridManagerRef.GetGridPosition(transform.position);
        Vector2Int fromPos = gridManagerRef.GetGridPosition(resource.transform.position);
        Vector2Int dirVec = fromPos - currentPos;

        bool isHorizontal = Mathf.Abs(dirVec.x) > Mathf.Abs(dirVec.y);

        if (isHorizontal && horizontalResources.Count >= maxResources) return false;
        if (!isHorizontal && verticalResources.Count >= maxResources) return false;

        resource.CurrentConveyor = this;
        resource.passedByCenter = false;

        if (isHorizontal)
            horizontalResources.Add(resource);
        else
            verticalResources.Add(resource);

        needsUpdate = true;
        return true;
    }

    public override void RemoveResource(Resource resource)
    {
        if (resource == null) return;

        if (horizontalResources.Contains(resource))
            horizontalResources.Remove(resource);
        else if (verticalResources.Contains(resource))
            verticalResources.Remove(resource);

        resource.CurrentConveyor = null;

        if (horizontalResources.Count == 0 && verticalResources.Count == 0)
            needsUpdate = false;
    }


    protected override void MoveResources()
    {
        MoveAxisResources(horizontalResources, true);
        MoveAxisResources(verticalResources, false);
    }

    private void MoveAxisResources(List<Resource> list, bool isHorizontal)
    {
        if (list.Count == 0) return;

        for (int i = 0; i < list.Count; i++)
        {
            Resource r = list[i];
            if (r == null) { RemoveResource(r); i--; continue; }

            if (!r.passedByCenter)
            {
                float distToCenter = Vector3.Distance(r.transform.position, transform.position);
                if (distToCenter > centerRange)
                {
                    r.transform.position = Vector3.MoveTowards(
                        r.transform.position,
                        transform.position,
                        speed * Time.deltaTime
                    );
                    continue;
                }
                else
                {
                    r.transform.position = transform.position;
                    r.passedByCenter = true;
                }
            }

            var conveyors = GetAllValidConveyorNearby();
            bool moved = false;

            foreach (var kvp in conveyors)
            {
                Direction dir = kvp.Key;
                Conveyor target = kvp.Value;

                if (isHorizontal && (dir != Direction.Gauche && dir != Direction.Droite))
                    continue;
                if (!isHorizontal && (dir != Direction.Haut && dir != Direction.Bas))
                    continue;

                if (target == null) continue;

                Vector2Int currentPos = gridManagerRef.GetGridPosition(transform.position);
                Vector2Int targetPos = gridManagerRef.GetGridPosition(target.transform.position);
                Vector2Int dirVec = targetPos - currentPos;
                Direction dirToTarget = gridManagerRef.VectorToDirection(dirVec);

                if (!target.IsOpposingDirection(Direction.Any, dirToTarget))
                {
                    // On tente directement d'ajouter : AddResource renvoie true si accepté
                    bool accepted = target.AddResource(r);
                    if (accepted)
                    {
                        RemoveResource(r);
                        i--;
                        moved = true;
                        break;
                    }
                    else
                    {
                        // target a refusé (axe plein, ou autre contrainte) — on essaye la suite
                        continue;
                    }
                }
            }

            if (!moved)
                r.transform.position = transform.position;
        }
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.red;
        foreach (var r in horizontalResources)
            if (r != null) Gizmos.DrawSphere(r.transform.position, 0.05f);

        Gizmos.color = Color.blue;
        foreach (var r in verticalResources)
            if (r != null) Gizmos.DrawSphere(r.transform.position, 0.05f);
    }
#endif
}
