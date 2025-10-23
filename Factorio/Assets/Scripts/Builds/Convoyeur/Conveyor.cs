using UnityEngine;
using System.Collections.Generic;

public class Conveyor : Building
{
    [Header("Conveyor Settings")]
    public float speed = 2f; // Vitesse de déplacement des ressources
    protected List<Resource> resources = new();
    public int resourcesCount => resources.Count;

    [Header("Resource Management")]
    public int maxResources = 3; // Limite max de ressources sur ce convoyeur
    public float minDistanceBetweenResources = 0.5f; // Distance minimale entre ressources
    public bool canRessourceBeTaken = true;

    [Header("Connections")]
    public Conveyor nextConveyor;
    public Transform spawnPos;
    [SerializeField] protected const float centerRange = 0.05f;
    [SerializeField] protected const float nextConvRange = 0.75f;

    [Header("Gizmos")]
    public Color gizmoColor = Color.cyan;

    protected Vector3 Center => transform.position;

    protected virtual void OnDestroy()
    {
        foreach (Resource resource in resources)
        {
            if (resource != null)
                Destroy(resource.gameObject);
        }
    }

    protected virtual void Update()
    {
        MoveResources();
    }


    protected virtual void MoveResources()
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
                    continue;
                }
                else
                {
                    r.transform.position = Center;
                    r.passedByCenter = true;
                }
            }

            if (nextConveyor != null)
            {
                if (nextConveyor.resourcesCount >= nextConveyor.maxResources)
                    continue;

                Vector3 target = nextConveyor.transform.position;
                float distToNext = Vector3.Distance(r.transform.position, target);

                if (i > 0)
                {
                    Vector3 previousPos = resources[i - 1].transform.position;
                    float distToPrevious = Vector3.Distance(r.transform.position, previousPos);

                    if (distToPrevious < minDistanceBetweenResources)
                    {
                        // Ralentit légèrement pour garder un espacement fluide
                        r.transform.position = Vector3.MoveTowards(
                            r.transform.position,
                            target,
                            (speed * 0.25f) * Time.deltaTime
                        );
                        continue;
                    }
                }

                if (distToNext > nextConvRange)
                {
                    r.transform.position = Vector3.MoveTowards(
                        r.transform.position,
                        target,
                        speed * Time.deltaTime
                    );
                }
                else
                {
                    nextConveyor.AddResource(r);
                    resources.RemoveAt(i);
                    i--;
                }
            }
            else
            {
                r.transform.position = Center;
            }
        }
    }

    public virtual void AddResource(Resource resource)
    {
        if (resources.Count >= maxResources)
            return;

        resource.canBeTaken = canRessourceBeTaken;
        resource.CurrentConveyor = this;
        resource.passedByCenter = false;
        resources.Add(resource);
    }

    public virtual void RemoveResource(Resource resource)
    {
        Debug.Log($"RemoveResource{resource.gameObject.name}");
        resources.Remove(resource);
        resource.CurrentConveyor = null;
    }
    
    protected virtual void UpdateNextConveyor()
    {
        if (gridManagerRef == null) return;

        Vector2Int currentPos = gridManagerRef.GetGridPosition(transform.position);
        Vector2Int forwardOffset = GetForwardOffset();
        Vector2Int nextPos = currentPos + forwardOffset;

        Building neighbor = gridManagerRef.GetBuildingAt(nextPos);
        Conveyor neighborConveyor = neighbor as Conveyor;

        if (neighborConveyor != null)
        {
            if (!neighborConveyor.IsOpposingDirection(facingDirection))
            {
                nextConveyor = neighborConveyor;

                foreach (var r in resources)
                {
                    if (r.passedByCenter)
                    {
                        Vector3 dir = (nextConveyor.transform.position - transform.position).normalized;
                        r.transform.position += dir * 0.1f;
                    }
                }
            }
            else
            {
                nextConveyor = null;
            }
        }
        else
        {
            nextConveyor = null;
        }
    }

    public virtual Vector2Int GetForwardOffset()
    {
        return facingDirection switch
        {
            Direction.Haut => Vector2Int.up,
            Direction.Bas => Vector2Int.down,
            Direction.Droite => Vector2Int.right,
            Direction.Gauche => Vector2Int.left,
            Direction.Any => Vector2Int.zero, // Aucun déplacement, ou défini selon ton besoin
            _ => Vector2Int.right
        };
    }

    public override void RefreshNeighbors()
    {
        base.RefreshNeighbors();
    }

    public override void Refresh()
    {
        UpdateNextConveyor();
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (nextConveyor != null)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawLine(transform.position, nextConveyor.transform.position);
        }

        // Visualisation des ressources
        Gizmos.color = Color.yellow;
        foreach (var r in resources)
            Gizmos.DrawSphere(r.transform.position, 0.05f);
    }
#endif
}
