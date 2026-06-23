using JxModule;
using UnityEngine;

public static class EventNPCFactory
{
    public static EventNpc Create(string eventID)
    {
        if (string.IsNullOrEmpty(eventID))
        {
            DebugExtension.LogColor($"EventFactory: event row is null.", Color.red);
            return null;
        }
        
        EventNpc npc = eventID switch
        {
            "event_shop" => PrefabManager.CachePrefab<EventMerchantNpc>(),
            //"event_reinforcement" => PrefabManager.CachePrefab<EventCraftmanNpc>(),
            _ => null,
        };
        
        return npc;
    }
}