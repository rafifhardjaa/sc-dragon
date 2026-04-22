using UnityEngine;

// ─── Shop Item Data ───────────────────────────────────────────────────────────
/// <summary>Data item toko.</summary>
[CreateAssetMenu(fileName = "ShopItemData", menuName = "Heroll/Shop Item Data")]
public class ShopItemData : ScriptableObject
{
    public enum EffectType
    {
        Artifact,
        HealHP,
        AddAttack,
        AddDefense,
        RemoveCurse
    }

    [Header("Identity")]
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Shop")]
    public int price = 10;

    [Header("Effect")]
    public EffectType effectType;
    public int effectValue = 0;
    public string artifactID; // diisi jika effectType = Artifact

    /// <summary>Clone ringan untuk modifikasi harga sementara.</summary>
    public ShopItemData Clone()
    {
        var clone = CreateInstance<ShopItemData>();
        clone.itemName = itemName;
        clone.description = description;
        clone.icon = icon;
        clone.price = price;
        clone.effectType = effectType;
        clone.effectValue = effectValue;
        clone.artifactID = artifactID;
        return clone;
    }
}

