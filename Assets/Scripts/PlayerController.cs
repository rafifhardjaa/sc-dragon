using System;
using UnityEngine;

/// <summary>
/// PlayerController — Mengelola state dan pergerakan pion pemain di Heroll.
///
/// Tanggung jawab:
///   • HP, Attack, Defense pemain (base + modifier)
///   • Pergerakan pion di papan (berpindah N tile sesuai hasil dadu)
///   • Reset antar run
///   • Menerima modifier dari RunManager (bonus start HP, dll)
/// </summary>
public class PlayerController : MonoBehaviour
{
    // ─── Base Stats ───────────────────────────────────────────────────────────
    [Header("Base Stats")]
    [SerializeField] private int baseMaxHP = 20;
    [SerializeField] private int baseAttack = 5;
    [SerializeField] private int baseDefense = 2;

    // ─── Runtime Stats ────────────────────────────────────────────────────────
    public int MaxHP { get; private set; }
    public int CurrentHP { get; private set; }
    public int Attack { get; private set; }
    public int Defense { get; private set; }
    public int CurrentTileIndex { get; private set; }
    public bool IsDead => CurrentHP <= 0;

    // ─── Events ───────────────────────────────────────────────────────────────
    public static event Action<int, int> OnHPChanged;       // (current, max)
    public static event Action<int> OnPlayerMoved;          // (newTileIndex)
    public static event Action OnPlayerDied;

    // ─── Init ─────────────────────────────────────────────────────────────────
    public void ResetForNewRun()
    {
        MaxHP = baseMaxHP;
        CurrentHP = MaxHP;
        Attack = baseAttack;
        Defense = baseDefense;
        CurrentTileIndex = 0;

        OnHPChanged?.Invoke(CurrentHP, MaxHP);
        Debug.Log("[PlayerController] Reset untuk run baru.");
    }

    /// <summary>Dipanggil RunManager.InitializeRun() — terapkan bonus awal run.</summary>
    public void ApplyRunModifiers(int bonusHP)
    {
        if (bonusHP > 0)
        {
            MaxHP += bonusHP;
            CurrentHP = Mathf.Min(CurrentHP + bonusHP, MaxHP);
            OnHPChanged?.Invoke(CurrentHP, MaxHP);
            Debug.Log($"[PlayerController] Bonus HP run: +{bonusHP}");
        }
    }

    // ─── Movement ─────────────────────────────────────────────────────────────
    /// <summary>
    /// Pindahkan pion sejauh <steps> tile ke depan.
    /// Dipanggil DiceManager setelah dadu dilempar.
    /// </summary>
    public void MoveForward(int steps)
    {
        int totalTiles = GameManager.Instance.BoardManager.TotalTiles;
        CurrentTileIndex = Mathf.Min(CurrentTileIndex + steps, totalTiles - 1);

        GameManager.Instance.RunManager.RecordTileReached(CurrentTileIndex);
        OnPlayerMoved?.Invoke(CurrentTileIndex);

        Debug.Log($"[PlayerController] Pindah ke tile {CurrentTileIndex}");
        GameManager.Instance.OnPlayerLanded();
    }

    // ─── HP Manipulation ──────────────────────────────────────────────────────
    public void TakeDamage(int rawDamage)
    {
        int damage = Mathf.Max(1, rawDamage - Defense);
        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        OnHPChanged?.Invoke(CurrentHP, MaxHP);
        Debug.Log($"[PlayerController] Terima damage {damage} (raw:{rawDamage}). HP: {CurrentHP}/{MaxHP}");

        if (CurrentHP <= 0)
        {
            OnPlayerDied?.Invoke();
            GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
        }
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
        OnHPChanged?.Invoke(CurrentHP, MaxHP);
        Debug.Log($"[PlayerController] Heal +{amount}. HP: {CurrentHP}/{MaxHP}");
    }

    // ─── Stat Modifiers ───────────────────────────────────────────────────────
    public void AddAttack(int amount)  => Attack  = Mathf.Max(0, Attack  + amount);
    public void AddDefense(int amount) => Defense = Mathf.Max(0, Defense + amount);
    public void AddMaxHP(int amount)
    {
        MaxHP = Mathf.Max(1, MaxHP + amount);
        CurrentHP = Mathf.Min(CurrentHP, MaxHP);
        OnHPChanged?.Invoke(CurrentHP, MaxHP);
    }
}
