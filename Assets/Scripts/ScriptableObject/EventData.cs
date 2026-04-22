using System;
using System.Collections.Generic;
using UnityEngine;


// ─── Event Data ───────────────────────────────────────────────────────────────
/// <summary>Data event tile — pilihan yang muncul saat pemain mendarat di EventTile.</summary>
[CreateAssetMenu(fileName = "EventData", menuName = "Heroll/Event Data")]
public class EventData : ScriptableObject
{
    [Header("Event Info")]
    public string eventTitle;
    [TextArea] public string flavorText;
    public Sprite illustration;

    [Header("Choices")]
    public List<EventChoice> choices = new();
}

[Serializable]
public class EventChoice
{
    public string label;
    [TextArea] public string outcomeText;
    public EventEffectType effectType;
    public int value;

    public enum EventEffectType
    {
        Heal,
        Damage,
        GainScales,
        LoseScales,
        GainCurse,
        GainBlessing,
        Nothing
    }
}
