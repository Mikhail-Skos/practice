using UnityEngine;

public class HeroPlacer : MonoBehaviour
{
    [System.Serializable]
    public class HeroSetup
    {
        public GameObject prefab;
        public Vector2Int gridPosition;
        public bool isPlayerTeam;
    }

    public HeroSetup[] heroSetups = new HeroSetup[]
    {
        new HeroSetup { gridPosition = new Vector2Int(1, 3), isPlayerTeam = true },
        new HeroSetup { gridPosition = new Vector2Int(0, 1), isPlayerTeam = true },
        new HeroSetup { gridPosition = new Vector2Int(0, 6), isPlayerTeam = true },
        new HeroSetup { gridPosition = new Vector2Int(5, 3), isPlayerTeam = false },
        new HeroSetup { gridPosition = new Vector2Int(6, 5), isPlayerTeam = false },
        new HeroSetup { gridPosition = new Vector2Int(6, 0), isPlayerTeam = false }
    };

    [Header("Отряды")]
    public Squad player1Squad;
    public Squad player2Squad;

    void Start()
    {
        foreach (var setup in heroSetups)
        {
            PlaceHero(setup);
        }
        
        // Обновляем UI после размещения всех юнитов
        GameManager.Instance?.UpdateUI();
    }

    void PlaceHero(HeroSetup setup)
    {
        GridCell cell = GridManager.Instance.GetCell(setup.gridPosition);
        if (cell == null || cell.occupiedHero != null) return;

        GameObject hero = Instantiate(setup.prefab, cell.transform.position, Quaternion.identity);
        Hero heroComponent = hero.GetComponent<Hero>();
        heroComponent.currentGridPos = setup.gridPosition;
        heroComponent.isPlayerTeam = setup.isPlayerTeam;
        cell.occupiedHero = heroComponent;

        // Добавляем героя в соответствующий отряд
        if (setup.isPlayerTeam)
        {
            if (!player1Squad.heroes.Contains(heroComponent))
                player1Squad.heroes.Add(heroComponent);
        }
        else
        {
            if (!player2Squad.heroes.Contains(heroComponent))
                player2Squad.heroes.Add(heroComponent);
        }
    }
}