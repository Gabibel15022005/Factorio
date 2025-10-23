using UnityEngine;
using UnityEngine.UIElements;

public class ScrollViewManager : MonoBehaviour
{
    [Header("Categories")]
    [SerializeField] Category[] categories;


    [Space(30)]
    [Header("Prefabs Ref")]
    [SerializeField] GameObject buttonCategoryPrefab;
    [SerializeField] GameObject buttonBuildingPrefab;
    [SerializeField] GameObject scrollViewPrefab;

    [Space(30)]
    [Header("GameObjects Parents Ref")]
    [SerializeField] Transform buttonCategoryPrefabParent;
    [SerializeField] Transform scrollViewPrefabParent;

    void Start()
    {
        ForTestPlayerPref();
        InstantiateAllButton();
    }

    void ForTestPlayerPref()
    {
        foreach (Category category in categories)
        {
            foreach (BuildingData data in category.buildingsOfCategory)
            {
                PlayerPrefs.SetInt($"{data.buildingName} is unlocked", 1);
            }
        }

        PlayerPrefs.Save();
    }

    void InstantiateAllButton()
    {
        foreach (Category category in categories)
        {
            GameObject scroll = Instantiate(scrollViewPrefab, scrollViewPrefabParent);

            // Récupère le RectTransform
            RectTransform rect = scroll.GetComponent<RectTransform>();

            // Réinitialise les offsets pour le centrer
            rect.anchoredPosition = Vector2.zero;
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;

            GameObject buttonCategory = Instantiate(buttonCategoryPrefab, buttonCategoryPrefabParent);

            ButtonCategory buttonCategoryScript = buttonCategory.GetComponent<ButtonCategory>();
            ScrollViewBuildingButton scrollScript = scroll.GetComponent<ScrollViewBuildingButton>();
            
            buttonCategoryScript.SetButton(category.categorySprite, scrollScript);

            foreach (BuildingData data in category.buildingsOfCategory)
            {
                if (PlayerPrefs.HasKey($"{data.buildingName} is unlocked") && PlayerPrefs.GetInt($"{data.buildingName} is unlocked") == 1)
                {
                    ButtonBuilding button = Instantiate(buttonBuildingPrefab).GetComponent<ButtonBuilding>();
                    button.SetButton(data);
                    scrollScript.AddButtonToContent(button);
                }
            }

            scrollScript.SetActiveScroll(false);
        }
    }


}
