using System;
using UnityEngine;

public enum ReplyType
{
    Negative,
    Neutral,
    Positive
}

// Один вариант ответа игрока и реакция гостя на него
[Serializable]
public class ReplyOption
{
    [TextArea] public string text;          // что говорит игрок
    [TextArea] public string guestReply;    // что отвечает гость
    public int socialBatteryChange;         // + или - к SocialBattery
    public int money;                       // сколько денег даёт ответ
}

// Одна реплика гостя + три ответа (всегда ровно по одному каждого типа)
[Serializable]
public class DialogEntry
{
    [TextArea] public string guestPhrase;
    public ReplyOption negative;
    public ReplyOption neutral;
    public ReplyOption positive;
}

// Группа гостей по настроению. Создаётся через ПКМ в Project:
// Create → Game → Dialog Group
[CreateAssetMenu(menuName = "Game/Dialog Group", fileName = "NewDialogGroup")]
public class DialogGroup : ScriptableObject
{
    public string groupName;

    [Range(0f, 1f)]
    [Tooltip("Шанс, что гость уйдёт сразу после негативного ответа")]
    public float leaveChanceOnNegative = 0.5f;

    public DialogEntry[] entries;

    public DialogEntry GetRandomEntry()
    {
        if (entries == null || entries.Length == 0) return null;
        return entries[UnityEngine.Random.Range(0, entries.Length)];
    }
}
