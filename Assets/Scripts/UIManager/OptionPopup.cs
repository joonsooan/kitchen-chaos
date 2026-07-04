using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 설정 팝업 — BGM/SFX 볼륨 슬라이더 (SoundManager 연동), 메인/종료 버튼
public class OptionPopup : UIPopup
{
    enum Sliders
    {
        BgmSlider,
        SfxSlider,
    }

    enum GameObjects
    {
        MainButton,
        QuitButton,
        CloseButton,
    }

    private Slider bgmSlider;
    private Slider sfxSlider;

    public override void Init()
    {
        Bind<Slider>(typeof(Sliders));
        Bind<GameObject>(typeof(GameObjects));

        bgmSlider = Get<Slider>((int)Sliders.BgmSlider);
        sfxSlider = Get<Slider>((int)Sliders.SfxSlider);

        // 저장된 볼륨으로 초기화 (SoundManager 기본값과 동일 키)
        if (bgmSlider != null)
        {
            bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
            bgmSlider.onValueChanged.AddListener(v =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.SetBGMVolume(v);
            });
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
            sfxSlider.onValueChanged.AddListener(v =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.SetSFXVolume(v);
            });
        }

        BindEvent(Get<GameObject>((int)GameObjects.MainButton), OnMainClicked);
        BindEvent(Get<GameObject>((int)GameObjects.QuitButton), OnQuitClicked);
        BindEvent(Get<GameObject>((int)GameObjects.CloseButton),
            evt => UIManager.Instance.ClosePopupUI(this));
    }

    private void OnDestroy()
    {
        // 팝업 닫힐 때 볼륨 저장
        if (SoundManager.Instance != null) SoundManager.Instance.SaveVolumes();
    }

    private void OnMainClicked(PointerEventData evt)
    {
        // TODO: 메인 화면 흐름 확정되면 전환 연결
        Debug.Log("[OptionPopup] 메인 화면으로 (흐름 미정)");
        UIManager.Instance.ClosePopupUI(this);
    }

    private void OnQuitClicked(PointerEventData evt)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
