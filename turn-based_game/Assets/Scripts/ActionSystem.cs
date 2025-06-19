using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ActionSystem : MonoBehaviour
{
    public static ActionSystem Instance;

    [Header("UI Elements")]
    public GameObject actionButtonsPanel;
    public Button attackButton;
    public Button moveButton;
    public Button ultimateButton;

    private Hero currentHero;
    private bool isWaitingForInput = false;
    private string currentAction = "";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        attackButton.onClick.AddListener(() => SetAction("Attack"));
        moveButton.onClick.AddListener(() => SetAction("Move"));
        ultimateButton.onClick.AddListener(() => SetAction("Ultimate"));
        actionButtonsPanel.SetActive(false);
    }

    public void BeginHeroActions()
    {
        StartCoroutine(SelectHeroCoroutine());
    }

    private IEnumerator SelectHeroCoroutine()
    {
        yield return null;
        Hero[] allHeroes = FindObjectsOfType<Hero>();
        foreach (Hero.HeroType type in new[] { Hero.HeroType.Tank, Hero.HeroType.Shooter, Hero.HeroType.Mage })
        {
            foreach (Hero h in allHeroes)
            {
                if (h.isPlayerTeam == GameManager.Instance.IsPlayer1Turn() &&
                    h.type == type && h.currentHP > 0)
                {
                    SelectHero(h);
                    yield break;
                }
            }
        }
        GameManager.Instance.EndTeamTurn();
    }

    private void SelectHero(Hero hero)
    {
        currentHero = hero;
        isWaitingForInput = true;
        currentAction = "";
        HighlightCurrentHero();
        actionButtonsPanel.SetActive(true);
        ultimateButton.interactable = currentHero.ultCharge >= currentHero.ultChargeNeeded;
    }

    private void HighlightCurrentHero()
    {
        foreach (Hero h in FindObjectsOfType<Hero>())
        {
            h.GetComponent<SpriteRenderer>().color = h.isPlayerTeam ? Color.cyan : Color.red;
        }
        currentHero.GetComponent<SpriteRenderer>().color = Color.yellow;
    }

    private void SetAction(string action)
    {
        currentAction = action;
        GridManager.Instance.ResetHighlights();

        for (int x = 0; x < 7; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                GridCell cell = GridManager.Instance.GetCell(pos);
                if (cell == null) continue;

                bool highlight = false;
                Color color = Color.white;

                switch (action)
                {
                    case "Attack":
                        highlight = currentHero.CanAttack(pos);
                        color = Color.red;
                        break;
                    case "Move":
                        highlight = currentHero.CanMoveTo(pos);
                        color = Color.green;
                        break;
                    case "Ultimate":
                        if (currentHero.ultCharge >= currentHero.ultChargeNeeded)
                        {
                            highlight = CheckUltimateTarget(pos);
                            color = Color.blue;
                        }
                        break;
                }

                if (highlight) cell.Highlight(color);
            }
        }
    }

    private bool CheckUltimateTarget(Vector2Int pos)
    {
        switch (currentHero.type)
        {
            case Hero.HeroType.Tank:
                return IsInTankUltimateRange(pos);
            case Hero.HeroType.Shooter:
                return currentHero.CanAttack(pos);
            case Hero.HeroType.Mage:
                GridCell cell = GridManager.Instance.GetCell(pos);
                return cell != null && cell.occupiedHero == null;
            default:
                return false;
        }
    }

    private bool IsInTankUltimateRange(Vector2Int pos)
    {
        foreach (Vector2Int dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
        {
            for (int i = 1; i <= 2; i++)
            {
                if (currentHero.currentGridPos + dir * i == pos)
                    return true;
            }
        }
        return false;
    }

    public void HandleCellClick(Vector2Int gridPos)
    {
        if (!isWaitingForInput || currentHero == null) return;

        GridCell cell = GridManager.Instance.GetCell(gridPos);
        if (cell == null) return;

        switch (currentAction)
        {
            case "Attack":
                if (cell.IsHighlighted(Color.red))
                {
                    Hero target = cell.occupiedHero;
                    if (target != null)
                    {
                        currentHero.Attack(target);
                        FinishAction();
                    }
                }
                break;
            case "Move":
                if (cell.IsHighlighted(Color.green))
                {
                    currentHero.MoveTo(gridPos);
                    FinishAction();
                }
                break;
            case "Ultimate":
                if (cell.IsHighlighted(Color.blue))
                {
                    currentHero.UseUltimate(gridPos);
                    FinishAction();
                }
                break;
        }
    }

    private void FinishAction()
    {
        isWaitingForInput = false;
        actionButtonsPanel.SetActive(false);
        GridManager.Instance.ResetHighlights();
        StartCoroutine(SelectNextHeroCoroutine());
    }

    private IEnumerator SelectNextHeroCoroutine()
    {
        yield return null;
        Hero[] allHeroes = FindObjectsOfType<Hero>();
        bool foundCurrent = false;

        foreach (Hero.HeroType type in new[] { Hero.HeroType.Tank, Hero.HeroType.Shooter, Hero.HeroType.Mage })
        {
            foreach (Hero h in allHeroes)
            {
                if (h.isPlayerTeam == GameManager.Instance.IsPlayer1Turn() &&
                    h.type == type && h.currentHP > 0)
                {
                    if (foundCurrent)
                    {
                        SelectHero(h);
                        yield break;
                    }
                    if (h == currentHero) foundCurrent = true;
                }
            }
        }
        GameManager.Instance.EndTeamTurn();
    }
}