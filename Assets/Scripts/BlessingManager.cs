using System;
using System.Collections.Generic;
using UnityEngine;


// ─── Blessing Manager ─────────────────────────────────────────────────────────
/// <summary>
/// BlessingManager — Mengelola sistem berkat/buff positif Heroll.
/// Kebalikan dari CurseManager — memberikan efek menguntungkan.
/// </summary>
public class BlessingManager : MonoBehaviour
{
    public enum BlessingType
    {
        DragonScale,    // +1 DEF permanen run ini
        LuckyStep,      // Peluang 20% dapat +1 langkah bonus
        SharpClaw,      // +2 ATK permanen run ini
        HealingAura,    // Pulih 1 HP tiap kali membunuh musuh
        MerchantFavor   // Toko 15% lebih murah
    }

    [Serializable]
    public class Blessing
    {
        public BlessingType type;
        public string displayName;
        [TextArea] public string description;
    }

    [Header("Blessing Pool")]
    [SerializeField] private List<Blessing> blessingPool = new();

    private List<Blessing> activeBlessing = new();
    public IReadOnlyList<Blessing> ActiveBlessings => activeBlessing;

    public static event Action<Blessing> OnBlessingApplied;

    public void ApplyBlessing(BlessingType type)
    {
        Blessing blessing = blessingPool.Find(b => b.type == type);
        if (blessing == null) return;

        switch (type)
        {
            case BlessingType.DragonScale:
                GameManager.Instance.PlayerController.AddDefense(1);
                break;
            case BlessingType.SharpClaw:
                GameManager.Instance.PlayerController.AddAttack(2);
                break;
        }

        activeBlessing.Add(blessing);
        OnBlessingApplied?.Invoke(blessing);
        Debug.Log($"[BlessingManager] Berkat diterapkan: {blessing.displayName}");
    }

    public bool HasBlessing(BlessingType type)
    {
        return activeBlessing.Exists(b => b.type == type);
    }

    /// <summary>Modifikasi attack pemain berdasarkan berkat aktif.</summary>
    public int ModifyPlayerAttack(int baseAttack)
    {
        // Contoh perluasan: artifact/blessing bisa tambah multiplier di sini
        return baseAttack;
    }

    public void ClearAll()
    {
        activeBlessing.Clear();
    }
}
