using System.Collections.Generic;
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

<<<<<<< HEAD
    private const string OrderPrefabPath = "UI/Slot/OrderSlot";

    private TextMeshProUGUI timeText;
    private Transform orderLayout;
    private float elapsed;
=======
    private const string OrderPrefabPath = "UI/Slot/Order";

    private TextMeshProUGUI TimeText;
    private Transform _orderLayout;
    private float _elapsed;
>>>>>>> 75ee4b2 (add: 인게임 HUD 모양잡기)

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Init();
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

<<<<<<< HEAD
        timeText     = Get<TextMeshProUGUI>((int)Texts.TimeText);
        orderLayout = Get<GameObject>((int)GameObjects.OrderLayout).transform;
    }

    private void OnEnable()
    {
        Customer.OnAnyCustomerSeated += HandleCustomerSeated;
        Customer.OnAnyCustomerLeft += HandleCustomerLeft;
    }

    private void OnDisable()
    {
        Customer.OnAnyCustomerSeated -= HandleCustomerSeated;
        Customer.OnAnyCustomerLeft -= HandleCustomerLeft;
    }

    // 손님이 착석해 주문을 넣는 시점 — 주문 슬롯 추가
    private void HandleCustomerSeated(Customer customer)
    {
        if (activeSlots.ContainsKey(customer)) return;
        activeSlots[customer] = AddOrder(customer.CustomerData);
    }

    // 손님이 퇴장하는 시점(성공/실패 모두) — 해당 주문 슬롯 제거
    private void HandleCustomerLeft(Customer customer)
    {
        if (!activeSlots.TryGetValue(customer, out UISlot slot)) return;

        activeSlots.Remove(customer);
        if (slot != null) Destroy(slot.gameObject);
    }

    // 주문 1개 추가 — 게임 로직에서 호출
    public UISlot AddOrder(CustomerData customer)
    {
        var go   = Instantiate(Resources.Load<GameObject>(OrderPrefabPath), orderLayout);
=======
        TimeText     = Get<TextMeshProUGUI>((int)Texts.TimeText);
        _orderLayout = Get<GameObject>((int)GameObjects.OrderLayout).transform;
    }

    // 주문 1개 추가 — 게임 로직에서 호출
    public UISlot AddOrder(RecipeData recipe)
    {
        var go   = Instantiate(Resources.Load<GameObject>(OrderPrefabPath), _orderLayout);
>>>>>>> 75ee4b2 (add: 인게임 HUD 모양잡기)
        var slot = go.GetComponent<UISlot>();
        if (slot == null)
        {
            slot = go.AddComponent<UISlot>();
        }
        slot.Init();
<<<<<<< HEAD
        slot.Setup(customer);
=======
        slot.Setup(recipe);
>>>>>>> 75ee4b2 (add: 인게임 HUD 모양잡기)
        return slot;
    }

    // Update is called once per frame
    void Update()
    {
        //시간따라 mm:ss
<<<<<<< HEAD
        elapsed += Time.deltaTime;

        int minutes = (int)(elapsed / 60f);
        int seconds = (int)(elapsed % 60f);
        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    // 풀에서 랜덤 손님 1명으로 주문 추가 (test용도)
    public void AddOrder()
    {
        var customers = DataTable.Customers;
        if (customers == null || customers.Length == 0) return;

        var customer = customers[Random.Range(0, customers.Length)];
        AddOrder(customer);
=======
        _elapsed += Time.deltaTime;

        int minutes = (int)(_elapsed / 60f);
        int seconds = (int)(_elapsed % 60f);
        TimeText.text = $"{minutes:00}:{seconds:00}";
        
        
>>>>>>> 75ee4b2 (add: 인게임 HUD 모양잡기)
    }
}
