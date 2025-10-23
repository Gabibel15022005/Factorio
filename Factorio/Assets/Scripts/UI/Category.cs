using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Category/Category")]
public class Category : ScriptableObject
{
    public Categories category;
    public Sprite categorySprite;
    public BuildingData[] buildingsOfCategory;
}

public enum Categories
{
    Conveyor,
    Mining,
    Factory,
    Electricity,
    Liquid

}