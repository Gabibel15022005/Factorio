using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // Dictionnaire : ResourceData → quantité
    private Dictionary<ResourceData, int> resources = new();

    public System.Action<ResourceData, int> OnResourceChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Ajoute une ressource (via son ResourceData)
    /// </summary>
    public void AddResource(ResourceData data, int amount = 1)
    {
        if (data == null)
        {
            Debug.LogWarning("Tried to add a null ResourceData to inventory.");
            return;
        }

        if (resources.ContainsKey(data))
            resources[data] += amount;
        else
            resources[data] = amount;

        OnResourceChanged?.Invoke(data, resources[data]);
    }

    /// <summary>
    /// Retire une ressource (si possible)
    /// </summary>
    public bool RemoveResource(ResourceData data, int amount = 1)
    {
        if (data == null || !resources.ContainsKey(data) || resources[data] < amount)
            return false;

        resources[data] -= amount;

        if (resources[data] <= 0)
            resources.Remove(data);

        OnResourceChanged?.Invoke(data, resources.ContainsKey(data) ? resources[data] : 0);
        return true;
    }

    /// <summary>
    /// Retourne une copie du stock actuel
    /// </summary>
    public Dictionary<ResourceData, int> GetResources()
    {
        return new Dictionary<ResourceData, int>(resources);
    }

    /// <summary>
    /// Retourne la quantité d’un type donné
    /// </summary>
    public int GetCount(ResourceData data)
    {
        return data != null && resources.TryGetValue(data, out var count) ? count : 0;
    }
}

[System.Serializable]
public struct RessourceCount
{
    public ResourceData resource;
    public int count;
}
