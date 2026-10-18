using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Одна строка в магазине (один автомат). Вешается на префаб строки.
public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;     // название автомата
    [SerializeField] private TMP_Text infoText;     // прибыль
    [SerializeField] private TMP_Text priceText;    // цена или "Куплено"
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text buttonLabel;  // необязательно: надпись на кнопке

    public void Setup(ArcadeMachine machine, Action<ArcadeMachine> onBuy)
    {
        if (nameText != null) nameText.text = machine.DisplayName;
        if (infoText != null) infoText.text = "Прибыль: " + machine.ProfitPerSecond.ToString("0.##") + "/с";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => onBuy(machine));
    }

    public void Refresh(ArcadeMachine machine, bool canBuy)
    {
        if (machine.IsPurchased)
        {
            if (priceText != null) priceText.text = "Куплено";
            if (buttonLabel != null) buttonLabel.text = "Куплено";
            buyButton.interactable = false;
        }
        else
        {
            if (priceText != null) priceText.text = "Цена: " + machine.Price;
            if (buttonLabel != null) buttonLabel.text = "Купить";
            buyButton.interactable = canBuy;
        }
    }
}
