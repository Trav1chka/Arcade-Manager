using System.Collections;
using TMPro;
using UnityEngine;

// Чёрный экран на весь Canvas для плавных переходов.
// Вешается на объект FadePanel: растянутый на весь экран чёрный Image с CanvasGroup.
// Сам FadePanel должен быть ВКЛЮЧЁН (alpha у CanvasGroup = 0), а текст с днём внутри него скрывается скриптом.
public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text dayText;                  // надпись "Day 2" (дочерний объект панели)

    [Header("Настройки")]
    [SerializeField] private float fadeDuration = 1f;           // длительность затемнения / появления
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

    // Экран плавно становится чёрным
    public IEnumerator FadeToBlack()
    {
        return Fade(1f);
    }

    // Чёрный экран плавно исчезает
    public IEnumerator FadeFromBlack()
    {
        return Fade(0f);
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

    private IEnumerator Fade(float target)
    {
        // Во время перехода и на полностью чёрном экране клики блокируются
        canvasGroup.blocksRaycasts = true;

        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
        canvasGroup.blocksRaycasts = target > 0.99f;
    }
}
