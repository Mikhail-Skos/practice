using UnityEngine;
using UnityEngine.UI;

public class RulesPanelController : MonoBehaviour
{
    [Header("Настройки")]
    public GameObject rulesPanel; // Ссылка на панель правил
    public Button toggleRulesButton; // Кнопка для открытия/закрытия
    
    [Header("Визуальные настройки")]
    public Text buttonText; // Текст на кнопке (опционально)

    private void Start()
    {
        // Гарантируем, что панель скрыта при старте
        if (rulesPanel != null)
            rulesPanel.SetActive(false);
        
        // Назначаем обработчик нажатия на кнопку
        if (toggleRulesButton != null)
        {
            toggleRulesButton.onClick.AddListener(ToggleRulesPanel);
        }
        
        // Обновляем текст кнопки
        UpdateButtonText();
    }

    // Метод переключения видимости панели
    public void ToggleRulesPanel()
    {
        if (rulesPanel != null)
        {
            // Инвертируем текущее состояние панели
            bool newState = !rulesPanel.activeSelf;
            rulesPanel.SetActive(newState);
            
            // Обновляем текст кнопки
            UpdateButtonText();
            
            // При необходимости можно приостанавливать игру
            // Time.timeScale = newState ? 0 : 1;
        }
    }
    
    // Обновление текста кнопки в зависимости от состояния
    private void UpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = rulesPanel.activeSelf ? "Закрыть правила" : "Правила игры";
        }
    }
    
    // Метод для закрытия панели (можно привязать к кнопке закрытия)
    public void CloseRulesPanel()
    {
        if (rulesPanel != null && rulesPanel.activeSelf)
        {
            rulesPanel.SetActive(false);
            UpdateButtonText();
            // Time.timeScale = 1;
        }
    }
}