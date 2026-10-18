using UnityEngine;
using UnityEngine.InputSystem;

// Касса. На объекте нужен Collider2D с включённым Is Trigger (зона перед гостем).
// Герою нужен тег Player.
public class Cashier : MonoBehaviour
{
    [SerializeField] private Transform guestPoint;              // где стоит гость
    [SerializeField] private DialogController dialogController;
    [SerializeField] private GameObject enterPrompt;            // необязательно: подсказка "Нажмите Enter"
    [SerializeField] private string playerTag = "Player";

    private Guest currentGuest;
    private Hero heroInZone;
    private bool dialogActive;

    public Transform GuestPoint => guestPoint;

    public bool TryRegisterGuest(Guest guest)
    {
        if (currentGuest != null) return false;
        currentGuest = guest;
        return true;
    }

    public void ClearGuest(Guest guest)
    {
        if (currentGuest == guest) currentGuest = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        heroInZone = other.GetComponentInParent<Hero>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        heroInZone = null;
    }

    private void Update()
    {
        bool canTalk = heroInZone != null
                       && currentGuest != null
                       && currentGuest.IsWaiting
                       && !dialogActive;

        if (enterPrompt != null) enterPrompt.SetActive(canTalk);
        if (!canTalk) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
        {
            StartDialog();
        }
    }

    private void StartDialog()
    {
        Guest guest = currentGuest;
        Hero hero = heroInZone;

        dialogActive = true;
        guest.BeginConversation();
        hero.CanMove = false;

        dialogController.StartConversation(guest.DialogGroup, guest.DisplayName, guest.Portrait, reply =>
        {
            hero.CanMove = true;
            dialogActive = false;
            guest.EndConversation(reply);
        });
    }
}
