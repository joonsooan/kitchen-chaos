// 컨테이너 내부 상태: 미완성 / 완성 / 실패(어떤 레시피도 완성 불가)
public enum ContainerState
{
    InProgress, // 미완성: 조합 중, 아직 완성 가능
    Complete,   // 완성: 레시피 정확히 매칭
    Failed,     // 실패: 어떤 레시피도 완성 불가
}
