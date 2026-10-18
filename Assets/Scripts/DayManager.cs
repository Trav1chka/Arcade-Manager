using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Управляет игровыми днями.
// В начале дня считает, сколько гостей придёт, исходя из числа купленных автоматов.
// После конца дня вызывает OnDayEnded (по нему открывается магазин).
// Новый день начинается вызовом StartNextDay() (кнопка в магазине).
public class DayManager : MonoBehaviour
{
    [Header("День")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private bool autoStartDay = true;
    [SerializeField] private int startingMachines = 1;      // сколько автоматов куплено в начале игры

    [Header("Тайминги (секунды)")]
    [SerializeField] private float dayDuration = 300f;      // длина дня, в течение которой приходят гости
    [SerializeField] private float playTime = 30f;          // сколько гость играет в автомате
    [Range(0.1f, 1f)]
    [SerializeField] private float machineBusyFraction = 0.5f;  // какую часть дня каждый автомат занят
    [SerializeField] private float cashierWaitReserve = 10f;    // гость уходит от кассы на столько раньше следующего
    [SerializeField] private float minCashierWait = 5f;         // но не быстрее этого

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
    public float PlayTime => playTime;
    public float CashierWaitTime => Mathf.Max(minCashierWait, currentInterval - cashierWaitReserve);

    public IReadOnlyList<ArcadeMachine> Machines => machines;
    public int TotalMachines => machines.Count;
    public int PurchasedCount
    {
        get
        {
            int n = 0;
            foreach (var m in machines) if (m.IsPurchased) n++;
            return n;
        }
    }
    public bool HasLockedMachine
    {
        get
        {
            foreach (var m in machines) if (!m.IsPurchased) return true;
            return false;
        }
    }

    private float currentInterval = 60f;
    private int activeGuests;
    private bool dayRunning;

    private void Start()
    {
        // Первые startingMachines автоматов куплены, остальные выключены
        for (int i = 0; i < machines.Count; i++)
        {
            machines[i].SetPurchased(i < startingMachines);
        }

        if (autoStartDay) StartDay();
    }

    public void StartDay()
    {
        if (dayRunning) return;
        StartCoroutine(DayRoutine());
    }

    // Вызывается кнопкой "Следующий день" в магазине
    public void StartNextDay()
    {
        if (dayRunning) return;
        currentDay++;
        StartDay();
    }

    // Покупка: включает следующий непокупленный автомат
    public bool UnlockNextMachine()
    {
        foreach (var m in machines)
        {
            if (!m.IsPurchased)
            {
                m.SetPurchased(true);
                return true;
            }
        }
        return false;
    }

    // Гость вызывает это после диалога. Возвращает забронированный свободный автомат или null.
    public ArcadeMachine TryReserveMachine()
    {
        var free = new List<ArcadeMachine>();
        foreach (var m in machines)
        {
            if (m.IsPurchased && !m.IsBusy) free.Add(m);
        }

        if (free.Count == 0) return null;

        ArcadeMachine chosen = free[UnityEngine.Random.Range(0, free.Count)];
        chosen.Reserve();
        return chosen;
    }

    public void OnGuestGone()
    {
        activeGuests--;
    }

    // Гостей столько, чтобы каждый автомат был занят machineBusyFraction от длины дня
    private int CalculateGuestCount(int machineCount)
    {
        float count = machineCount * dayDuration * machineBusyFraction / playTime;
        return Mathf.Max(1, Mathf.RoundToInt(count));
    }

    private IEnumerator DayRoutine()
    {
        dayRunning = true;

        int machineCount = PurchasedCount;
        if (machineCount == 0)
        {
            Debug.LogError("DayManager: нет купленных автоматов, день не начат.");
            dayRunning = false;
            yield break;
        }

        int count = CalculateGuestCount(machineCount);
        currentInterval = dayDuration / count;

        Debug.Log("День " + currentDay + ": автоматов " + machineCount + ", гостей " + count +
                  ", интервал " + currentInterval.ToString("0.0") + " с");

        OnDayStarted?.Invoke(currentDay);

        for (int i = 0; i < count; i++)
        {
            SpawnGuest();

            if (i < count - 1)
            {
                yield return new WaitForSeconds(currentInterval);
            }
        }

        // День заканчивается, когда все гости ушли
        while (activeGuests > 0)
        {
            yield return null;
        }

        dayRunning = false;
        OnDayEnded?.Invoke(currentDay);
    }

    private void SpawnGuest()
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
        guest.Init(this, cashier);
    }
}