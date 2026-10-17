using UnityEngine;

// Игровой автомат. Доход гостя = profitPerSecond * время игры.
public class ArcadeMachine : MonoBehaviour
{
    [SerializeField] private Transform standPoint;          // где стоит гость (если пусто, позиция автомата)
    [SerializeField] private float profitPerSecond = 0.5f;  // прибыльность

    public bool IsOccupied { get; private set; }
    public float ProfitPerSecond => profitPerSecond;
    public Transform StandPoint => standPoint != null ? standPoint : transform;

    public void Occupy() => IsOccupied = true;
    public void Release() => IsOccupied = false;
}
