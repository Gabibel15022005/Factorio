using Unity.VisualScripting;
using UnityEngine;

public class ScrollViewBuildingButton : MonoBehaviour
{
    [SerializeField] GameObject scrollView;
    [SerializeField] Transform content;

    void ShowScrollView(ScrollViewBuildingButton script)
    {
        if (script == this) SetActiveScroll(true);
        else SetActiveScroll(false);
    }

    public void SetActiveScroll(bool value)
    {
        scrollView.SetActive(value);
    }
    
    public void AddButtonToContent(ButtonBuilding buttonBuilding)
    {
        buttonBuilding.transform.SetParent(content);
    }
    void OnEnable()
    {
        ButtonCategory.ShowScrollViewAction += ShowScrollView;
    }
    void OnDisable()
    {
        ButtonCategory.ShowScrollViewAction -= ShowScrollView;
    }
}
