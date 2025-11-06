using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class ListRessourcesUI : MonoBehaviour
{
    [Header("Prefab du module de ressource")]
    public GameObject ressourcesUIPrefab;

    private List<RessourcesUI> ressources = new();

    /// <summary>
    /// Supprime ou conserve les RessourcesUI selon la liste actuelle de ressources
    /// </summary>
    private Dictionary<ResourceData, RessourcesUI> SyncRessourcesUI(Dictionary<ResourceData, int> resources)
    {
        Dictionary<ResourceData, RessourcesUI> existingDict = new();
        foreach (var res in ressources)
            existingDict[res.data] = res;

        for (int i = ressources.Count - 1; i >= 0; i--)
        {
            RessourcesUI res = ressources[i];
            if (!resources.ContainsKey(res.data))
            {
                Destroy(res.gameObject);
                ressources.RemoveAt(i);
                existingDict.Remove(res.data);
            }
        }

        return existingDict;
    }

    /// <summary>
    /// Met à jour ou crée les UI de ressources à partir d’un dictionnaire (ResourceData → quantité)
    /// </summary>
    public void UpdateRessources(Dictionary<ResourceData, int> resources)
    {
        if (ressourcesUIPrefab == null)
        {
            Debug.LogError("No prefab assigned to ListRessourcesUI");
            return;
        }

        Dictionary<ResourceData, RessourcesUI> existingDict = SyncRessourcesUI(resources);

        foreach (var (data, count) in resources)
        {
            string formatted = FormatNumber(count);

            if (existingDict.TryGetValue(data, out RessourcesUI existing))
            {
                existing.tmpText.text = formatted;
            }
            else
            {
                GameObject go = Instantiate(ressourcesUIPrefab, transform);
                RessourcesUI newUI = go.GetComponent<RessourcesUI>();

                if (newUI == null)
                {
                    Debug.LogError("Le prefab ne contient pas de composant RessourcesUI !");
                    Destroy(go);
                    continue;
                }

                // Initialise les infos
                newUI.data = data;
                newUI.tmpText.text = formatted;

                if (newUI.image != null && data.sprite != null)
                    newUI.image.sprite = data.sprite;

                ressources.Add(newUI);
            }
        }
    }

    /// <summary>
    /// Formate un nombre en version courte : 1,2K / 1,5M
    /// </summary>
    private string FormatNumber(int value)
    {
        if (value < 1000)
            return value.ToString();

        if (value < 1_000_000)
            return (value / 1000f).ToString("0.#") + "K";

        return (value / 1_000_000f).ToString("0.#") + "M";
    }
}
