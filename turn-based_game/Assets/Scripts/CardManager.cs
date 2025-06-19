using UnityEngine;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    [System.Serializable]
    public class GameCard
    {
        public string name;
        public string description;
        public System.Action<Squad> effect;
    }

    public List<GameCard> allCards = new List<GameCard>();
    public GameObject cardPrefab;
    public Transform[] playerCardSlots = new Transform[2];
    public Transform[] enemyCardSlots = new Transform[2];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeCards();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeCards()
    {
        allCards = new List<GameCard>
        {
            new GameCard {
                name = "Ярость",
                description = "+1 к урону на ход",
                effect = squad => {
                    if (squad == null) return;
                    foreach (Hero h in squad.heroes)
                    {
                        if (h != null) 
                            h.currentAttackDamage = h.baseAttackDamage + 1;
                    }
                }
            },
            new GameCard {
                name = "Щит",
                description = "+2 к защите на ход",
                effect = squad => {
                    if (squad == null) return;
                    foreach (Hero h in squad.heroes)
                    {
                        if (h != null) 
                            h.currentDefense = h.baseDefense + 2;
                    }
                }
            },
            new GameCard {
                name = "Лечение",
                description = "+2 HP всем",
                effect = squad => {
                    if (squad == null) return;
                    bool updated = false;
                    
                    foreach (Hero h in squad.heroes)
                    {
                        if (h != null && h.currentHP > 0 && h.currentHP < h.baseMaxHP)
                        {
                            // Рассчитываем, сколько HP можно восстановить
                            int healAmount = Mathf.Min(2, h.baseMaxHP - h.currentHP);
                            h.currentHP += healAmount;
                            h.UpdateVisuals();
                            updated = true;
                        }
                    }
                    
                    if (updated) GameManager.Instance?.UpdateUI();
                }
            },
            new GameCard {
                name = "Скорость",
                description = "Танк и Маг двигаются как Стрелок",
                effect = squad => {
                    if (squad == null) return;
                    foreach (Hero h in squad.heroes)
                    {
                        if (h != null && h.type != Hero.HeroType.Shooter)
                        {
                            h.hasSpeedBoost = true;
                            h.currentMoveRange = 2;
                        }
                    }
                }
            },
            new GameCard {
                name = "Ультимативный заряд",
                description = "+1 заряда суперудара каждому",
                effect = squad => {
                    if (squad == null) return;
                    foreach (Hero h in squad.heroes)
                    {
                        if (h != null)
                        {
                            h.ultCharge = Mathf.Min(h.ultCharge + 1, h.ultChargeNeeded);
                            h.UpdateVisuals();
                        }
                    }
                }
            },
            new GameCard {
                name = "Уклонение",
                description = "50% шанс избежать урона",
                effect = squad => {
                    if (squad == null) return;
                    foreach (Hero h in squad.heroes)
                    {
                        if (h != null) 
                            h.hasEvasion = true;
                    }
                }
            },
            new GameCard {
                name = "Критический удар",
                description = "50% шанс нанести двойной урон",
                effect = squad => {
                    if (squad == null) return;
                    foreach (Hero h in squad.heroes)
                    {
                        if (h != null) 
                            h.hasCritical = true;
                    }
                }
            },
            new GameCard {
                name = "Регенерация",
                description = "+5 HP самому раненому",
                effect = squad => {
                    if (squad == null) return;
                    
                    Hero mostInjured = null;
                    int minHP = int.MaxValue;
                    int maxMissingHP = 0; // Максимальный недобор HP
                    
                    // 1. Находим героя с наибольшим недобором HP
                    foreach (Hero h in squad.heroes)
                    {
                        if (h != null && h.currentHP > 0 && h.currentHP < h.baseMaxHP)
                        {
                            int missingHP = h.baseMaxHP - h.currentHP;
                            if (missingHP > maxMissingHP)
                            {
                                maxMissingHP = missingHP;
                                mostInjured = h;
                            }
                            else if (missingHP == maxMissingHP && 
                                     h.currentHP < minHP)
                            {
                                // Если недобор одинаковый, выбираем того, у кого меньше текущее HP
                                minHP = h.currentHP;
                                mostInjured = h;
                            }
                        }
                    }
                    
                    // 2. Если нашли подходящего героя - лечим
                    if (mostInjured != null)
                    {
                        int healAmount = Mathf.Min(5, mostInjured.baseMaxHP - mostInjured.currentHP);
                        mostInjured.currentHP += healAmount;
                        mostInjured.UpdateVisuals();
                        GameManager.Instance?.UpdateUI();
                    }
                }
            },
        };
    }

    public void ShowCards()
    {
        ClearOldCards();
        CreateCardsForPlayer(true);
        CreateCardsForPlayer(false);
    }

    private void CreateCardsForPlayer(bool isPlayer1)
    {
        Transform[] slots = isPlayer1 ? playerCardSlots : enemyCardSlots;
        List<GameCard> tempPool = new List<GameCard>(allCards);

        for (int i = 0; i < 2; i++)
        {
            if (tempPool.Count == 0) break;
            
            int randIndex = Random.Range(0, tempPool.Count);
            GameCard selected = tempPool[randIndex];
            tempPool.RemoveAt(randIndex);

            GameObject cardObj = Instantiate(cardPrefab, slots[i]);
            CardObject cardComponent = cardObj.GetComponent<CardObject>();
            if (cardComponent != null)
            {
                cardComponent.Setup(selected, isPlayer1);
            }
        }
    }

    public void ClearPlayerCards(bool isPlayer1)
    {
        Transform[] slots = isPlayer1 ? playerCardSlots : enemyCardSlots;
        foreach (Transform t in slots)
        {
            if (t.childCount > 0)
            {
                for (int i = 0; i < t.childCount; i++)
                {
                    Destroy(t.GetChild(i).gameObject);
                }
            }
        }
    }

    public void ClearOldCards()
    {
        ClearPlayerCards(true);
        ClearPlayerCards(false);
    }

    public void ResetAllEffects()
    {
        if (GameManager.Instance.player1Squad != null)
            ResetCardEffects(GameManager.Instance.player1Squad);
        
        if (GameManager.Instance.player2Squad != null)
            ResetCardEffects(GameManager.Instance.player2Squad);
    }

    private void ResetCardEffects(Squad squad)
    {
        if (squad == null) return;
        
        foreach (Hero h in squad.heroes)
        {
            if (h != null)
            {
                h.currentAttackDamage = h.baseAttackDamage;
                h.currentDefense = h.baseDefense;
                h.currentMoveRange = h.type == Hero.HeroType.Shooter ? 2 : h.baseMoveRange;
                h.hasSpeedBoost = false;
                h.hasCritical = false;
                h.hasEvasion = false;
            }
        }
    }
}