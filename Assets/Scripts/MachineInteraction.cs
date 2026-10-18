using UnityEngine;
using UnityEngine.InputSystem;

// Позволяет игроку забрать деньги у гостя, который доиграл и ждёт у автомата:
// подойти к нему и нажать Enter. Повесьте на любой постоянный объект сцены.
public class MachineInteraction : MonoBehaviour
{
    [SerializeField] private Hero hero;                 // если пусто, найдётся автоматически
    [SerializeField] private float interactRadius = 1.5f;  // дальность взаимодействия (в единицах мира)

    private void Start()
    {
        if (hero == null) hero = FindAnyObjectByType<Hero>();

        if (hero == null)
        {
            Debug.LogError("MachineInteraction: на сцене не найден Hero.");
        }
    }

    private void Update()
    {
        if (hero == null) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        bool enterPressed = kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame;
        if (!enterPressed) return;

        // Пока идёт диалог, герой заморожен, и забирать деньги нельзя
        if (!hero.CanMove) return;

        Guest nearest = null;
        float bestDistance = float.MaxValue;

        foreach (Guest guest in FindObjectsByType<Guest>())
        {
            if (!guest.IsReadyToCollect) continue;

            float distance = Vector2.Distance(hero.transform.position, guest.transform.position);
            if (distance <= interactRadius && distance < bestDistance)
            {
                bestDistance = distance;
                nearest = guest;
            }
        }

        if (nearest != null) nearest.Collect();
    }

    // Круг вокруг героя в Scene View, когда объект выбран (помогает подобрать радиус)
    private void OnDrawGizmosSelected()
    {
        if (hero == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(hero.transform.position, interactRadius);
    }
}