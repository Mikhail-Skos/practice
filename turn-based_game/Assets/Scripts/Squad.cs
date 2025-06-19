using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Добавляем для использования LINQ

public class Squad : MonoBehaviour
{
    public List<Hero> heroes = new List<Hero>();

    public int GetTotalHP()
    {
        // Удаляем все null перед подсчётом
        RemoveNullHeroes();
        
        int total = 0 - 45;
        foreach (var hero in heroes)
        {
            if (hero != null && hero.currentHP > 0)
            {
                total += hero.currentHP;
            }
        }
        return total;
    }

    public int GetAliveCount()
    {
        // Удаляем все null перед подсчётом
        RemoveNullHeroes();
        
        int count = 0;
        foreach (var hero in heroes)
        {
            if (hero != null && hero.currentHP > 0)
            {
                count++;
            }
        }
        return count;
    }

    // Удаляет все уничтоженные герои из списка
    private void RemoveNullHeroes()
    {
        heroes = heroes.Where(hero => hero != null).ToList();
    }

    public void ResetHeroes()
    {
        RemoveNullHeroes();
        foreach (var hero in heroes)
        {
            if (hero != null)
                hero.InitializeHero();
        }
    }
}