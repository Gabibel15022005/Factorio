using System.Collections.Generic;
using UnityEngine;

public class Routeur : Conveyor
{
    private List<Conveyor> validNextConveyors = new List<Conveyor>();
    private int currentIndex = 0; // round-robin

    public override void Refresh()
    {
        UpdateNextConveyors();
    }

    private void UpdateNextConveyors()
    {
        validNextConveyors.Clear();
        if (gridManagerRef == null) return;

        Vector2Int currentPos = gridManagerRef.GetGridPosition(transform.position);

        // Vérifie les 4 directions autour
        Vector2Int[] offsets = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int offset in offsets)
        {
            Building neighbor = gridManagerRef.GetBuildingAt(currentPos + offset);
            Conveyor neighborConv = neighbor as Conveyor;
            if (neighborConv == null) continue;

            // 🧭 Vérifie si le voisin pointe vers le routeur (à éviter)
            Vector2Int dirFromNeighbor = neighborConv.GetForwardOffset();
            if (dirFromNeighbor == -offset)
                continue; // ce convoyeur envoie vers le routeur → on l'ignore

            // Sinon, c’est un convoyeur valide pour sortie
            validNextConveyors.Add(neighborConv);
        }

        nextConveyor = null; // le routeur n’a pas de next unique
    }

    protected override void MoveResources()
    {
        if (resources.Count == 0) return;

        for (int i = 0; i < resources.Count; i++)
        {
            Resource r = resources[i];

            // Étape 1 : aller au centre
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

            // Étape 2 : distribuer la ressource
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

        // 🔹 Visualisation des sorties
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
