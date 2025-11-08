using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Building : MonoBehaviour
{
    #region Variables
    [Header("Scriptable")]
    public BuildingData data;
    
    [Space(30)]
    [Header("Health Variables")]
    [SerializeField] private float initialMaxHp = 10;
    private float initialHp = 10;
    public float Hp => hpRef.Value;

    [Space(20)]
    [Header("Grid Variables")]
    protected GridManager gridManagerRef;
    protected Vector2Int buildingSize = Vector2Int.one;

    [Space(20)]
    [Header("Refresh nearby Variable")]
    [SerializeField] Vector2 refreshRange = new Vector2(2, 2);
    public Color refreshRangeColor = Color.red;

    [Space(20)]
    [Header("Direction Variable")]
    public Direction facingDirection;

    protected List<BuildingUIModule> modules = new();

    #endregion

    #region Refs

    // Les Ref<T> pour les modules UI
    [NonSerialized] public Ref<string> nameRef;
    [NonSerialized] public Ref<float> hpRef;
    [NonSerialized] public Ref<float> maxHpRef;

    #endregion
    
    protected virtual void Awake()
    {
        // Création des Ref<T> à partir des valeurs initiales
        nameRef = new Ref<string>();
        hpRef = new Ref<float>(initialHp);
        maxHpRef = new Ref<float>(initialMaxHp);
    }

    protected virtual void Start()
    {
        hpRef.Value = maxHpRef.Value;
    }

    public void SetGridManagerRef(GridManager refGrid) { gridManagerRef = refGrid; }

    public virtual void DestroyBuilding()
    {
        Debug.Log($"Destroy : {gameObject.name}");
        Destroy(gameObject);
    }

    #region Health Function

    public virtual void TakeDamage(int damage)
    {
        hpRef.Value -= damage;
        if (hpRef.Value <= 0) DestroyBuilding();
    }

    public virtual void Heal(int heal)
    {
        hpRef.Value += heal;
        if (hpRef.Value > maxHpRef.Value) hpRef.Value = maxHpRef.Value;
    }

    #endregion

    #region Refresh Function

    public virtual void RefreshNeighbors()
    {
        if (gridManagerRef == null) return;

        Refresh();

        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, refreshRange, transform.eulerAngles.z);
        foreach (Collider2D coll in colliders)
        {
            if (coll.gameObject == gameObject) continue;

            if (coll.TryGetComponent(out Building building))
            {
                building.Refresh();
            }
        }
    }

    public abstract void Refresh();

    #endregion

    #region BuildingSize Function

    public void SetBuildingSize(Vector2Int size) { buildingSize = size; }
    public Vector2Int GetBuildingSize() { return buildingSize; }

    #endregion

    #region Direction

    public bool IsOpposingDirection(Direction otherDir, Direction dirToOther)
    {
        if (facingDirection == Direction.Any)
        {
            return otherDir == GetOppositeDirection(dirToOther);
        }

        if (otherDir == Direction.Any)
            return false;

        return GetOppositeDirection(facingDirection) == otherDir;
    }

    public Direction GetOppositeDirection(Direction dir)
    {
        return dir switch
        {
            Direction.Haut => Direction.Bas,
            Direction.Bas => Direction.Haut,
            Direction.Droite => Direction.Gauche,
            Direction.Gauche => Direction.Droite,
            _ => Direction.Any
        };
    }

    #endregion

    #region UI

    public void ShowBuildingModules()
    {
        BuildingUI.SendBuildingScript?.Invoke(this);
    }

    public List<BuildingUIModule> GetBuildingModules()
    {
        if (modules.Count == 0)
        {
            CreateBuildingModules();
        }

        return modules;
    }

    protected virtual void CreateBuildingModules()
    {
        if (modules.Count != 0) return;

        modules.Add(CreateNameModule());

        modules.Add(CreateSliderModule(hpRef, maxHpRef,false));
    }

    protected virtual NameUIModule CreateNameModule()
    {
        var nameModule = new NameUIModule
        {
            nameRef = nameRef
        };
        return nameModule;
    }
    protected virtual RessourcesUIModule CreateRessourcesModule(
        Ref<Dictionary<ResourceData, int>> storedRef,
        Ref<Dictionary<ResourceData, int>> capacityRef)
    {
        var ressourceModule = new RessourcesUIModule
        {
            storedResourcesRef = storedRef,
            capacityResourcesRef = capacityRef
        };
        return ressourceModule;
    }

    protected virtual SliderUIModule CreateSliderModule(Ref<float> progressRef,Ref<float> maxProgressRef, bool isPercentage)
    {
        var sliderModule = new SliderUIModule
        {
            sliderValueRef = progressRef,
            maxValueRef = maxProgressRef
        };
        sliderModule.isPercent = isPercentage;
        return sliderModule;
    }
    
    protected virtual SliderUIModule CreateSliderModule(Ref<float> progressRef,Ref<float> maxProgressRef, bool isPercentage, Color background, Color fill)
    {
        var sliderModule = new SliderUIModule
        {
            sliderValueRef = progressRef,
            maxValueRef = maxProgressRef
        };
        sliderModule.isPercent = isPercentage;

        sliderModule.changeSliderColor = true;
        
        sliderModule.backgroundColor = background;
        sliderModule.fillColor = fill;
        
        return sliderModule;
    }
    
    protected virtual SliderUIModule CreateSliderModule(Ref<float> progressRef,Ref<float> maxProgressRef, bool isPercentage, Color background, Color fill ,Sprite sprite)
    {
        var sliderModule = new SliderUIModule
        {
            sliderValueRef = progressRef,
            maxValueRef = maxProgressRef
        };
        sliderModule.isPercent = isPercentage;

        sliderModule.changeSliderColor = true;
        
        sliderModule.ressourceSprite =  sprite;
        sliderModule.backgroundColor = background;
        sliderModule.fillColor = fill;
        
        return sliderModule;
    }

    #endregion
    
    protected virtual void OnDrawGizmos()
    {
        if (refreshRange == Vector2.zero) return;
        Gizmos.color = refreshRangeColor;

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;

        Gizmos.DrawWireCube(Vector3.zero, refreshRange);

        Gizmos.matrix = Matrix4x4.identity;
    }
}
