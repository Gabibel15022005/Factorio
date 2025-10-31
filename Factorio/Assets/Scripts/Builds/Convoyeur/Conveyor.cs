using UnityEngine;
using System.Collections.Generic;
using System;

public class Conveyor : Building
{
#region Variable
    [Header("Conveyor Settings")]
    public float speed = 2f; // Vitesse de déplacement des ressources
    protected List<Resource> resources = new();
    public virtual int resourcesCount => resources.Count;

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
    
    protected bool needsUpdate = false;

#endregion

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
        if (!needsUpdate) return;
        MoveResources();

        // Si plus aucune ressource n’est présente, on désactive les updates
        if (resourcesCount == 0)
            needsUpdate = false;
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
                    // On essaye d'ajouter la ressource au prochain conveyor.
                    // AddResource renvoie true si elle est acceptée.
                    bool accepted = nextConveyor.AddResource(r);
                    if (accepted)
                    {
                        resources.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        // si non acceptée, on ne supprime pas la ressource et elle restera au centre
                        // tu peux décommenter pour debug :
                        // Debug.Log($"{name} : next conveyor {nextConveyor.name} refused resource {r.name}");
                    }
                }
            }
            else
            {
                r.transform.position = Center;
            }
        }
    }

    // Maintenant renvoie true si la ressource a été ajoutée.
    public virtual bool AddResource(Resource resource)
    {
        if (resources.Count >= maxResources)
            return false;
        
        OnResourceExitBridge(resource.gameObject);
        resource.canBeTaken = canRessourceBeTaken;
        resource.CurrentConveyor = this;
        resource.passedByCenter = false;
        resources.Add(resource);

        needsUpdate = true;
        return true;
    }

    public virtual void RemoveResource(Resource resource)
    {
        resources.Remove(resource);
        resource.CurrentConveyor = null;

        if (resources.Count == 0)
            needsUpdate = false;
    }

    /// <summary>
    /// Appelé quand une ressource quitte le pont.
    /// </summary>
    public void OnResourceExitBridge(GameObject resource)
    {
        var sr = resource.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        
        

        if (resource.TryGetComponent(out OriginalSortingOrder original))
        {
            sr.sortingOrder = original.originalSortingOrder;
        }
    }
    
    protected virtual void UpdateNextConveyor()
    {
        if (gridManagerRef == null) return;

        Conveyor neighborConveyor = GetConveyorByDirection(facingDirection);

        if (neighborConveyor != null)
        {
            Vector2Int currentPos = gridManagerRef.GetGridPosition(transform.position);
            Vector2Int neighborPos = gridManagerRef.GetGridPosition(neighborConveyor.transform.position);
            Vector2Int offset = neighborPos - currentPos;

            Direction dirToNeighbor = gridManagerRef.VectorToDirection(offset);

            if (!neighborConveyor.IsOpposingDirection(facingDirection, dirToNeighbor))
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

    public virtual Vector2Int GetForwardOffset(Direction dirToCheck)
    {
        return dirToCheck switch
        {
            Direction.Haut => Vector2Int.up,
            Direction.Bas => Vector2Int.down,
            Direction.Droite => Vector2Int.right,
            Direction.Gauche => Vector2Int.left,
            Direction.Any => Vector2Int.zero,
            _ => Vector2Int.right
        };
    }

    public virtual Conveyor GetConveyorByDirection(Direction dirToCheck)
    {
        Vector2Int currentPos = gridManagerRef.GetGridPosition(transform.position);
        Vector2Int forwardOffset = GetForwardOffset(dirToCheck);
        Vector2Int nextPos = currentPos + forwardOffset;

        Building neighbor = gridManagerRef.GetBuildingAt(nextPos);
        Conveyor neighborConveyor = neighbor as Conveyor;

        return neighborConveyor;
    }

    public virtual Dictionary<Direction, Conveyor> GetAllConveyorNearby()
    {
        var result = new Dictionary<Direction, Conveyor>();

        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            if (dir == Direction.Any) continue;

            Conveyor conv = GetConveyorByDirection(dir);
            if (conv != null)
                result.Add(dir, conv);
        }

        return result;
    }

    public virtual Dictionary<Direction, Conveyor> GetAllValidConveyorNearby()
    {
        var result = new Dictionary<Direction, Conveyor>();

        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            if (dir == Direction.Any) continue;

            Conveyor conv = GetConveyorByDirection(dir);
            if (conv != null && !IsOpposingDirection(conv.facingDirection, dir))
                result.Add(dir, conv);
        }

        return result;
    }

    public virtual Dictionary<Direction, Conveyor> GetAllOpposingConveyorNearby()
    {
        var result = new Dictionary<Direction, Conveyor>();

        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            if (dir == Direction.Any) continue;

            Conveyor conv = GetConveyorByDirection(dir);

            if (conv != null && IsOpposingDirection(conv.facingDirection, dir))
                result.Add(dir, conv);
        }

        return result;
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
        {
            if (r != null)
                Gizmos.DrawSphere(r.transform.position, 0.05f);
        }
    }
#endif
}
