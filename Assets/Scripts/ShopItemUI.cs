using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Одна карточка в магазине (один автомат): иконка, цена, кнопка.
// Вешается на префаб карточки.
public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;       // картинка автомата
    [SerializeField] private TMP_Text priceText;    // цена
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text buttonLabel;  // необязательно: надпись на кнопке

    [Header("Надписи на кнопке")]
    [SerializeField] private string buyText = "Купить";
    [SerializeField] private string ownedText = "Куплено";

    public void Setup(ArcadeMachine machine, Action<ArcadeMachine> onBuy)
    {
        if (iconImage != null)
        {
            iconImage.sprite = machine.Icon;
            iconImage.preserveAspect = true;
            iconImage.enabled = machine.Icon != null;
        }

        if (buttonLabel == null)
        {
            buttonLabel = buyButton.GetComponentInChildren<TMP_Text>();
        }

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => onBuy(machine));
    }

    public void Refresh(ArcadeMachine machine, bool canBuy)
    {
        if (priceText != null) priceText.text = machine.Price + "$";

        if (machine.IsPurchased)
        {
            if (buttonLabel != null) buttonLabel.text = ownedText;
            buyButton.interactable = false;
        }
        else
        {
            if (buttonLabel != null) buttonLabel.text = buyText;
            buyButton.interactable = canBuy;
        }
    }
}