using System.Collections;
using TMPro;
using UnityEngine;

// Чёрный экран на весь Canvas для плавных переходов.
// Вешается на объект FadePanel: растянутый на весь экран чёрный Image с CanvasGroup.
// Сам FadePanel должен быть ВКЛЮЧЁН (alpha у CanvasGroup = 0), а текст с днём внутри него скрывается скриптом.
public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text dayText;                  // надпись "Day 2" (дочерний объект панели, можно не назначать)

    [Header("Настройки")]
    [SerializeField] private float fadeDuration = 1f;           // длительность затемнения / появления по умолчанию
    [SerializeField] private float titleDuration = 2f;          // сколько держать надпись с днём
    [SerializeField] private string dayTextFormat = "Day {0}";  // формат надписи

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        if (dayText != null) dayText.gameObject.SetActive(false);
    }

    // Мгновенно делает экран чёрным (например, в начале игровой сцены)
    public void SetBlack()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    // Экран плавно становится чёрным. duration меньше 0 означает "взять значение по умолчанию"
    public IEnumerator FadeToBlack(float duration = -1f)
    {
        return Fade(1f, duration);
    }

    // Чёрный экран плавно исчезает
    public IEnumerator FadeFromBlack(float duration = -1f)
    {
        return Fade(0f, duration);
    }

    // Показывает надпись с номером дня на чёрном экране
    public IEnumerator ShowDayTitle(int dayNumber)
    {
        if (dayText != null)
        {
            dayText.text = string.Format(dayTextFormat, dayNumber);
            dayText.gameObject.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(titleDuration);

        if (dayText != null) dayText.gameObject.SetActive(false);
    }

    private IEnumerator Fade(float target, float duration)
    {
        if (duration < 0f) duration = fadeDuration;

        // Во время перехода и на полностью чёрном экране клики блокируются
        canvasGroup.blocksRaycasts = true;

        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }

        canvasGroup.alpha = target;
        canvasGroup.blocksRaycasts = target > 0.99f;
    }
}