using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
    [SerializeField] private Text legacyText;
    [SerializeField] private TMP_Text tmpText;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip letterSound;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] private bool silentSpaces = true;

    [SerializeField] private float letterDelay = 0.05f;

    private Coroutine typingRoutine;
    private string fullText;

    public bool IsTyping { get; private set; }

    public void ShowText(string text)
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
        }

        fullText = text;
        typingRoutine = StartCoroutine(TypeRoutine(text));
    }

    public void Skip()
    {
        if (!IsTyping) return;

        StopCoroutine(typingRoutine);
        SetText(fullText);
        IsTyping = false;
    }

    private IEnumerator TypeRoutine(string text)
    {
        IsTyping = true;
        SetText(string.Empty);

        var wait = new WaitForSeconds(letterDelay);

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            SetText(text.Substring(0, i + 1));

            bool silent = silentSpaces && char.IsWhiteSpace(c);
            if (!silent)
            {
                PlayLetterSound();
            }

            yield return wait;
        }

        IsTyping = false;
    }

    private void PlayLetterSound()
    {
        if (audioSource == null || letterSound == null) return;

        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(letterSound);
    }

    private void SetText(string value)
    {
        if (legacyText != null) legacyText.text = value;
        if (tmpText != null) tmpText.text = value;
    }
}