using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingUI : MonoBehaviour
{
    private Building buildingScript;
    private Building lastBuildingScript;
    private Camera mainCamera;

    public static Action<Building> SendBuildingScript;

    [Header("Prefabs par type de module")]
    [SerializeField] private List<ModulePrefabEntry> modulePrefabs = new(); // assigné dans l’inspector
    private Dictionary<ModuleType, GameObject> prefabByType;

    private readonly List<GameObject> currentUIs = new();

    private void Awake()
    {
        prefabByType = new Dictionary<ModuleType, GameObject>();
        foreach (var entry in modulePrefabs)
        {
            if (!prefabByType.ContainsKey(entry.type))
                prefabByType.Add(entry.type, entry.prefab);
        }
        
        mainCamera = Camera.main;
    }
    
    private void Update()
    {

        if (buildingScript == null && currentUIs.Count > 0)
        {
            // Avant que Unity détruise la scène, on nettoie proprement
            foreach (var ui in currentUIs)
            {
                if (ui != null)
                    DestroyImmediate(ui);
            }
            currentUIs.Clear();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.T)) // test damage
            {
                buildingScript.TakeDamage(2);
            }
            if (Input.GetKeyDown(KeyCode.H)) // test damage
            {
                buildingScript.Heal(2);
            }
        }
        
        
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.TryGetComponent(out Building building))
        {
            if (lastBuildingScript == building) return;
            lastBuildingScript = building;
            building.ShowBuildingModules();
        }
    }

    private void OnEnable() => SendBuildingScript += GetBuildingScript;

    private void OnDisable()
    {
        SendBuildingScript -= GetBuildingScript;
        
        // Avant que Unity détruise la scène, on nettoie proprement
        foreach (var ui in currentUIs)
        {
            if (ui != null)
                DestroyImmediate(ui);
        }
        currentUIs.Clear();
    } 

    private void GetBuildingScript(Building script)
    {
        if (script == buildingScript) return;

        buildingScript = script;

        // 🔄 Reset tous les anciens modules UI
        foreach (var ui in currentUIs)
            Destroy(ui);
        currentUIs.Clear();

        // 🧩 Crée les nouveaux modules à partir du building
        foreach (var module in script.GetBuildingModules())
        {
            CreateNewModule(module);
        }
    }

    protected virtual void CreateNewModule(BuildingUIModule uiModule)
    {
        if (!prefabByType.TryGetValue(uiModule.type, out var prefab))
        {
            Debug.LogWarning($"Aucun prefab assigné pour le module {uiModule.type}");
            return;
        }

        // Appelle la fonction de création spécifique du module
        GameObject instance = uiModule.CreateModule(transform, prefab);
        currentUIs.Add(instance);
    }
}

[Serializable]
public struct ModulePrefabEntry
{
    public ModuleType type;
    public GameObject prefab;
}