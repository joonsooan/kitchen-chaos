using UnityEngine;

public class RandomBoxManager : KSingleton<RandomBoxManager>
{
    [SerializeField] private int cost = 100;

    public bool TryOpen()
    {
        if (GameManager.Instance.Money < cost) return false;

        GameManager.Instance.AddMoney(-cost);
        OnBoxOpened();
        return true;
    }

    private void OnBoxOpened()
    {
        // TODO: 랜덤박스 보상 로직
        Debug.Log("RandomBox opened");
    }
}
