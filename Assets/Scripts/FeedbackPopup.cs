using System.Collections;
using TMPro;
using UnityEngine;

// Всплывающее число: показывает, сколько SocialBattery даёт выбранный ответ.
// Число появляется в случайном месте в центре экрана, медленно поднимается и медленно исчезает.
// Вызов из других скриптов: FeedbackPopup.ShowSocialBattery(-3);
public class FeedbackPopup : MonoBehaviour
{
    public static FeedbackPopup Instance { get; private set; }

    [Header("Ссылки")]
    [SerializeField] private RectTransform container;   // пустой слой на весь экран внутри Canvas
    [SerializeField] private TMP_Text textPrefab;       // префаб числа (TMP Text)

    [Header("Где появляется (в пикселях Canvas, 0 = центр экрана)")]
    [SerializeField] private Vector2 center = Vector2.zero;
    [SerializeField] private Vector2 spread = new Vector2(300f, 150f);  // разброс по X и Y вокруг центра

    [Header("Анимация")]
    [SerializeField] private float duration = 2f;           // сколько живёт число, секунд
    [SerializeField] private float riseDistance = 120f;     // на сколько пикселей поднимается
    [Range(0f, 0.9f)]
    [SerializeField] private float fadeStart = 0.4f;        // с какой доли времени начинает исчезать

    [Header("Цвета")]
    [SerializeField] private Color positiveColor = new Color(0.3f, 1f, 0.4f);
    [SerializeField] private Color negativeColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color neutralColor = new Color(1f, 1f, 1f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public static void ShowSocialBattery(int change)
    {
        if (Instance != null) Instance.Spawn(change);
    }

    private void Spawn(int change)
    {
        if (textPrefab == null || container == null)
        {
            Debug.LogError("FeedbackPopup: не назначены Container или Text Prefab.");
            return;
        }

        TMP_Text text = Instantiate(textPrefab, container);
        text.raycastTarget = false;
        text.text = (change > 0 ? "+" : "") + change;
        text.color = change > 0 ? positiveColor : (change < 0 ? negativeColor : neutralColor);

        // Привязка к центру экрана, чтобы позиция не зависела от разрешения
        RectTransform rt = text.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Vector2 start = center + new Vector2(
            Random.Range(-spread.x, spread.x),
            Random.Range(-spread.y, spread.y));
        rt.anchoredPosition = start;

        StartCoroutine(Animate(text, rt, start));
    }

    private IEnumerator Animate(TMP_Text text, RectTransform rt, Vector2 start)
    {
        Color baseColor = text.color;
        float time = 0f;

        while (time < duration)
        {
            if (text == null) yield break;

            time += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(time / duration);

            // Подъём замедляется к концу (плавное затухание движения)
            float eased = 1f - (1f - p) * (1f - p);
            rt.anchoredPosition = start + Vector2.up * (riseDistance * eased);

            // Первое время число полностью видно, затем плавно исчезает
            float alpha = p < fadeStart ? 1f : 1f - (p - fadeStart) / (1f - fadeStart);
            Color c = baseColor;
            c.a = Mathf.Clamp01(alpha);
            text.color = c;

            yield return null;
        }

        if (text != null) Destroy(text.gameObject);
    }
}
