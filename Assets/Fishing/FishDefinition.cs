using UnityEngine;

[CreateAssetMenu(menuName = "FishingDefinition")]
public class FishDefinition : ScriptableObject
{
    public string ID = "Fish";

    public int Size;
    public int Rarity;
    
    public Fish GetFromDefinition()
    {
        return new Fish(this);
    }
}

public class Fish
{
    public string ResolvedID;
    
    public int Size;
    public int Rarity;
    
    public Fish(FishDefinition definition)
    {
        Size = definition.Size;
        Rarity = definition.Rarity;

        ResolvedID = string.Format($"{Size} {Rarity} {definition.ID}");
    }
}
