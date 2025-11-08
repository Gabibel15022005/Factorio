using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonCategory : MonoBehaviour
{
    [SerializeField] Image image;
    ScrollViewBuildingButton scrollViewScipt;
    public static Action<ScrollViewBuildingButton> ShowScrollViewAction;

    public void SetButton(Sprite sprite,ScrollViewBuildingButton scrollScript)
    {
        image.sprite = sprite;
        scrollViewScipt = scrollScript;
    }
    public void OnClickButton()
    {
        ShowScrollViewAction?.Invoke(scrollViewScipt);
    }
}
