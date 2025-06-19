using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardObject : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image iconImage;

    private CardManager.GameCard currentCard;
    private bool isPlayer1Card;
    private bool isSelected = false;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void Setup(CardManager.GameCard card, bool isPlayer1)
    {
        currentCard = card;
        isPlayer1Card = isPlayer1;
        nameText.text = card.name;
        descriptionText.text = card.description;
        isSelected = false;
    }

    private void OnClick()
    {
        if (isSelected || currentCard == null) return;
        isSelected = true;

        // Получаем отряд для применения эффекта
        Squad squad = isPlayer1Card ? 
            GameManager.Instance.player1Squad : GameManager.Instance.player2Squad;
        
        if (squad != null)
        {
            // Применяем эффект карты
            currentCard.effect?.Invoke(squad);
            
            // Сообщаем GameManager о выборе карты
            GameManager.Instance.OnCardSelected(isPlayer1Card);
        }
        
        // Уничтожаем карту
        Destroy(gameObject);
    }
}