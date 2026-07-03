using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "KitchenChaos/BuildingData")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public Vector2Int[] footprintCells = { Vector2Int.zero };

    // TODO: 조리 방법 관련 필드 추가 예정
}
