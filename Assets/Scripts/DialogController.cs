using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Проводит диалог: реплика гостя -> 3 ответа в случайном порядке -> реакция гостя.
// Вешать на ПОСТОЯННО активный объект (не на panel, которая скрывается).
public class DialogController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;          // вся панель диалога
    [SerializeField] private Dialog dialog;             // ваш скрипт с печатью по буквам
    [SerializeField] private Button[] answerButtons;    // ровно 3 кнопки

    [Header("Собеседник")]
    [SerializeField] private Image portraitImage;       // UI Image для портрета
    [SerializeField] private Text nameLegacyText;       // заполните один из двух
    [SerializeField] private TMP_Text nameTmpText;

    [Header("Настройки")]
    [SerializeField] private float replyHoldTime = 2f;  // сколько держать ответ гостя на экране

    private int chosenIndex = -1;

    public bool IsActive { get; private set; }

    private void Awake()
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;
            answerButtons[i].onClick.AddListener(() => chosenIndex = index);
        }

        SetButtonsVisible(false);
        if (panel != null) panel.SetActive(false);
    }

    public void StartConversation(DialogGroup group, string speakerName, Sprite speakerPortrait, Action<ReplyType> onFinished)
    {
        if (IsActive) return;

        DialogEntry entry = group != null ? group.GetRandomEntry() : null;
        if (entry == null)
        {
            Debug.LogError("DialogController: у гостя нет DialogGroup или в ней нет реплик.");
            onFinished?.Invoke(ReplyType.Neutral);
            return;
        }

        StartCoroutine(ConversationRoutine(entry, speakerName, speakerPortrait, onFinished));
    }

    private IEnumerator ConversationRoutine(DialogEntry entry, string speakerName, Sprite speakerPortrait, Action<ReplyType> onFinished)
    {
        IsActive = true;
        panel.SetActive(true);
        SetSpeaker(speakerName, speakerPortrait);
        SetButtonsVisible(false);

        // Пропускаем кадр, чтобы Enter, начавший диалог, не промотал текст
        yield return null;

        // 1. Реплика гостя
        yield return Say(entry.guestPhrase);

        // 2. Три ответа в случайном порядке
        var options = new List<(ReplyOption option, ReplyType type)>
        {
            (entry.negative, ReplyType.Negative),
            (entry.neutral, ReplyType.Neutral),
            (entry.positive, ReplyType.Positive)
        };
        Shuffle(options);

        for (int i = 0; i < answerButtons.Length && i < options.Count; i++)
        {
            SetButtonText(answerButtons[i], (i + 1) + ". " + options[i].option.text);
        }

        SetButtonsVisible(true);
        chosenIndex = -1;

        while (chosenIndex < 0 || chosenIndex >= options.Count)
        {
            ReadNumberKeys();
            yield return null;
        }

        SetButtonsVisible(false);

        // 3. Эффект ответа
        var chosen = options[chosenIndex];
        AudioManager.PlayReply(chosen.type);
        FeedbackPopup.ShowSocialBattery(chosen.option.socialBatteryChange);
        if (GameStats.Instance != null)
        {
            GameStats.Instance.AddSocialBattery(chosen.option.socialBatteryChange);
            GameStats.Instance.AddMoney(chosen.option.money);
        }

        // 4. Реакция гостя
        yield return Say(chosen.option.guestReply);

        float t = 0f;
        while (t < replyHoldTime && !EnterPressed())
        {
            t += Time.deltaTime;
            yield return null;
        }

        panel.SetActive(false);
        IsActive = false;
        onFinished?.Invoke(chosen.type);
    }

    // Подставляет имя и портрет собеседника в панель
    private void SetSpeaker(string speakerName, Sprite portrait)
    {
        if (nameLegacyText != null) nameLegacyText.text = speakerName;
        if (nameTmpText != null) nameTmpText.text = speakerName;

        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.preserveAspect = true;
            portraitImage.enabled = portrait != null;
        }
    }

    // Печатает текст по буквам, Enter пропускает печать
    private IEnumerator Say(string text)
    {
        dialog.ShowText(text);
        yield return null;

        while (dialog.IsTyping)
        {
            if (EnterPressed()) dialog.Skip();
            yield return null;
        }
    }

    private void ReadNumberKeys()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame) chosenIndex = 0;
        else if (kb.digit2Key.wasPressedThisFrame) chosenIndex = 1;
        else if (kb.digit3Key.wasPressedThisFrame) chosenIndex = 2;
    }

    private bool EnterPressed()
    {
        var kb = Keyboard.current;
        return kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame);
    }

    private void SetButtonsVisible(bool visible)
    {
        foreach (var b in answerButtons)
        {
            b.gameObject.SetActive(visible);
        }
    }

    // Работает и с TextMeshPro, и с обычным Text внутри кнопки
    private void SetButtonText(Button button, string value)
    {
        var tmp = button.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = value;
            return;
        }

        var legacy = button.GetComponentInChildren<Text>(true);
        if (legacy != null) legacy.text = value;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}