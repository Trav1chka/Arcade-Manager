using System.Collections;
using UnityEngine;

// Гость: идёт к кассе -> ждёт игрока -> диалог -> (возможно) играет в автомате -> уходит.
// На префабе: SpriteRenderer. Rigidbody2D не нужен; коллайдер, если есть, лучше с Is Trigger.
public class Guest : MonoBehaviour
{
    private enum State { Walking, Waiting, Talking, Playing, Leaving }

    [SerializeField] private DialogGroup dialogGroup;   // группа (настроение) этого персонажа
    [SerializeField] private float speed = 2f;

    public DialogGroup DialogGroup => dialogGroup;
    public bool IsWaiting => state == State.Waiting;

    private DayManager day;
    private Cashier cashier;
    private GuestPlan plan;
    private SpriteRenderer sr;

    private State state;
    private bool conversationFinished;
    private bool leaveAfterDialog;

    public void Init(DayManager dayManager, Cashier cashierRef, GuestPlan guestPlan)
    {
        day = dayManager;
        cashier = cashierRef;
        plan = guestPlan;
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

        // 4. Играем в автомате, если не обиделись и нам хватило места
        if (!leaveAfterDialog && plan.fitsInHall && plan.machine != null)
        {
            yield return PlayAtMachine();
        }

        yield return Leave();
    }

    private IEnumerator PlayAtMachine()
    {
        ArcadeMachine machine = plan.machine;

        state = State.Walking;
        yield return MoveTo(machine.StandPoint.position);

        while (machine.IsOccupied)
        {
            yield return null;
        }

        machine.Occupy();
        state = State.Playing;

        float playTime = day.HallStayTime;
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
