using System;
using System.Collections.Generic;
using UnityEngine;

// ─── Shop Manager ─────────────────────────────────────────────────────────────
/// <summary>
/// ShopManager — Mengelola toko dalam game Heroll.
///
/// Toko menawarkan artifact, dadu, dan blessing yang bisa dibeli
/// menggunakan Sisik Naga in-run.
/// </summary>
public class ShopManager : MonoBehaviour
{
    [Header("Shop Config")]
    [SerializeField] private int itemsPerShop = 3;
    [SerializeField] private List<ShopItemData> itemPool = new();

    private List<ShopItemData> currentStock = new();
    public IReadOnlyList<ShopItemData> CurrentStock => currentStock;

    public static event Action<List<ShopItemData>> OnShopOpened;
    public static event Action<ShopItemData> OnItemPurchased;

    public void OpenShop()
    {
        GenerateStock();
        OnShopOpened?.Invoke(currentStock);
        GameManager.Instance.UIManager.ShowShopPanel();
        Debug.Log($"[ShopManager] Toko dibuka dengan {currentStock.Count} item.");
    }

    private void GenerateStock()
    {
        currentStock.Clear();
        List<ShopItemData> pool = new List<ShopItemData>(itemPool);

        // Cek apakah MerchantFavor blessing aktif
        bool discount = GameManager.Instance.BlessingManager.HasBlessing(BlessingManager.BlessingType.MerchantFavor);

        for (int i = 0; i < itemsPerShop && pool.Count > 0; i++)
        {
            int idx = UnityEngine.Random.Range(0, pool.Count);
            ShopItemData item = pool[idx].Clone();

            if (discount) item.price = Mathf.RoundToInt(item.price * 0.85f);
            if (GameManager.Instance.CurseManager.HasCurse(CurseManager.CurseType.GreedyCurse))
                item.price = Mathf.RoundToInt(item.price * 1.2f);

            currentStock.Add(item);
            pool.RemoveAt(idx);
        }
    }

    public bool TryPurchase(ShopItemData item)
    {
        if (!GameManager.Instance.CurrencyManager.SpendScales(item.price))
            return false;

        ApplyItemEffect(item);
        currentStock.Remove(item);
        OnItemPurchased?.Invoke(item);
        Debug.Log($"[ShopManager] Item dibeli: {item.itemName}");
        return true;
    }

    private void ApplyItemEffect(ShopItemData item)
    {
        switch (item.effectType)
        {
            case ShopItemData.EffectType.Artifact:
                GameManager.Instance.InventorySystem.AddArtifact(item.artifactID);
                break;
            case ShopItemData.EffectType.HealHP:
                GameManager.Instance.PlayerController.Heal(item.effectValue);
                break;
            case ShopItemData.EffectType.AddAttack:
                GameManager.Instance.PlayerController.AddAttack(item.effectValue);
                break;
            case ShopItemData.EffectType.AddDefense:
                GameManager.Instance.PlayerController.AddDefense(item.effectValue);
                break;
            case ShopItemData.EffectType.RemoveCurse:
                // Hapus kutukan paling awal
                if (GameManager.Instance.CurseManager.CurseCount > 0)
                    GameManager.Instance.CurseManager.RemoveCurse(
                        GameManager.Instance.CurseManager.ActiveCurses[0].type);
                break;
        }
    }

    public void CloseShop()
    {
        GameManager.Instance.OnTileEventFinished();
    }
}
