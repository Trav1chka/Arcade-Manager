using System.Collections;
using UnityEngine;

// Гость: идёт к кассе -> ждёт игрока -> диалог -> (возможно) играет в автомате -> уходит.
// На префабе: SpriteRenderer. Rigidbody2D не нужен; коллайдер, если есть, лучше с Is Trigger.
public class Guest : MonoBehaviour
{
    private enum State { Walking, Waiting, Talking, Playing, Leaving }

    [SerializeField] private DialogGroup dialogGroup;   // группа (настроение) этого персонажа
    [SerializeField] private float speed = 2f;

    [Header("Отображение в диалоге")]
    [SerializeField] private string displayName = "Гость";
    [SerializeField] private Sprite portrait;

    public DialogGroup DialogGroup => dialogGroup;
    public string DisplayName => displayName;
    public bool IsWaiting => state == State.Waiting;

    public Sprite Portrait
    {
        get
        {
            if (portrait != null) return portrait;

            // Если портрет не задан, берём картинку самого гостя
            var renderer = GetComponent<SpriteRenderer>();
            return renderer != null ? renderer.sprite : null;
        }
    }

    private DayManager day;
    private Cashier cashier;
    private SpriteRenderer sr;

    private State state;
    private bool conversationFinished;
    private bool leaveAfterDialog;

    public void Init(DayManager dayManager, Cashier cashierRef)
    {
        day = dayManager;
        cashier = cashierRef;
        sr = GetComponent<SpriteRenderer>();

        StartCoroutine(Life());
    }

    // Вызывает касса, когда игрок нажал Enter
    public void BeginConversation()
    {
        state = State.Talking;
    }

    // Вызывает касса, когда диалог закончился
    public void EndConversation(ReplyType reply)
    {
        leaveAfterDialog = reply == ReplyType.Negative
                           && Random.value < dialogGroup.leaveChanceOnNegative;
        conversationFinished = true;
    }

    private IEnumerator Life()
    {
        // 1. Идём к кассе
        state = State.Walking;
        yield return MoveTo(cashier.GuestPoint.position);

        // Если касса занята предыдущим гостем, ждём своей очереди
        while (!cashier.TryRegisterGuest(this))
        {
            yield return null;
        }

        // 2. Ждём игрока
        state = State.Waiting;
        float timer = 0f;
        float maxWait = day.CashierWaitTime;

        while (state == State.Waiting && timer < maxWait)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (state == State.Waiting)
        {
            // Игрок не успел: уходим
            cashier.ClearGuest(this);
            yield return Leave();
            yield break;
        }

        // 3. Идёт диалог
        while (!conversationFinished)
        {
            yield return null;
        }
        cashier.ClearGuest(this);

        // 4. Если не обиделись, бронируем свободный автомат (нет свободного: уходим)
        if (!leaveAfterDialog)
        {
            ArcadeMachine machine = day.TryReserveMachine();
            if (machine != null)
            {
                yield return PlayAtMachine(machine);
            }
        }

        yield return Leave();
    }

    private IEnumerator PlayAtMachine(ArcadeMachine machine)
    {
        state = State.Walking;
        yield return MoveTo(machine.StandPoint.position);

        state = State.Playing;
        float playTime = day.PlayTime;
        yield return new WaitForSeconds(playTime);

        machine.Release();

        int income = Mathf.RoundToInt(machine.ProfitPerSecond * playTime);
        if (GameStats.Instance != null)
        {
            GameStats.Instance.AddMoney(income);
        }
    }

    private IEnumerator Leave()
    {
        state = State.Leaving;
        yield return MoveTo(day.ExitPoint.position);

        day.OnGuestGone();
        Destroy(gameObject);
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        target.z = transform.position.z;

        while ((transform.position - target).sqrMagnitude > 0.0001f)
        {
            Vector3 next = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

            if (sr != null && Mathf.Abs(next.x - transform.position.x) > 0.0001f)
            {
                sr.flipX = next.x < transform.position.x;
            }

            transform.position = next;
            yield return null;
        }
    }
}