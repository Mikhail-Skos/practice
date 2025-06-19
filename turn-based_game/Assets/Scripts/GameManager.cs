using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GamePhase { CardSelection, Player1Action, Player2Action }

    [Header("Настройки")]
    public int maxTurns = 10;
    public int currentTurn = 1;
    public GamePhase currentPhase = GamePhase.CardSelection;

    [Header("UI")]
    public TextMeshProUGUI turnCounterText;
    public TextMeshProUGUI player1HpText;
    public TextMeshProUGUI player2HpText;
    public GameObject actionPanel;
    public GameObject endGamePanel;
    public TextMeshProUGUI endGameText;
    public Button restartButton;

    [Header("Отряды")]
    public Squad player1Squad;
    public Squad player2Squad;

    private bool player1CardSelected = false;
    private bool player2CardSelected = false;

    public bool IsPlayer1Turn() => currentPhase == GamePhase.Player1Action;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        InitializeGame();
    }

    private void InitializeGame()
    {
        Time.timeScale = 1f;
        actionPanel.SetActive(false);
        endGamePanel.SetActive(false);
        
        if (restartButton != null)
            restartButton.gameObject.SetActive(false);
            
        currentTurn = 1;
        currentPhase = GamePhase.CardSelection;
        player1CardSelected = false;
        player2CardSelected = false;
        
        player1Squad.ResetHeroes();
        player2Squad.ResetHeroes();
        
        UpdateUI();
        CardManager.Instance.ShowCards();
    }

    public void UpdateUI()
    {
        turnCounterText.text = $"Ход {currentTurn}/{maxTurns}";
        player1HpText.text = $"Команда 1: {player1Squad.GetTotalHP()} HP";
        player2HpText.text = $"Команда 2: {player2Squad.GetTotalHP()} HP";
    }

    public void OnCardSelected(bool isPlayer1)
    {
        if (isPlayer1) 
            player1CardSelected = true;
        else 
            player2CardSelected = true;

        CardManager.Instance.ClearPlayerCards(isPlayer1);

        if (player1CardSelected && player2CardSelected)
        {
            player1CardSelected = false;
            player2CardSelected = false;
            currentPhase = GamePhase.Player1Action;
            actionPanel.SetActive(true);
            ActionSystem.Instance.BeginHeroActions();
        }
    }

    public void EndTeamTurn()
    {
        if (currentPhase == GamePhase.Player1Action)
        {
            currentPhase = GamePhase.Player2Action;
            ActionSystem.Instance.BeginHeroActions();
        }
        else
        {
            EndTurn();
        }
    }

    public void EndTurn()
    {
        CardManager.Instance.ResetAllEffects();
        currentTurn++;
        
        if (CheckWinCondition()) return;
        
        currentPhase = GamePhase.CardSelection;
        actionPanel.SetActive(false);
        UpdateUI();
        CardManager.Instance.ShowCards();
    }

    public bool CheckWinCondition()
    {
        // Получаем общее HP обеих команд
        int totalHP1 = player1Squad.GetTotalHP();
        int totalHP2 = player2Squad.GetTotalHP();
        
        // Если у одной из команд HP <= 0 (все юниты мертвы)
        if (totalHP1 <= 0 || totalHP2 <= 0)
        {
            string result;
            
            if (totalHP1 <= 0 && totalHP2 <= 0)
                result = "Ничья!";
            else if (totalHP1 <= 0)
                result = "Игрок 2 победил!";
            else 
                result = "Игрок 1 победил!";
            
            ShowEndScreen(result);
            return true;
        }
        
        // Если достигнут лимит ходов
        if (currentTurn > maxTurns)
        {
            string result;
            
            if (totalHP1 > totalHP2) 
                result = "Игрок 1 победил!";
            else if (totalHP2 > totalHP1) 
                result = "Игрок 2 победил!";
            else 
                result = "Ничья!";
            
            ShowEndScreen(result);
            return true;
        }
        
        return false;
    }

    private void ShowEndScreen(string text)
    {
        endGameText.text = text;
        endGamePanel.SetActive(true);
        actionPanel.SetActive(false);
        
        if (restartButton != null)
            restartButton.gameObject.SetActive(true);
            
        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}