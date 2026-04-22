using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RunManager — Mengelola satu siklus run roguelike Heroll.
///
/// Tanggung jawab:
///   • Menyimpan statistik run yang sedang berjalan (tile dicapai, enemy dibunuh, dll)
///   • Menghitung reward meta saat run selesai (menang atau kalah)
///   • Menyimpan daftar konten yang sudah di-unlock ke SaveSystem
///   • Mengelola modifier antar-run (contoh: "run berikutnya mulai dengan +1 HP")
/// </summary>
public class RunManager : MonoBehaviour
{
    // ─── Run Stats ────────────────────────────────────────────────────────────
    [Header("Run State (read-only in inspector)")]
    [SerializeField] private int currentRunNumber = 1;
    [SerializeField] private int highestTileReached = 0;
    [SerializeField] private int enemiesDefeated = 0;
    [SerializeField] private int totalScalesEarned = 0; // Sisik Naga terkumpul run ini
    [SerializeField] private bool runActive = false;

    // ─── Meta Unlock Pool ─────────────────────────────────────────────────────
    // Daftar semua ID konten yang bisa di-unlock (diisi via Inspector atau SO)
    [Header("Unlock Pool")]
    [SerializeField] private List<string> allUnlockableIDs = new();

    // Konten yang sudah di-unlock (persisten via SaveSystem)
    private HashSet<string> unlockedIDs = new();

    // ─── Run Modifiers ────────────────────────────────────────────────────────
    // Modifier yang berlaku di run BERIKUTNYA (contoh: bonus HP awal dari unlock)
    public int BonusStartHP { get; private set; } = 0;
    public int BonusStartScales { get; private set; } = 0;
    public float CurseChanceModifier { get; private set; } = 1f; // multiplier

    // ─── Events ───────────────────────────────────────────────────────────────
    public static event Action<RunSummary> OnRunEnded;
    public static event Action<string> OnContentUnlocked; // ID konten baru

    // ─── Run Lifecycle ────────────────────────────────────────────────────────
    /// <summary>Dipanggil GameManager.StartNewRun().</summary>
    public void InitializeRun()
    {
        highestTileReached = 0;
        enemiesDefeated = 0;
        totalScalesEarned = 0;
        runActive = true;

        // Terapkan modifier run
        GameManager.Instance.PlayerController.ApplyRunModifiers(BonusStartHP);
        GameManager.Instance.CurrencyManager.AddScales(BonusStartScales);

        Debug.Log($"[RunManager] Run #{currentRunNumber} dimulai.");
    }

    /// <summary>Dipanggil GameManager saat pemain mati.</summary>
    public void OnRunFailed()
    {
        if (!runActive) return;
        runActive = false;

        var summary = BuildSummary(victory: false);
        GrantMetaRewards(summary);
        TryUnlockContent(summary);

        currentRunNumber++;
        OnRunEnded?.Invoke(summary);

        Debug.Log($"[RunManager] Run gagal. Tile tertinggi: {highestTileReached}");
    }

    /// <summary>Dipanggil GameManager saat pemain menyelesaikan papan.</summary>
    public void OnRunCompleted()
    {
        if (!runActive) return;
        runActive = false;

        var summary = BuildSummary(victory: true);
        GrantMetaRewards(summary);
        TryUnlockContent(summary);

        currentRunNumber++;
        OnRunEnded?.Invoke(summary);

        Debug.Log($"[RunManager] Run menang! Total run: {currentRunNumber - 1}");
    }

    // ─── Stat Tracking ────────────────────────────────────────────────────────
    public void RecordTileReached(int tileIndex)
    {
        if (tileIndex > highestTileReached)
            highestTileReached = tileIndex;
    }

    public void RecordEnemyDefeated()
    {
        enemiesDefeated++;
    }

    public void RecordScalesEarned(int amount)
    {
        totalScalesEarned += amount;
    }

    // ─── Rewards & Unlock ─────────────────────────────────────────────────────
    private void GrantMetaRewards(RunSummary summary)
    {
        // Formula reward meta: semakin jauh, semakin banyak sisik bonus
        int metaScales = Mathf.FloorToInt(summary.HighestTile * 0.5f)
                       + summary.EnemiesDefeated
                       + (summary.Victory ? 20 : 0);

        GameManager.Instance.SaveSystem.AddMetaScales(metaScales);
        Debug.Log($"[RunManager] Meta reward: +{metaScales} Sisik Naga (meta)");
    }

    private void TryUnlockContent(RunSummary summary)
    {
        // Contoh kondisi unlock — kembangkan sesuai desain game
        CheckAndUnlock("dice_risk_x2", summary.RunNumber >= 2);
        CheckAndUnlock("curse_slot_extra", summary.HighestTile >= 50);
        CheckAndUnlock("boss_naga_alternate", summary.Victory);
        CheckAndUnlock("artifact_harta", summary.EnemiesDefeated >= 10);
    }

    private void CheckAndUnlock(string id, bool condition)
    {
        if (!condition) return;
        if (unlockedIDs.Contains(id)) return;
        if (!allUnlockableIDs.Contains(id)) return;

        unlockedIDs.Add(id);
        OnContentUnlocked?.Invoke(id);
        Debug.Log($"[RunManager] Konten BARU di-unlock: {id}");
    }

    // ─── Modifier Recalculation ───────────────────────────────────────────────
    /// <summary>
    /// Dipanggil SaveSystem setelah load, atau setelah unlock baru.
    /// Hitung ulang modifier run berdasarkan semua konten yang sudah di-unlock.
    /// </summary>
    public void RecalculateModifiers()
    {
        BonusStartHP = 0;
        BonusStartScales = 0;
        CurseChanceModifier = 1f;

        if (unlockedIDs.Contains("bonus_hp_start")) BonusStartHP += 2;
        if (unlockedIDs.Contains("bonus_scales_start")) BonusStartScales += 5;
        if (unlockedIDs.Contains("curse_resistance")) CurseChanceModifier = 0.75f;

        Debug.Log($"[RunManager] Modifier recalculated — BonusHP:{BonusStartHP} BonusScales:{BonusStartScales}");
    }

    // ─── Save / Load Integration ──────────────────────────────────────────────
    public void LoadUnlocks(List<string> savedUnlocks)
    {
        unlockedIDs = new HashSet<string>(savedUnlocks);
        RecalculateModifiers();
    }

    public List<string> GetUnlocksForSave()
    {
        return new List<string>(unlockedIDs);
    }

    public bool IsUnlocked(string id) => unlockedIDs.Contains(id);

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private RunSummary BuildSummary(bool victory)
    {
        return new RunSummary
        {
            RunNumber = currentRunNumber,
            Victory = victory,
            HighestTile = highestTileReached,
            EnemiesDefeated = enemiesDefeated,
            ScalesEarned = totalScalesEarned
        };
    }

    public int CurrentRunNumber => currentRunNumber;
}

// ─── Data Struct ──────────────────────────────────────────────────────────────
[Serializable]
public struct RunSummary
{
    public int RunNumber;
    public bool Victory;
    public int HighestTile;
    public int EnemiesDefeated;
    public int ScalesEarned;
}