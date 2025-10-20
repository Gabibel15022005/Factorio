using UnityEngine;

public abstract class Building : MonoBehaviour
{
    [SerializeField] private int maxHp;
    private int hp;
    public int Hp => hp;

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

    public virtual void DestroyBuilding()
    {
        Debug.Log($"Destroy : {gameObject.name}");
        Destroy(gameObject);
    }
}
