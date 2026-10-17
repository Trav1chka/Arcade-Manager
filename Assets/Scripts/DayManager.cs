using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// План для одного гостя, считается в начале дня
public class GuestPlan
{
    public bool fitsInHall;         // поместится ли гость в зале
    public ArcadeMachine machine;   // за каким автоматом будет играть
}

// Управляет одним игровым днём. Позже сюда можно подключить полноценную систему дней:
// достаточно вызывать StartDay() и подписаться на OnDayEnded.
public class DayManager : MonoBehaviour
{
    [Header("День")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private bool autoStartDay = true;

    [Header("Время (секунды)")]
    [SerializeField] private float spawnInterval = 60f;         // интервал между гостями
    [SerializeField] private float hallStayMultiplier = 2f;     // в зале гость проводит interval * это число
    [SerializeField] private float cashierWaitReserve = 10f;    // гость уходит от кассы на столько секунд раньше следующего

    [Header("Количество гостей за день")]
    [SerializeField] private int baseGuests = 3;
    [SerializeField] private int guestsPerDay = 1;              // прибавка за каждый день
    [SerializeField] private int guestsPerMachine = 2;          // прибавка за каждый автомат

    [Header("Сцена")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private Cashier cashier;
    [SerializeField] private Guest[] guestPrefabs;
    [SerializeField] private List<ArcadeMachine> machines = new List<ArcadeMachine>();

    public event Action<int> OnDayStarted;
    public event Action<int> OnDayEnded;

    public int CurrentDay => currentDay;
    public Transform ExitPoint => exitPoint;
    public float HallStayTime => spawnInterval * hallStayMultiplier;
    public float CashierWaitTime => Mathf.Max(0f, spawnInterval - cashierWaitReserve);

    private int activeGuests;
    private bool dayRunning;

    private void Start()
    {
        if (autoStartDay) StartDay();
    }

    public void StartDay()
    {
        if (dayRunning) return;
        StartCoroutine(DayRoutine());
    }

    // Для будущей покупки автоматов: вызывайте после покупки, и автомат учтётся со следующего дня
    public void RegisterMachine(ArcadeMachine machine)
    {
        if (!machines.Contains(machine)) machines.Add(machine);
    }

    public void OnGuestGone()
    {
        activeGuests--;
    }

    private int CalculateGuestCount()
    {
        return baseGuests + (currentDay - 1) * guestsPerDay + machines.Count * guestsPerMachine;
    }

    // В начале дня заранее считаем, кто поместится в зал, а кто нет.
    // Гость приходит раз в spawnInterval и занимает автомат на HallStayTime.
    private List<GuestPlan> BuildPlans(int count)
    {
        var plans = new List<GuestPlan>();
        var freeAt = new float[machines.Count];

        for (int i = 0; i < count; i++)
        {
            float arrival = i * spawnInterval;
            var plan = new GuestPlan();

            for (int m = 0; m < machines.Count; m++)
            {
                if (freeAt[m] <= arrival + 0.01f)
                {
                    plan.fitsInHall = true;
                    plan.machine = machines[m];
                    freeAt[m] = arrival + HallStayTime;
                    break;
                }
            }

            plans.Add(plan);
        }

        return plans;
    }

    private IEnumerator DayRoutine()
    {
        dayRunning = true;

        int count = CalculateGuestCount();
        List<GuestPlan> plans = BuildPlans(count);

        OnDayStarted?.Invoke(currentDay);

        for (int i = 0; i < count; i++)
        {
            SpawnGuest(plans[i]);

            if (i < count - 1)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        // День заканчивается, когда все гости ушли
        while (activeGuests > 0)
        {
            yield return null;
        }

        dayRunning = false;
        OnDayEnded?.Invoke(currentDay);
        currentDay++;
    }

    private void SpawnGuest(GuestPlan plan)
    {
        if (guestPrefabs == null || guestPrefabs.Length == 0)
        {
            Debug.LogError("DayManager: не назначены префабы гостей.");
            return;
        }

        Guest prefab = guestPrefabs[UnityEngine.Random.Range(0, guestPrefabs.Length)];
        
        Vector3 pos = spawnPoint.position;
        pos.z = 0f;
        Guest guest = Instantiate(prefab, pos, Quaternion.identity);

        activeGuests++;
        guest.Init(this, cashier, plan);
    }
}
