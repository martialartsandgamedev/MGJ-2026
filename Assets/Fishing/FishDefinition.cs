// using Sirenix.OdinInspector;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum FishRarity { Bronze, Silver, Gold }
public enum FishSize {Small, Medium, Large, Gargantuan}


[CreateAssetMenu(menuName = "Fish")]
public class FishDefinition : ScriptableObject
{
    public string ID = "Fish";
    
    public Vector2Int SizeRange;
    public FishRarity Rarity;
    
    // [Button]
    public Fish GetFromDefinition()
    {
        Fish newFish = new Fish(this);
        return newFish;
    }
}

public class Fish
{
    public string ResolvedID;
    
    public FishSize Size;
    public FishRarity Rarity;
    
    public Fish(FishDefinition definition)
    {
        //Make sure the resolved size does not go outside the range of the enum value member count
        Size = (FishSize) Mathf.Clamp(  Random.Range(definition.SizeRange.x, definition.SizeRange.y), 0, Enum.GetValues(typeof(FishSize)).Length-1);
        Rarity = (FishRarity)Random.Range(0, Enum.GetValues(typeof(FishRarity)).Length);

        ResolvedID = string.Format($"{Size} {Rarity} {definition.ID}");
        
        Debug.LogFormat($"Created a new fish {ResolvedID} with size {Size} and rarity {Rarity}");
    }
}
