using UnityEngine;

public class ActionManager : MonoBehaviour
{
    public static ActionManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void BeginHeroActions()
    {
        Debug.Log("Starting hero actions");
        // Здесь будет логика поочередных действий героев
    }
}