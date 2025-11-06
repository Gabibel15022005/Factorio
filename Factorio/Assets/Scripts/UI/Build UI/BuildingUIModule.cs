using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public abstract class BuildingUIModule
{
    protected GameObject uiInstance; // instance du prefab
    public abstract ModuleType type { get; }
    public abstract GameObject CreateModule(Transform parentTransf, GameObject prefabUI);
    public abstract void UpdateModuleOnChange();

    public virtual void Dispose()
    {
        Debug.Log("Dispose called");
        
        if (uiInstance != null)
        {
            Object.DestroyImmediate(uiInstance);
            uiInstance = null;
        }
    }
}

#region --- NameUIModule ---
public class NameUIModule : BuildingUIModule
{
    public override ModuleType type => ModuleType.Name;
    public Ref<string> nameRef;
    private TMP_Text nameText;

    public string Name
    {
        get => nameRef?.Value;
        set => nameRef.Value = value;
    }

    public override GameObject CreateModule(Transform parentTransf, GameObject prefabUI)
    {
        uiInstance = Object.Instantiate(prefabUI, parentTransf);
        uiInstance.name = "NameUI";

        nameText = uiInstance.GetComponentInChildren<TMP_Text>();
        if (nameText != null)
            nameText.text = Name;

        nameRef.OnValueChanged += HandleNameChanged;
        return uiInstance;
    }

    private void HandleNameChanged(string _) => UpdateModuleOnChange();

    public override void UpdateModuleOnChange()
    {
        if (nameText != null)
            nameText.text = Name;
    }

    public override void Dispose()
    {
        base.Dispose();
        if (nameRef != null)
            nameRef.OnValueChanged -= HandleNameChanged;
    }
}
#endregion


#region --- SliderUIModule ---
public class SliderUIModule : BuildingUIModule
{
    public override ModuleType type => ModuleType.Slider;
    public Ref<float> sliderValueRef;
    public Ref<float> maxValueRef;

    private SliderScript sliderScript = null;

    private Slider slider;
    private TMP_Text valueText;
    public bool isPercent = false;
    
    public Color backgroundColor;
    public Color fillColor;
    public bool changeSliderColor = false;
    
    public Sprite ressourceSprite = null;

    public float Value
    {
        get => sliderValueRef?.Value ?? 0;
        set => sliderValueRef.Value = value;
    }

    public float Max => maxValueRef?.Value ?? 1;

    public override GameObject CreateModule(Transform parentTransf, GameObject prefabUI)
    {
        uiInstance = Object.Instantiate(prefabUI, parentTransf);
        uiInstance.name = "SliderUI";
        
        sliderScript = uiInstance.GetComponent<SliderScript>(); 

        slider = sliderScript.slider;
        valueText = sliderScript.text;

        if (changeSliderColor)
        {
            sliderScript.backgroundImage.color = backgroundColor;
            sliderScript.fillImage.color = fillColor;
        }

        if (ressourceSprite != null)
        {
            sliderScript.typeImage.sprite = ressourceSprite;
            sliderScript.typeImage.color = Color.white;
        }

        if (slider != null)
        {
            slider.maxValue = Max;
            slider.value = Value;
        }

        if (valueText != null)
            valueText.text = $"{Value:0.##} / {Max:0.##}";

        sliderValueRef.OnValueChanged += HandleSliderChanged;
        maxValueRef.OnValueChanged += HandleSliderChanged;
        
        return uiInstance;
    }

    private void HandleSliderChanged(float _) => UpdateModuleOnChange();

    public override void UpdateModuleOnChange()
{
    if (slider != null)
    {
        slider.maxValue = Max;
        slider.value = Value;
    }

    if (valueText != null)
    {
        if (isPercent)
        {
            float percent = Max == 0 ? 0 : (Value / Max) * 100f;
            valueText.text = $"{Mathf.RoundToInt(percent)}%";
        }
        else
        {
            valueText.text = $"{Value:0.##} / {Max:0.##}";
        }
    }
}


    public override void Dispose()
    {
        base.Dispose();
        if (sliderValueRef != null)
            sliderValueRef.OnValueChanged -= HandleSliderChanged;
        if (maxValueRef != null)
            maxValueRef.OnValueChanged -= HandleSliderChanged;
    }
}

#endregion


#region --- ImageUIModule ---
public class ImageUIModule : BuildingUIModule
{
    public override ModuleType type => ModuleType.Image;
    public Ref<Sprite> imageRef;
    private Image image;

    public override GameObject CreateModule(Transform parentTransf, GameObject prefabUI)
    {
        uiInstance = Object.Instantiate(prefabUI, parentTransf);
        uiInstance.name = "ImageUI";

        image = uiInstance.GetComponentInChildren<Image>();
        if (image != null && imageRef != null)
            image.sprite = imageRef.Value;

        imageRef.OnValueChanged += HandleImageChanged;
        
        return uiInstance;
    }

    private void HandleImageChanged(Sprite _) => UpdateModuleOnChange();

    public override void UpdateModuleOnChange()
    {
        if (image != null && imageRef != null)
            image.sprite = imageRef.Value;
    }

    public override void Dispose()
    {
        base.Dispose();
        if (imageRef != null)
            imageRef.OnValueChanged -= HandleImageChanged;
    }
}

#endregion

#region --- RessourcesUIModule ---
public class RessourcesUIModule : BuildingUIModule
{
    public override ModuleType type => ModuleType.Ressources;

    // 🔹 Références vers les ressources (quantités actuelles et capacités)
    public Ref<Dictionary<ResourceData, int>> storedResourcesRef;
    public Ref<Dictionary<ResourceData, int>> capacityResourcesRef;

    private ListRessourcesUI ressourcesUI;

    private Dictionary<ResourceData, int> Stored => storedResourcesRef?.Value ?? new();
    private Dictionary<ResourceData, int> Capacity => capacityResourcesRef?.Value ?? new();

    public override GameObject CreateModule(Transform parentTransf, GameObject prefabUI)
    {
        uiInstance = Object.Instantiate(prefabUI, parentTransf);
        uiInstance.name = "RessourcesUI";

        ressourcesUI = uiInstance.GetComponent<ListRessourcesUI>();
        if (ressourcesUI == null)
        {
            Debug.LogError("Le prefab du module Ressources doit contenir un ListRessourcesUI !");
            return uiInstance;
        }

        // Première mise à jour
        ressourcesUI.UpdateRessources(Stored);

        // Abonnement aux changements de valeurs
        if (storedResourcesRef != null)
            storedResourcesRef.OnValueChanged += HandleResourcesChanged;

        if (capacityResourcesRef != null)
            capacityResourcesRef.OnValueChanged += HandleResourcesChanged;

        return uiInstance;
    }

    private void HandleResourcesChanged(Dictionary<ResourceData, int> _)
    {
        UpdateModuleOnChange();
    }

    public override void UpdateModuleOnChange()
    {
        if (ressourcesUI != null)
            ressourcesUI.UpdateRessources(Stored);
    }

    public override void Dispose()
    {
        base.Dispose();

        if (storedResourcesRef != null)
            storedResourcesRef.OnValueChanged -= HandleResourcesChanged;

        if (capacityResourcesRef != null)
            capacityResourcesRef.OnValueChanged -= HandleResourcesChanged;
    }
}
#endregion

public enum ModuleType
{
    Name,
    Slider,
    Image,
    Ressources
}