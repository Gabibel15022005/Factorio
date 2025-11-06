using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class ListRessourcesUI : MonoBehaviour
{
    [Header("Prefab du module de ressource")]
    public GameObject ressourcesUIPrefab;

    // Liste actuelle des UI instanciées
    private List<RessourcesUI> ressources = new();

    /// <summary>
    /// Supprime ou conserve les RessourcesUI selon la liste actuelle de ressources
    /// </summary>
    private Dictionary<ResourceData, RessourcesUI> SyncRessourcesUI(Dictionary<ResourceData, int> resources)
    {
        Dictionary<ResourceData, RessourcesUI> existingDict = new();
        foreach (var res in ressources)
            existingDict[res.data] = res;

        // Supprime les UI qui ne sont plus présentes dans la nouvelle liste
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

        // Synchronisation
        Dictionary<ResourceData, RessourcesUI> existingDict = SyncRessourcesUI(resources);

        // Création / mise à jour
        foreach (var (data, count) in resources)
        {
            if (existingDict.TryGetValue(data, out RessourcesUI existing))
            {
                // Met à jour le texte
                existing.tmpText.text = count.ToString();
            }
            else
            {
                // Instanciation d’un nouveau prefab
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
                newUI.tmpText.text = count.ToString();

                if (newUI.image != null && data.sprite != null)
                    newUI.image.sprite = data.sprite;

                ressources.Add(newUI);
            }
        }
    }
}
