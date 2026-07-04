// 몬스터 HP 바가 읽는 체력 인터페이스 (CabbageMonster·WeedTile 구현)
public interface IHasHealth
{
    int CurrentHealth { get; }
    int MaxHealth { get; }
}
