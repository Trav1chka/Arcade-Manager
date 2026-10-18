using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

// ВАЖНО: этот файл должен лежать в папке Assets/Editor (создайте её, если нет).
// Меню: Tools → Dialog Importer
public class DialogImporter : EditorWindow
{
    private DialogGroup targetGroup;
    private TextAsset sourceFile;
    private bool replaceExisting = true;
    private bool skipFirstRow = true;

    private int negBattery = -10;
    private int negMoney = 0;
    private int neutBattery = 0;
    private int neutMoney = 5;
    private int posBattery = 10;
    private int posMoney = 15;

    [MenuItem("Tools/Dialog Importer")]
    public static void Open()
    {
        GetWindow<DialogImporter>("Dialog Importer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Файл и группа", EditorStyles.boldLabel);
        targetGroup = (DialogGroup)EditorGUILayout.ObjectField("Dialog Group", targetGroup, typeof(DialogGroup), false);
        sourceFile = (TextAsset)EditorGUILayout.ObjectField("Файл с диалогами (.txt/.csv)", sourceFile, typeof(TextAsset), false);

        replaceExisting = EditorGUILayout.Toggle("Заменить старые записи", replaceExisting);
        skipFirstRow = EditorGUILayout.Toggle("Первая строка это заголовок", skipFirstRow);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Значения по умолчанию (если нет в файле)", EditorStyles.boldLabel);
        negBattery = EditorGUILayout.IntField("Негатив: SocialBattery", negBattery);
        negMoney = EditorGUILayout.IntField("Негатив: деньги", negMoney);
        neutBattery = EditorGUILayout.IntField("Нейтрал: SocialBattery", neutBattery);
        neutMoney = EditorGUILayout.IntField("Нейтрал: деньги", neutMoney);
        posBattery = EditorGUILayout.IntField("Позитив: SocialBattery", posBattery);
        posMoney = EditorGUILayout.IntField("Позитив: деньги", posMoney);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(targetGroup == null || sourceFile == null))
        {
            if (GUILayout.Button("Импортировать", GUILayout.Height(30)))
            {
                Import();
            }
        }
    }

    private void Import()
    {
        string[] lines = sourceFile.text.Split('\n');
        char separator = DetectSeparator(lines);

        var result = new List<DialogEntry>();
        int skipped = 0;
        bool firstLine = true;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (firstLine)
            {
                firstLine = false;
                if (skipFirstRow) continue;
            }

            List<string> c = SplitLine(line, separator);

            if (c.Count < 7)
            {
                skipped++;
                Debug.LogWarning("DialogImporter: строка " + (i + 1) + " пропущена, в ней меньше 7 столбцов (найдено " + c.Count + ").");
                continue;
            }

            var entry = new DialogEntry
            {
                guestPhrase = c[0],
                negative = MakeOption(c[1], c[2], c, 7, negBattery, negMoney),
                neutral = MakeOption(c[3], c[4], c, 9, neutBattery, neutMoney),
                positive = MakeOption(c[5], c[6], c, 11, posBattery, posMoney)
            };

            result.Add(entry);
        }

        Undo.RecordObject(targetGroup, "Import dialogs");

        if (replaceExisting || targetGroup.entries == null)
        {
            targetGroup.entries = result.ToArray();
        }
        else
        {
            var all = new List<DialogEntry>(targetGroup.entries);
            all.AddRange(result);
            targetGroup.entries = all.ToArray();
        }

        EditorUtility.SetDirty(targetGroup);
        AssetDatabase.SaveAssets();

        string message = "Импортировано записей: " + result.Count + "\nПропущено строк: " + skipped;
        Debug.Log("DialogImporter: " + message.Replace("\n", ", "));
        EditorUtility.DisplayDialog("Dialog Importer", message, "OK");
    }

    // Столбцы 8-13 необязательные: если их нет, берутся значения по умолчанию из окна
    private ReplyOption MakeOption(string text, string reply, List<string> cells, int extraIndex, int defBattery, int defMoney)
    {
        return new ReplyOption
        {
            text = text,
            guestReply = reply,
            socialBatteryChange = cells.Count > extraIndex ? ParseInt(cells[extraIndex], defBattery) : defBattery,
            money = cells.Count > extraIndex + 1 ? ParseInt(cells[extraIndex + 1], defMoney) : defMoney
        };
    }

    private int ParseInt(string value, int fallback)
    {
        return int.TryParse(value.Trim(), out int parsed) ? parsed : fallback;
    }

    // Табуляция (копирование таблицы из Word), иначе точка с запятой (CSV из русского Excel), иначе запятая
    private char DetectSeparator(string[] lines)
    {
        foreach (string l in lines)
        {
            if (string.IsNullOrWhiteSpace(l)) continue;
            if (l.Contains("\t")) return '\t';
            if (l.Contains(";")) return ';';
            return ',';
        }
        return '\t';
    }

    // Разбиение строки на ячейки с поддержкой ячеек в кавычках (так сохраняет Excel)
    private List<string> SplitLine(string line, char separator)
    {
        var cells = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char ch = line[i];

            if (ch == '"')
            {
                if (inQuotes)
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else if (sb.Length == 0)
                {
                    inQuotes = true;
                }
                else
                {
                    sb.Append(ch);
                }
            }
            else if (ch == separator && !inQuotes)
            {
                cells.Add(sb.ToString().Trim());
                sb.Clear();
            }
            else
            {
                sb.Append(ch);
            }
        }

        cells.Add(sb.ToString().Trim());
        return cells;
    }
}
