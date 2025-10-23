using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class CheckPos
{
    [SerializeField] private Transform checkArea;
    public Transform CheckArea => checkArea;

    [SerializeField] private Transform spawnArea;
    public Transform SpawnArea => spawnArea;
}

[Serializable]
public class RessourceList
{
    public ResourceType ressourceNeeded;
    public int quantityNeeded = 1;       // quantité requise pour produire
    public int maxCapacity = 10;         // capacité max stockable dans cette liste
    [HideInInspector] public List<Resource> resources = new();
}

public abstract class CreatorBuilding : Building
{
    // === PRODUCE ===
    [Header("Production Settings")]
    [SerializeField] protected ResourceData resourceData;
    [SerializeField] protected int amountPerProduce = 1;
    [SerializeField] protected int maxStoredResources = 10;
    protected readonly List<Resource> storedResources = new();

    // === INPUT CHECKS ===
    [Header("Inputs Checks")]
    [SerializeField] protected List<CheckPos> inputChecks = new();
    [SerializeField] protected List<RessourceList> inputResources = new();
    [SerializeField] protected LayerMask inputInteractLayer;

    // === OUTPUT CHECKS ===
    [Header("Outputs Checks")]
    protected List<Conveyor> outputConveyors = new();
    [SerializeField] protected List<CheckPos> outputChecks = new();
    [SerializeField] protected float size = 0.5f;
    [SerializeField] protected LayerMask interactLayer;

    [Header("Transfer Settings")]
    [SerializeField] protected float transferDelay = 0.35f;
    protected bool isTransferring = false;

    public int StoredCount => storedResources.Count;
    public int MaxStored => maxStoredResources;

    protected virtual void TickProduce(float deltaTime) { }
    protected virtual void TickTransfer(float deltaTime) => TryTransferResources();
    protected virtual void TickPullResources(float deltaTime) => TryPullResources();

    protected void TryPullResources()
    {
        foreach (var check in inputChecks)
        {
            if (check.CheckArea == null) continue;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(check.CheckArea.position, size, inputInteractLayer);

            foreach (Collider2D coll in colliders)
            {
                if (!coll.TryGetComponent(out Resource resource)) continue;

                Conveyor conv = resource.CurrentConveyor;
                if (conv == null || !resource.canBeTaken || !resource.passedByCenter) continue;

                Vector3 dirToFactory = (transform.position - conv.transform.position).normalized;
                Vector3 convForward = conv.transform.right;
                if (Vector3.Dot(convForward, dirToFactory) < 0.7f)
                    continue;

                RessourceList targetList = inputResources.Find(r => r.ressourceNeeded == resource.scriptable.resourceName);
                if (targetList == null) continue;

                if (targetList.resources.Count >= targetList.maxCapacity)
                    continue;

                conv.RemoveResource(resource);
                targetList.resources.Add(resource);

                float speed = conv.speed;
                StartCoroutine(MoveResourceTo(resource, check.SpawnArea != null ? check.SpawnArea.position : transform.position, speed));
            }
        }
    }

    protected IEnumerator MoveResourceTo(Resource resource, Vector3 targetPos, float speed)
    {
        while (resource != null && Vector3.Distance(resource.transform.position, targetPos) > 0.05f)
        {
            resource.transform.position = Vector3.MoveTowards(resource.transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        if (resource != null)
            resource.transform.position = targetPos;
    }

    protected bool CanProduce =>
        storedResources.Count < maxStoredResources &&
        inputResources.TrueForAll(r => r.resources.Count >= r.quantityNeeded);

    protected virtual void TryProduce()
    {
        if (!CanProduce)
            return;

        foreach (var ressourceList in inputResources)
        {
            ressourceList.resources.RemoveRange(0, ressourceList.quantityNeeded);
        }

        for (int i = 0; i < amountPerProduce; i++)
        {
            Resource newRes = SpawnResource(resourceData, transform.position);
            newRes.gameObject.SetActive(false);
            storedResources.Add(newRes);
        }
    }

    protected virtual Resource SpawnResource(ResourceData data, Vector3 pos)
    {
        Resource resource = Instantiate(data.resourcePrefab, pos, quaternion.identity).GetComponent<Resource>();
        return resource;
    }

    protected void TryTransferResources()
    {
        if (isTransferring || storedResources.Count == 0 || outputConveyors.Count == 0)
            return;

        StartCoroutine(TransferCoroutine());
    }

    protected IEnumerator TransferCoroutine()
    {
        isTransferring = true;

        while (storedResources.Count > 0)
        {
            Conveyor target = GetAvailableConveyor();
            if (target == null)
            {
                yield return new WaitForSeconds(transferDelay);
                continue;
            }

            Resource resource = storedResources[0];
            storedResources.RemoveAt(0);

            resource.gameObject.SetActive(true);
            resource.transform.position = target.spawnPos != null ? target.spawnPos.position : target.transform.position;
            target.AddResource(resource);

            yield return new WaitForSeconds(transferDelay);
        }

        isTransferring = false;
    }

    protected Conveyor GetAvailableConveyor()
    {
        List<Conveyor> validConveyors = new();
        int maxFree = -1;

        foreach (var c in outputConveyors)
        {
            if (c == null) continue;
            int free = c.maxResources - c.resourcesCount;
            if (free <= 0) continue;

            if (free > maxFree)
            {
                validConveyors.Clear();
                maxFree = free;
            }

            if (free == maxFree)
                validConveyors.Add(c);
        }

        if (validConveyors.Count == 0) return null;

        return validConveyors[UnityEngine.Random.Range(0, validConveyors.Count)];
    }

    protected virtual void OnDestroy()
    {
        foreach (var rList in inputResources)
        {
            foreach (var res in rList.resources)
                if (res != null) Destroy(res.gameObject);
            rList.resources.Clear();
        }

        foreach (var res in storedResources)
            if (res != null) Destroy(res.gameObject);
        storedResources.Clear();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (inputChecks != null && inputChecks.Count != 0)
        {
            Gizmos.color = Color.green;
            foreach (var inputCheck in inputChecks)
            {
                Gizmos.DrawWireSphere(inputCheck.CheckArea.position, size);
                Gizmos.DrawWireSphere(inputCheck.SpawnArea.position, size / 2);
            }
        }

        if (outputChecks != null && outputChecks.Count != 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var outputCheck in outputChecks)
            {
                Gizmos.DrawWireSphere(outputCheck.CheckArea.position, size);
                Gizmos.DrawWireSphere(outputCheck.SpawnArea.position, size / 2);
            }
        }
    }
}
