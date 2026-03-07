//Specific info for the instance
using Controllers;
using System;
using System.Collections.Generic;
[Serializable]
public class FishingSpotContext
{
    public float RemainingFish;
    public int ActionCount;
    public float Duration;
    public double Buffer;
    public DefaultFishingWidget UIWidget { get; set; }
    public List<FishDefinition> Table { get; set; }
    public FishingSpotContext(FishingSpotDefinition definition)
    {
        RemainingFish = definition.Capacity;
        ActionCount = definition.ActionCount;
        Duration = definition.Duration;
        Buffer = definition.Buffer;
        UIWidget = definition.UIWidget;
        Table = definition.Table;
    }
}
