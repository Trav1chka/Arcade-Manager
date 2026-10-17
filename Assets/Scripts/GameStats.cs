using System;
using UnityEngine;
using TMPro;

// Глобальные значения игры: деньги и SocialBattery.
// Повесьте на один постоянный объект сцены (например, GameManager).
public class GameStats : MonoBehaviour
{
    public static GameStats Instance { get; private set; }

    [SerializeField] private int money = 0;
    [SerializeField] private int maxSocialBattery = 100;
    [SerializeField] private int socialBattery = 100;
    [SerializeField] private TMP_Text tmpTextMoney;
    [SerializeField] private TMP_Text tmpTextBattery;

    public int Money => money;
    public int SocialBattery => socialBattery;
    public int MaxSocialBattery => maxSocialBattery;

    // На эти события потом подпишется интерфейс (полоска батареи, счётчик денег)
    public event Action<int> OnMoneyChanged;
    public event Action<int> OnSocialBatteryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        tmpTextMoney.text = money.ToString();
        tmpTextBattery.text = socialBattery.ToString();
    }

    public void AddMoney(int amount)
    {
        money += amount;
        OnMoneyChanged?.Invoke(money);
    }

    public bool TrySpendMoney(int amount)
    {
        if (money < amount) return false;
        money -= amount;
        OnMoneyChanged?.Invoke(money);
        return true;
    }

    public void AddSocialBattery(int amount)
    {
        socialBattery = Mathf.Clamp(socialBattery + amount, 0, maxSocialBattery);
        OnSocialBatteryChanged?.Invoke(socialBattery);
    }
}
