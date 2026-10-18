using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Магазин: открывается после конца дня (через затемнение). Показывает список всех автоматов,
// можно купить один автомат в день и начать следующий день.
// Повесьте на ПОСТОЯННО активный объект (не на shopPanel, которая скрывается).
public class ShopController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private DayManager day;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button nextDayButton;
    [SerializeField] private ScreenFader fader;             // чёрный экран для переходов

    [Header("Список автоматов")]
    [SerializeField] private Transform itemsContainer;      // сюда создаются строки (или лежат готовые карточки)
    [SerializeField] private ShopItemUI itemPrefab;         // префаб строки

    [Header("Покупка")]
    [SerializeField] private int socialBatteryBonus = 30;   // награда за покупку (максимум 100 держит GameStats)

    private readonly List<ShopItemUI> items = new List<ShopItemUI>();
    private bool boughtToday;
    private bool transitioning;

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

    // Если в Items Container уже лежат карточки с ShopItemUI, привязываем их к автоматам по порядку.
    // Если карточек нет, создаём их из Item Prefab.
    private void BuildItems()
    {
        if (itemsContainer == null)
        {
            Debug.LogError("ShopController: не назначен Items Container.");
            return;
        }

        IReadOnlyList<ArcadeMachine> machines = day.Machines;
        ShopItemUI[] existing = itemsContainer.GetComponentsInChildren<ShopItemUI>(true);

        if (existing.Length > 0)
        {
            if (existing.Length != machines.Count)
            {
                Debug.LogWarning("ShopController: карточек " + existing.Length + ", а автоматов в списке " + machines.Count + ".");
            }

            int count = Mathf.Min(existing.Length, machines.Count);
            for (int i = 0; i < count; i++)
            {
                existing[i].Setup(machines[i], OnBuyClicked);
                items.Add(existing[i]);
            }
            return;
        }

        if (itemPrefab == null)
        {
            Debug.LogError("ShopController: в Items Container нет карточек с ShopItemUI и не назначен Item Prefab.");
            return;
        }

        foreach (ArcadeMachine machine in machines)
        {
            ShopItemUI item = Instantiate(itemPrefab, itemsContainer);
            item.Setup(machine, OnBuyClicked);
            items.Add(item);
        }
    }

    private void HandleDayEnded(int endedDay)
    {
        StartCoroutine(OpenShopRoutine());
    }

    private void HandleDayStarted(int startedDay)
    {
        boughtToday = false;
        shopPanel.SetActive(false);
    }

    // День закончился: экран чернеет, под чёрным включается магазин, экран проясняется
    private IEnumerator OpenShopRoutine()
    {
        transitioning = true;

        if (fader != null) yield return fader.FadeToBlack();

        shopPanel.SetActive(true);
        Refresh();
        AudioManager.PlayShopMusic();

        if (fader != null) yield return fader.FadeFromBlack();

        transitioning = false;
    }

    // Следующий день: экран чернеет, показывается "Day N", день стартует, экран проясняется
    private IEnumerator NextDayRoutine()
    {
        transitioning = true;
        AudioManager.PlayNextDay();
        AudioManager.StopShopMusic();

        if (fader != null) yield return fader.FadeToBlack();

        shopPanel.SetActive(false);

        if (fader != null) yield return fader.ShowDayTitle(day.CurrentDay + 1);

        day.StartNextDay();

        if (fader != null) yield return fader.FadeFromBlack();

        transitioning = false;
    }

    private void Refresh()
    {
        GameStats stats = GameStats.Instance;
        int money = stats != null ? stats.Money : 0;

        for (int i = 0; i < items.Count; i++)
        {
            ArcadeMachine machine = day.Machines[i];
            bool canBuy = !boughtToday && !machine.IsPurchased && money >= machine.Price;
            items[i].Refresh(machine, canBuy);
        }
    }

    private void OnBuyClicked(ArcadeMachine machine)
    {
        if (transitioning) return;

        if (boughtToday)
        {
            Debug.Log("Покупка отклонена: сегодня уже куплен автомат");
            return;
        }

        if (machine.IsPurchased)
        {
            Debug.Log("Покупка отклонена: этот автомат уже куплен");
            return;
        }

        GameStats stats = GameStats.Instance;
        if (stats == null)
        {
            Debug.LogError("Покупка отклонена: на сцене нет GameStats");
            return;
        }

        if (!stats.TrySpendMoney(machine.Price))
        {
            Debug.Log("Покупка отклонена: не хватает денег (есть " + stats.Money + ")");
            return;
        }

        machine.SetPurchased(true);
        AudioManager.PlayMachinePurchased();
        stats.AddSocialBattery(socialBatteryBonus);
        boughtToday = true;

        Refresh();
    }

    private void OnNextDayClicked()
    {
        if (transitioning) return;
        StartCoroutine(NextDayRoutine());
    }
}