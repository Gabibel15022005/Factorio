using Unity.VisualScripting;
using UnityEngine;
public abstract class Building : MonoBehaviour
{
    #region  Variables

    [Header("Health Variables")]
    [SerializeField] private int maxHp = 10;
    [SerializeField] private int hp;
    public int Hp => hp;


    [Space(20)]
    [Header("Grid Variables")]
    protected GridManager gridManagerRef;
    protected Vector2Int buildingSize = Vector2Int.one; // par défaut 1x1

    [Space(20)]
    [Header("Refresh nearby Variable")]
    [SerializeField] Vector2 refreshRange = new Vector2(2,2);
    public Color refreshRangeColor = Color.red;

    [Space(20)]
    [Header("Direction Variable")]
    public Direction facingDirection;
    
    

    #endregion
    protected virtual void Start() { hp = maxHp; }
    public void SetGridManagerRef(GridManager refGrid) { gridManagerRef = refGrid; }
    public virtual void DestroyBuilding()
    {
        Debug.Log($"Destroy : {gameObject.name}");
        Destroy(gameObject);
    }

    #region Health Function
    public virtual void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0) DestroyBuilding();
    }
    public virtual void Heal(int heal)
    {
        hp += heal;
        if (hp > maxHp) hp = maxHp;
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
                //Debug.Log($"Refreh : {building.gameObject.name}");
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

    protected virtual void ShowBuildingUI()
    {
        BuildingUI.SendBuildingScript?.Invoke(this);
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
