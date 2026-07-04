using UnityEngine;

// 랜덤박스 버프 — 효과는 서브클래스가 구현 (버프 추가 = SO 에셋 추가, 코드 수정 없음)
public abstract class BuffData : ScriptableObject
{
    public string buffName;
    [TextArea] public string description;
    public Sprite icon;
    public float duration = 30f;              // 0 이하 = 즉시 소멸 (꽝)
    [Range(0, 100)] public int weight = 25;   // 등장 확률 가중치

    public abstract void Apply();
    public abstract void Remove();
}
