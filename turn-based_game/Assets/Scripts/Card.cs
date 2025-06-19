[System.Serializable]
public class Card 
{
    public string cardName;
    public string description;
    public System.Action<Squad> effect;
}