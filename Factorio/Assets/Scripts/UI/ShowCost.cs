using System;
using System.Collections.Generic;
using UnityEngine;

public class ShowCost : MonoBehaviour
{
    [SerializeField] private ListRessourcesUI ressourcesUI;
    private void OnEnable()
    {
        BuildingPlacer.ChangeBuildingToPlaceAction +=  UpdateCostUI;
    }

    private void OnDisable()
    {
        BuildingPlacer.ChangeBuildingToPlaceAction -=  UpdateCostUI;
    }

    private void UpdateCostUI(BuildingData buildingData)
    {
        Dictionary<ResourceData, int> costDict = new Dictionary<ResourceData, int>();
        foreach (RessourceCount rc in buildingData.cost)
        {
            if (rc.resource != null)
                costDict[rc.resource] = rc.count;
        }
        ressourcesUI.UpdateRessources(costDict);
    }
}
