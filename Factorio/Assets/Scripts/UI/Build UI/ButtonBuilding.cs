using UnityEngine;
using UnityEngine.UI;

public class ButtonBuilding : MonoBehaviour
{
    BuildingData buildingData;
    [SerializeField] Image image;

    public void SetButton(BuildingData data)
    {
        buildingData = data;
        image.sprite = data.iconSprite;
    }
    public void OnButtonClick()
    {
        BuildingPlacer.ChangeBuildingToPlaceAction?.Invoke(buildingData);
    }
}
