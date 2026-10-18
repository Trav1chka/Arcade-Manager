using UnityEngine;

// Игровой автомат. Доход гостя = profitPerSecond * время игры.
// Непокупленные автоматы выключаются (gameObject неактивен) и гости к ним не ходят.
public class ArcadeMachine : MonoBehaviour
{
    [Header("Магазин")]
    [SerializeField] private string displayName = "Автомат";
    [SerializeField] private int price = 100;

    [Header("Работа")]
    [SerializeField] private Transform standPoint;          // где стоит гость (если пусто, позиция автомата)
    [SerializeField] private float profitPerSecond = 0.5f;  // прибыльность

    public string DisplayName => displayName;
    public int Price => price;

    public bool IsPurchased { get; private set; }
    public bool IsBusy { get; private set; }               // занят или забронирован гостем
    public float ProfitPerSecond => profitPerSecond;
    public Transform StandPoint => standPoint != null ? standPoint : transform;

    public void SetPurchased(bool purchased)
    {
        IsPurchased = purchased;
        IsBusy = false;
        gameObject.SetActive(purchased);
    }

    public void Reserve() => IsBusy = true;
    public void Release() => IsBusy = false;
}