using UnityEngine;

// Туториал первого дня.
// Когда первый гость дошёл до кассы и ждёт, вокруг зоны диалога мигает рамка.
// Когда игрок вошёл в зону, рамка пропадает и появляется подсказка "Нажмите Enter".
// Как только диалог начался, туториал завершается (больше не показывается).
public class TutorialController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private DayManager day;
    [SerializeField] private Cashier cashier;           // на этом объекте должен быть Collider2D зоны диалога
    [SerializeField] private GameObject enterHint;      // подсказка "Нажмите Enter" (например, TMP Text в Canvas)

    [Header("Рамка зоны диалога")]
    [SerializeField] private Color outlineColor = new Color(1f, 0.9f, 0.1f, 1f);
    [Range(0.01f, 0.2f)]
    [SerializeField] private float thicknessRatio = 0.04f;  // толщина линии как доля меньшей стороны зоны
    [SerializeField] private float blinkSpeed = 2.5f;       // скорость мигания
    [Range(0f, 1f)]
    [SerializeField] private float minAlpha = 0.1f;         // минимальная яркость в момент "погасания"
    [SerializeField] private int sortingOrder = 100;        // чтобы рамка была поверх спрайтов
    [SerializeField] private Material lineMaterial;         // необязательно: если пусто, берётся Sprites/Default

    private Collider2D zone;
    private LineRenderer outline;
    private bool armed;        // туториал сейчас активен
    private bool completed;    // туториал уже пройден

    private void OnEnable()
    {
        if (day != null) day.OnDayStarted += HandleDayStarted;
    }

    private void OnDisable()
    {
        if (day != null) day.OnDayStarted -= HandleDayStarted;
    }

    private void Start()
    {
        zone = cashier.GetComponent<Collider2D>();
        if (zone == null)
        {
            Debug.LogError("TutorialController: на объекте кассы нет Collider2D зоны диалога.");
        }

        outline = CreateOutline();
        SetVisible(false, false);
    }

    // Туториал включается только в первый день
    private void HandleDayStarted(int dayNumber)
    {
        armed = dayNumber == 1 && !completed;

        if (!armed) SetVisible(false, false);
    }

    private void Update()
    {
        if (!armed || zone == null) return;

        // Диалог начался: туториал выполнен
        if (cashier.IsDialogActive)
        {
            completed = true;
            armed = false;
            SetVisible(false, false);
            return;
        }

        // Гостя у кассы нет (ещё не пришёл или ушёл): ничего не показываем
        if (!cashier.HasWaitingGuest)
        {
            SetVisible(false, false);
            return;
        }

        if (cashier.IsHeroInZone)
        {
            SetVisible(false, true);    // игрок в зоне: подсказка про Enter
        }
        else
        {
            SetVisible(true, false);    // игрок далеко: мигающая рамка
            Blink();
        }
    }

    private void SetVisible(bool showOutline, bool showHint)
    {
        if (outline != null)
        {
            if (showOutline && !outline.enabled) UpdateOutlinePoints();
            outline.enabled = showOutline;
        }

        if (enterHint != null) enterHint.SetActive(showHint);
    }

    private void Blink()
    {
        float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);

        Color c = outlineColor;
        c.a = Mathf.Lerp(minAlpha, 1f, t);

        outline.startColor = c;
        outline.endColor = c;
    }

    // Рамка по периметру границ коллайдера зоны
    private void UpdateOutlinePoints()
    {
        Bounds b = zone.bounds;
        float z = b.center.z;

        outline.SetPosition(0, new Vector3(b.min.x, b.min.y, z));
        outline.SetPosition(1, new Vector3(b.max.x, b.min.y, z));
        outline.SetPosition(2, new Vector3(b.max.x, b.max.y, z));
        outline.SetPosition(3, new Vector3(b.min.x, b.max.y, z));

        float thickness = Mathf.Min(b.size.x, b.size.y) * thicknessRatio;
        outline.startWidth = thickness;
        outline.endWidth = thickness;
    }

    private LineRenderer CreateOutline()
    {
        var go = new GameObject("TutorialOutline");
        go.transform.SetParent(transform, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = 4;
        lr.sortingOrder = sortingOrder;

        if (lineMaterial != null)
        {
            lr.material = lineMaterial;
        }
        else
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                lr.material = new Material(shader);
            }
            else
            {
                Debug.LogError("TutorialController: не найден шейдер Sprites/Default, назначьте Line Material.");
            }
        }

        lr.enabled = false;
        return lr;
    }
}
