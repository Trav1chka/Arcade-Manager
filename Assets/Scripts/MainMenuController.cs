using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Главное меню: фон и кнопка Play. После нажатия:
// затемнение -> другой фон, музыка и текст по кусочкам (по предложениям, без пропуска) ->
// музыка затихает -> затемнение -> загрузка игровой сцены.
public class MainMenuController : MonoBehaviour
{
    [Header("Панели")]
    [SerializeField] private GameObject menuPanel;      // фон меню + кнопка Play
    [SerializeField] private GameObject introPanel;     // другой фон + текст интро (выключена в начале)
    [SerializeField] private Button playButton;

    [Header("Текст интро")]
    [SerializeField] private Dialog introDialog;        // Dialog на тексте интро (звук букв не назначайте)
    [TextArea(5, 12)]
    [SerializeField] private string introText =
        "Dědovo herna byla kdysi nejbezpečnějším místem pro mě – mohl jsem se schovávat před krutým světem. " +
        "Jenže jednoho dne, děda onemocněl a musel do nemocnice na několik měsíců. " +
        "Teď bude na mě vést tenhle obchod a střetnout se s lidmi, před kterými jsem se tak dlouho schovával.";
    [SerializeField] private float chunkHoldTime = 3f;  // сколько держится кусочек после того, как напечатан

    [Header("Переходы")]
    [SerializeField] private ScreenFader fader;
    [SerializeField] private float fadeDuration = 2.5f; // длительность затемнений в меню, секунд
    [SerializeField] private string gameSceneName = "Game";

    private void Awake()
    {
        menuPanel.SetActive(true);
        introPanel.SetActive(false);
        playButton.onClick.AddListener(OnPlayClicked);
    }

    private void OnPlayClicked()
    {
        playButton.interactable = false;
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        // 1. Затемнение, смена фона, включение музыки
        yield return fader.FadeToBlack(fadeDuration);

        menuPanel.SetActive(false);
        introPanel.SetActive(true);
        AudioManager.PlayIntroMusic();

        yield return fader.FadeFromBlack(fadeDuration);

        // 2. Текст по кусочкам: каждый печатается по буквам, держится, затем заменяется следующим
        foreach (string chunk in SplitIntoSentences(introText))
        {
            introDialog.ShowText(chunk);
            yield return null;

            while (introDialog.IsTyping)
            {
                yield return null;
            }

            yield return new WaitForSeconds(chunkHoldTime);
        }

        // 3. Музыка заканчивается, затемнение, переход в игру
        AudioManager.StopIntroMusic();
        yield return fader.FadeToBlack(fadeDuration);

        SceneManager.LoadScene(gameSceneName);
    }

    // Делит текст на предложения по . ! ? …
    private string[] SplitIntoSentences(string text)
    {
        string[] parts = Regex.Split(text.Trim(), @"(?<=[.!?…])\s+");
        var result = new System.Collections.Generic.List<string>();

        foreach (string part in parts)
        {
            string trimmed = part.Trim();
            if (trimmed.Length > 0) result.Add(trimmed);
        }

        return result.ToArray();
    }
}
