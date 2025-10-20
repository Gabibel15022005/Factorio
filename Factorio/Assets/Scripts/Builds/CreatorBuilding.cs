using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class CreatorBuilding : Building
{
    protected virtual Resource SpawnResource(ResourceData data, Vector3 pos)
    {
        Resource resource = Instantiate(data.resourcePrefab, pos, quaternion.identity).GetComponent<Resource>();
        return resource;
    }
}
