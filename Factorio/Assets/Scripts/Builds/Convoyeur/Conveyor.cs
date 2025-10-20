using UnityEngine;
using System.Collections.Generic;

public class Conveyor : Building
{
    public float speed = 2f;
    private List<Resource> resources = new();

    public Conveyor nextConveyor; // Prochain conveyor pour le déplacement
    public Conveyor[] splitConveyors; // jusqu'à 3 directions

    void Update()
    {
        for (int i = resources.Count - 1; i >= 0; i--)
        {
            Resource r = resources[i];
            if (nextConveyor != null)
            {
                r.transform.position = Vector3.MoveTowards(r.transform.position, nextConveyor.transform.position, speed * Time.deltaTime);
                if (Vector3.Distance(r.transform.position, nextConveyor.transform.position) < 0.1f)
                {
                    nextConveyor.AddResource(r);
                    resources.RemoveAt(i);
                }
            }
        }
    }

    public void AddResource(Resource resource)
    {
        resources.Add(resource);
    }
}