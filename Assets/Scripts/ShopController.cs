using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Магазин: открывается после конца дня. Показывает список всех автоматов,
// можно купить один автомат в день и начать следующий день.
// Повесьте на ПОСТОЯННО активный объект (не на shopPanel, которая скрывается).
public class ShopController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private DayManager day;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button nextDayButton;
    [SerializeField] private TMP_Text infoText;             // день, деньги, SocialBattery

    [Header("Список автоматов")]
    [SerializeField] private Transform itemsContainer;      // сюда создаются строки
    [SerializeField] private ShopItemUI itemPrefab;         // префаб строки

    [Header("Покупка")]
    [SerializeField] private int socialBatteryBonus = 30;   // награда за покупку (максимум 100 держит GameStats)

    private readonly List<ShopItemUI> items = new List<ShopItemUI>();
    private bool boughtToday;

    private void Awake()
    {
        nextDayButton.onClick.AddListener(OnNextDayClicked);
        BuildItems();
        shopPanel.SetActive(false);
    }

    private void OnEnable()
    {
        day.OnDayEnded += HandleDayEnded;
        day.OnDayStarted += HandleDayStarted;
    }

    private void OnDisable()
    {
        day.OnDayEnded -= HandleDayEnded;
        day.OnDayStarted -= HandleDayStarted;
    }

    // Создаёт по одной строке на каждый автомат из списка DayManager
    private void BuildItems()
    {
        foreach (ArcadeMachine machine in day.Machines)
        {
            ShopItemUI item = Instantiate(itemPrefab, itemsContainer);
            item.Setup(machine, OnBuyClicked);
            items.Add(item);
        }
    }

    private void HandleDayEnded(int endedDay)
    {
        shopPanel.SetActive(true);
        Refresh();
    }

    private void HandleDayStarted(int startedDay)
    {
        boughtToday = false;
        shopPanel.SetActive(false);
    }

    private void Refresh()
    {
        GameStats stats = GameStats.Instance;
        int money = stats != null ? stats.Money : 0;
        int battery = stats != null ? stats.SocialBattery : 0;

        if (infoText != null)
        {
            string note = boughtToday ? "\nСегодня автомат уже куплен" : "\nМожно купить один автомат";
            infoText.text = "День " + day.CurrentDay + " завершён\n" +
                            "Деньги: " + money + "\n" +
                            "SocialBattery: " + battery + "%" + note;
        }

        for (int i = 0; i < items.Count; i++)
        {
            ArcadeMachine machine = day.Machines[i];
            bool canBuy = !boughtToday && !machine.IsPurchased && money >= machine.Price;
            items[i].Refresh(machine, canBuy);
        }
    }

    private void OnBuyClicked(ArcadeMachine machine)
    {
        if (boughtToday || machine.IsPurchased) return;

        GameStats stats = GameStats.Instance;
        if (stats == null) return;

        if (!stats.TrySpendMoney(machine.Price)) return;

        machine.SetPurchased(true);
        stats.AddSocialBattery(socialBatteryBonus);
        boughtToday = true;

        Refresh();
    }

    private void OnNextDayClicked()
    {
        shopPanel.SetActive(false);
        day.StartNextDay();
    }
}