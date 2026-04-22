using System;
using System.Collections.Generic;
using UnityEngine;

// ─── Currency Manager ─────────────────────────────────────────────────────────
/// <summary>
/// CurrencyManager — Mengelola Sisik Naga (mata uang in-run) Heroll.
/// Sisik in-run TIDAK persisten — direset tiap run.
/// Sisik meta (antar-run) dikelola oleh SaveSystem.
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private int startingScales = 0;

    public int CurrentScales { get; private set; }

    public static event Action<int> OnScalesChanged;

    public void ResetForNewRun()
    {
        CurrentScales = startingScales;
        OnScalesChanged?.Invoke(CurrentScales);
    }

    public void AddScales(int amount)
    {
        if (amount <= 0) return;
        CurrentScales += amount;
        OnScalesChanged?.Invoke(CurrentScales);
        Debug.Log($"[CurrencyManager] +{amount} Sisik. Total: {CurrentScales}");
    }

    public bool SpendScales(int amount)
    {
        if (CurrentScales < amount)
        {
            Debug.Log("[CurrencyManager] Sisik tidak cukup.");
            return false;
        }
        CurrentScales -= amount;
        OnScalesChanged?.Invoke(CurrentScales);
        Debug.Log($"[CurrencyManager] -{amount} Sisik. Total: {CurrentScales}");
        return true;
    }
}
