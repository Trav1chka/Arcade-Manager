using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Отображает SocialBattery (заполняемая картинка + проценты) и деньги.
// Вешать на любой объект в Canvas (например, на панель со статистикой).
public class StatsHUD : MonoBehaviour
{
    [Header("SocialBattery")]
    [SerializeField] private Image batteryFill;     // картинка ПОЛНОЙ батарейки (лежит поверх пустой)
    [SerializeField] private TMP_Text batteryText;  // "78%"
    [SerializeField] private Image.FillMethod fillMethod = Image.FillMethod.Horizontal;
    [Tooltip("Horizontal: 0 = слева направо, 1 = справа налево. Vertical: 0 = снизу вверх, 1 = сверху вниз.")]
    [SerializeField] private int fillOrigin = 0;

    [Header("Деньги")]
    [SerializeField] private TMP_Text moneyText;    // "$100"

    private GameStats stats;

    private void Awake()
    {
        // Режим заполнения настраивается из кода, вручную в Image его менять не нужно
        if (batteryFill != null)
        {
            batteryFill.type = Image.Type.Filled;
            batteryFill.fillMethod = fillMethod;
            batteryFill.fillOrigin = fillOrigin;
        }
    }

    // Подписываемся в Start, чтобы GameStats уже успел создаться в своём Awake
    private void Start()
    {
        stats = GameStats.Instance;
        if (stats == null)
        {
            Debug.LogError("StatsHUD: на сцене нет GameStats.");
            return;
        }

        stats.OnMoneyChanged += UpdateMoney;
        stats.OnSocialBatteryChanged += UpdateBattery;

        UpdateMoney(stats.Money);
        UpdateBattery(stats.SocialBattery);
    }

    private void OnDestroy()
    {
        if (stats == null) return;

        stats.OnMoneyChanged -= UpdateMoney;
        stats.OnSocialBatteryChanged -= UpdateBattery;
    }

    private void UpdateMoney(int value)
    {
        if (moneyText != null) moneyText.text = "$" + value;
    }

    private void UpdateBattery(int value)
    {
        float percent = stats.MaxSocialBattery > 0 ? (float)value / stats.MaxSocialBattery : 0f;
        percent = Mathf.Clamp01(percent);

        if (batteryFill != null) batteryFill.fillAmount = percent;
        if (batteryText != null) batteryText.text = Mathf.RoundToInt(percent * 100f) + "%";
    }
}
