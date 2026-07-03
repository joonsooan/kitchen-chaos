# 플러그인 사용 설명서

바로 복붙해서 쓰는 용도. 각 항목 = "언제 + 코드/인스펙터 순서".

---

## Wingman

인스펙터에서 스크립트 빠르게 찾기.

- 인스펙터 상단 **검색창**에 이름 치면 컴포넌트/필드 바로 나옴.
- 설치만 돼있으면 됨. 따로 코드 없음.

---

## UIManager

> 유진이 처리.

---

## SoundManager

`SoundManager.Instance` 로 어디서든 접근 (싱글톤).

### 1) 먼저 준비

1. `GamePhase.cs`, `SFXType.cs` 에 enum 추가
   ```csharp
   public enum SFXType { Click, Jump, Hit, Coin }
   public enum GamePhase { Title, Play, Boss }
   ```
2. `Data > SO`의 SoundLibrary에 각 타입에 맞는 clip 넣기.

### 2) 바로 쓰는 코드

```csharp
// 효과음 (2D, 어디서 나든 소리 같음)
SoundManager.Instance.PlaySFX(SFXType.Click);

// 효과음 (3D, 특정 위치에서 — 거리감 있음)
SoundManager.Instance.PlaySFXAt(SFXType.Hit, transform.position);

// BGM 페이즈 전환 (crossfade 자동)
SoundManager.Instance.SetPhase(GamePhase.Boss);

// BGM 직접 재생 / 정지
SoundManager.Instance.PlayBGM(myClip);       // loop
SoundManager.Instance.PlayBGM(myClip, false); // 1회
SoundManager.Instance.StopBGMFade();          // 서서히 정지

// 전체 효과음 정지
SoundManager.Instance.StopAllSFX();

// 볼륨 (0~1)
SoundManager.Instance.SetBGMVolume(0.7f);
SoundManager.Instance.SetSFXVolume(0.5f);
SoundManager.Instance.SaveVolumes();          // PlayerPrefs 저장
```

**규칙**: UI/일반 효과음 → `PlaySFX`. 월드 오브젝트(적, 아이템) → `PlaySFXAt`.

---

## DOTween Pro (Demigiant)

트윈 애니메이션. **Pro라서 인스펙터로도** 트윈 세팅 가능.

### A) 인스펙터로 (코드 X) — Pro 전용

1. 오브젝트에 **Add Component → DOTween Animation** 붙임.
2. 인스펙터에서 설정:
   - **Animation Type**: Scale / Move / Fade / Rotate 등 선택
   - **Duration**, **Ease**, **To**(목표값) 지정
   - **Loops**: -1 = 무한, **Loop Type**: Yoyo(왕복) 등
   - **AutoPlay** 체크 → 시작 시 자동 재생
3. 여러 개 붙이고 **Play On** 타이밍 조절하면 코드 없이 연출 완성.

### B) 코드로 — 찰진 팝업 효과 예시

```csharp
using DG.Tweening;

// 팝업 통통 튀며 등장
transform.localScale = Vector3.zero;
transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);

// 버튼 눌림
transform.DOScale(0.9f, 0.1f).SetLoops(2, LoopType.Yoyo);

// Sequence — 순서대로 이어붙이기
Sequence seq = DOTween.Sequence();
seq.Append(transform.DOScale(1.2f, 0.15f));   // 커지고
seq.Append(transform.DOScale(1.0f, 0.15f));   // 돌아오고
seq.Join(spriteRenderer.DOFade(1f, 0.3f));    // 동시에 페이드인
```

- `Append` = 순서대로, `Join` = 동시에.
- 찰진 느낌 핵심 = **Ease** (`OutBack`, `OutBounce` 추천).

---

## Feel (MoreMountains)

화면 흔들림·타격감·진동 등 **종합 game feel**을 코드 거의 없이 붙임.

### 쓰는 순서

1. 빈 오브젝트에 **Add Component → MMF Player** (구버전은 MMFeedbacks).
2. **Add new feedback** 눌러 원하는 효과 추가:
   - **Transform → Scale / Position** : 튕김, 흔들기
   - **Camera → Camera Shake** : 카메라 흔들림
   - **Flash / Squash and Stretch** : 타격감
   - **Sound / Particles / Haptics(진동)**
3. 각 feedback에서 세기·시간 조절.
4. 코드에서 한 줄로 전체 재생:
   ```csharp
   [SerializeField] MMF_Player hitFeedback;

   void OnHit() => hitFeedback.PlayFeedbacks();
   ```

**언제**: 피격·획득·폭발 순간에 여러 효과(흔들림+사운드+파티클)를 한 번에.

**주의**
- Post Processing / Cinemachine / TextMeshPro 피드백은 해당 패키지 설치돼야 동작.
- URP면 `Feel/FeelDemosURP`의 `.unitypackage` import 필요.
- 문서: https://feel-docs.moremountains.com/

---

## Juicer

가벼운 UI/오브젝트 juice 효과. 컴포넌트 붙이는 방식.

### 쓰는 순서

1. 오브젝트에 `Assets/Plugins/Juicer/Core/Scripts`의 juice 컴포넌트 붙임.
2. 인스펙터에서 효과·타이밍 설정 후 재생.
3. `Demos` 폴더 예제 씬 열어보면 바로 감 잡힘.
4. 상세: `Assets/Plugins/Juicer/Juicer v1.1 - Documentation.pdf`.

---

## 뭐 쓸지 헷갈릴 때

| 상황 | 추천 |
|------|------|
| UI 버튼/팝업 통통 효과 | DOTween(코드·인스펙터) 또는 Juicer |
| 순서 있는 연출(커졌다 작아지기 등) | DOTween Sequence |
| 타격감·카메라 흔들림·진동 종합 | Feel (MMF Player) |
| 사운드 | SoundManager |
