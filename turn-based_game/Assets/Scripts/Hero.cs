using UnityEngine;
using TMPro;

public class Hero : MonoBehaviour
{
    public enum HeroType { Tank, Shooter, Mage }

    [Header("Settings")]
    public HeroType type;
    public string heroName;
    public bool isPlayerTeam;

    [Header("Base Stats")]
    public int baseMaxHP = 10;
    public int baseAttackDamage = 3;
    public int baseDefense = 1;
    public int baseMoveRange = 1;

    [Header("Current Stats")]
    public int currentHP;
    public int currentAttackDamage;
    public int currentDefense;
    public int currentMoveRange;
    public int ultCharge;
    public int ultChargeNeeded = 3;

    [Header("Card Effects")]
    public bool hasEvasion;
    public bool hasCritical;
    public bool hasSpeedBoost;

    public Vector2Int currentGridPos;

    [Header("UI")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI ultText;

    void Start()
    {
        InitializeHero();
    }

    public void InitializeHero()
    {
        currentHP = baseMaxHP;
        currentAttackDamage = baseAttackDamage;
        currentDefense = baseDefense;
        currentMoveRange = (type == HeroType.Shooter) ? 2 : baseMoveRange;
        ultCharge = 0;
        hasEvasion = false;
        hasCritical = false;
        hasSpeedBoost = false;
        UpdateVisuals();
    }

public void UpdateVisuals()
    {
        if (hpText != null) 
            hpText.text = $"{currentHP}/{baseMaxHP} HP";
        
        if (ultText != null) 
            ultText.text = $"{ultCharge}/{ultChargeNeeded} super";
        
        if (GameManager.Instance != null)
            GameManager.Instance.UpdateUI();
    }

    public bool CanMoveTo(Vector2Int targetPos)
    {
        var cell = GridManager.Instance.GetCell(targetPos);
        if (cell == null || cell.occupiedHero != null) return false;
        
        int range = hasSpeedBoost ? 2 : currentMoveRange;
        int dx = Mathf.Abs(targetPos.x - currentGridPos.x);
        int dy = Mathf.Abs(targetPos.y - currentGridPos.y);
        return dx <= range && dy <= range;
    }

    public void MoveTo(Vector2Int targetPos)
    {
        // Освобождаем текущую клетку
        GridCell currentCell = GridManager.Instance.GetCell(currentGridPos);
        if (currentCell != null) 
            currentCell.occupiedHero = null;
        
        // Занимаем новую клетку
        GridCell targetCell = GridManager.Instance.GetCell(targetPos);
        if (targetCell != null)
        {
            transform.position = targetCell.transform.position;
            currentGridPos = targetPos;
            targetCell.occupiedHero = this;
        }
    }

    public bool CanAttack(Vector2Int targetPos)
    {
        var target = GridManager.Instance.GetHeroAtPosition(targetPos);
        if (target == null || target.isPlayerTeam == isPlayerTeam) return false;

        return type switch
        {
            HeroType.Tank => IsAdjacent(targetPos),
            HeroType.Shooter => IsStraightLine(targetPos),
            HeroType.Mage => true,
            _ => false,
        };
    }

    public void Attack(Hero target)
    {
        if (target == null || target.currentHP <= 0) return;

        // Проверка уклонения
        if (target.hasEvasion && Random.value < 0.5f)
        {
            Debug.Log("УКЛОНЕНИЕ!");
            return;
        }

        // Расчет урона с учетом крита
        int damage = hasCritical && Random.value < 0.5f ? 
            currentAttackDamage * 2 : currentAttackDamage;
        
        // Учет защиты цели
        int finalDamage = Mathf.Max(1, damage - target.currentDefense);

        target.TakeDamage(finalDamage);
        ultCharge = Mathf.Min(ultCharge + 1, ultChargeNeeded);
        UpdateVisuals();
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            UpdateVisuals();
        }
    }

    private void Die()
    {
        // Освобождаем клетку
        GridCell cell = GridManager.Instance.GetCell(currentGridPos);
        if (cell != null) 
            cell.occupiedHero = null;
        
        // Удаляем себя из отряда
        Squad squad = isPlayerTeam ? 
            GameManager.Instance.player1Squad : GameManager.Instance.player2Squad;
        
        if (squad != null && squad.heroes.Contains(this)) 
            squad.heroes.Remove(this);
        
        // Уничтожаем объект
        Destroy(gameObject);
        
        // Немедленная проверка условий победы
        if (GameManager.Instance != null)
            GameManager.Instance.CheckWinCondition();
    }

    public void UseUltimate(Vector2Int targetPos)
    {
        if (ultCharge < ultChargeNeeded) return;

        switch (type)
        {
            case HeroType.Tank:
                TankUltimate(targetPos); 
                break;
            case HeroType.Shooter:
                ShooterUltimate(targetPos); 
                break;
            case HeroType.Mage:
                MageUltimate(targetPos); 
                break;
        }

        ultCharge = 0;
        UpdateVisuals();
    }

    private void TankUltimate(Vector2Int targetPos)
    {
        // Атака по выбранной цели с удвоенным уроном
        Hero target = GridManager.Instance.GetHeroAtPosition(targetPos);
        if (target != null && target.isPlayerTeam != isPlayerTeam)
        {
            int damage = Mathf.Max(1, currentAttackDamage * 2 - target.currentDefense);
            target.TakeDamage(damage);
        }
    }

private void ShooterUltimate(Vector2Int targetPos)
    {
        // 1. Стандартная атака по цели
        Hero target = GridManager.Instance.GetHeroAtPosition(targetPos);
        if (target != null && target.isPlayerTeam != isPlayerTeam)
        {
            int damage = Mathf.Max(1, currentAttackDamage - target.currentDefense);
            target.TakeDamage(damage);
        }

        // 2. Лечение всей своей команды (до максимума)
        Squad squad = isPlayerTeam ? 
            GameManager.Instance.player1Squad : GameManager.Instance.player2Squad;
        
        bool updated = false;
        
        if (squad != null)
        {
            foreach (Hero ally in squad.heroes)
            {
                if (ally != null && ally.currentHP > 0 && ally.currentHP < ally.baseMaxHP)
                {
                    // Рассчитываем, сколько HP можно восстановить
                    int healAmount = Mathf.Min(3, ally.baseMaxHP - ally.currentHP);
                    ally.currentHP += healAmount;
                    ally.UpdateVisuals();
                    updated = true;
                }
            }
            
            if (updated) GameManager.Instance?.UpdateUI();
        }
    }

    private void MageUltimate(Vector2Int targetPos)
    {
        // 1. Телепортация на выбранную клетку
        GridCell targetCell = GridManager.Instance.GetCell(targetPos);
        if (targetCell != null && targetCell.occupiedHero == null)
        {
            MoveTo(targetPos);
        }

        // 2. Урон всем врагам
        Squad enemies = isPlayerTeam ? 
            GameManager.Instance.player2Squad : GameManager.Instance.player1Squad;
        
        if (enemies != null)
        {
            foreach (Hero enemy in enemies.heroes)
            {
                if (enemy != null && enemy.currentHP > 0)
                {
                    // Фиксированный урон 3 минус защита
                    int damage = Mathf.Max(1, 3 - enemy.currentDefense);
                    enemy.TakeDamage(damage);
                }
            }
        }
    }

    bool IsAdjacent(Vector2Int pos) =>
        Mathf.Abs(pos.x - currentGridPos.x) <= 1 && 
        Mathf.Abs(pos.y - currentGridPos.y) <= 1;

    bool IsStraightLine(Vector2Int pos) =>
        pos.x == currentGridPos.x || pos.y == currentGridPos.y;
}