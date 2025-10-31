using System;
using UnityEngine;

public class BuildingUI : MonoBehaviour
{
    // this script will be on a UI and will be used to display the information of each building
    
    // this script will have module for each part that will be sent by the building like : name / sliding bar for progress / the actual ressources / etc ....
    
    private Building buildingScript;
    
    public static Action<Building> SendBuildingScript;

    private void OnEnable() { SendBuildingScript += GetBuildingScript; }
    private void OnDisable() { SendBuildingScript -= GetBuildingScript; }

    private void GetBuildingScript(Building script)
    {
        if (script == buildingScript) return;
        
        buildingScript = script;
        // Reset all module 

        foreach (ModuleType type in Enum.GetValues(typeof(ModuleType)))
        {
            
        }
    }
}



public enum ModuleType  // CHANGE TYPE LATER
{
    Text,
    Image,
    Slider,
    Ressources
}