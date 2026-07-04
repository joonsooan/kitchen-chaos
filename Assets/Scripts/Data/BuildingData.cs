using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "KitchenChaos/BuildingData")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public Vector2Int[] footprintCells = { Vector2Int.zero };
    public CookingMethod cookingMethod = CookingMethod.None;
}
