using TMPro;
using UnityEngine;

public class InGameHUD : UIHUD
{
    
    enum Texts
    {
        TimeText,
    }

    enum GameObjects
    {
        OrderLayout,
    }

    private const string OrderPrefabPath = "UI/Slot/Order";

    private TextMeshProUGUI TimeText;
    private Transform _orderLayout;
    private float _elapsed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Init();
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        TimeText     = Get<TextMeshProUGUI>((int)Texts.TimeText);
        _orderLayout = Get<GameObject>((int)GameObjects.OrderLayout).transform;
    }

    // 주문 1개 추가 — 게임 로직에서 호출
    public UISlot AddOrder(RecipeData recipe)
    {
        var go   = Instantiate(Resources.Load<GameObject>(OrderPrefabPath), _orderLayout);
        var slot = go.GetComponent<UISlot>();
        if (slot == null)
        {
            slot = go.AddComponent<UISlot>();
        }
        slot.Init();
        slot.Setup(recipe);
        return slot;
    }

    // Update is called once per frame
    void Update()
    {
        //시간따라 mm:ss
        _elapsed += Time.deltaTime;

        int minutes = (int)(_elapsed / 60f);
        int seconds = (int)(_elapsed % 60f);
        TimeText.text = $"{minutes:00}:{seconds:00}";
        
        
    }
}
