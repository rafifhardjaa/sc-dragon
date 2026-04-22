using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DiceManager — Mengelola sistem dadu Heroll.
///
/// Tanggung jawab:
///   • Menyimpan koleksi dadu yang dimiliki pemain
///   • Melakukan "roll" berdasarkan dadu yang dipilih
///   • Mengelola dadu bertipe khusus (Risk x2, Blessing, dll)
///   • Mengirim hasil roll ke PlayerController
/// </summary>
public class DiceManager : MonoBehaviour
{
    // ─── Dice Types ───────────────────────────────────────────────────────────
    public enum DiceType
    {
        Normal,      // D6 biasa (1-6)
        Safe,        // D4 (1-4) — lebih aman
        RiskX2,      // D6 × 2 hasil, tapi musuh lebih kuat
        Blessing,    // D6 + peluang bonus langkah
        Cursed       // D6, tapi ada peluang kutukan tambah
    }

    [Serializable]
    public class DiceItem
    {
        public DiceType type;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public int minRoll;
        public int maxRoll;
        public bool isUnlocked;
    }

    // ─── Config ───────────────────────────────────────────────────────────────
    [Header("Dice Collection")]
    [SerializeField] private List<DiceItem> allDice = new();

    // ─── Runtime ──────────────────────────────────────────────────────────────
    private DiceItem selectedDice;
    public DiceItem SelectedDice => selectedDice;

    // ─── Events ───────────────────────────────────────────────────────────────
    public static event Action<int, DiceType> OnDiceRolled; // (result, type)

    // ─── Selection ────────────────────────────────────────────────────────────
    public List<DiceItem> GetAvailableDice()
    {
        var available = new List<DiceItem>();
        foreach (var dice in allDice)
        {
            if (dice.isUnlocked) available.Add(dice);
        }
        return available;
    }

    public void SelectDice(DiceType type)
    {
        selectedDice = allDice.Find(d => d.type == type && d.isUnlocked);
        if (selectedDice == null)
            Debug.LogWarning($"[DiceManager] Dadu {type} tidak ditemukan atau terkunci.");
        else
            Debug.Log($"[DiceManager] Dadu dipilih: {selectedDice.displayName}");
    }

    // ─── Roll ─────────────────────────────────────────────────────────────────
    public void RollSelectedDice()
    {
        if (selectedDice == null)
        {
            Debug.LogWarning("[DiceManager] Tidak ada dadu dipilih!");
            return;
        }

        int result = Roll(selectedDice);
        ApplyDiceEffect(result, selectedDice.type);
    }

    private int Roll(DiceItem dice)
    {
        return UnityEngine.Random.Range(dice.minRoll, dice.maxRoll + 1);
    }

    private void ApplyDiceEffect(int result, DiceType type)
    {
        int finalSteps = result;

        switch (type)
        {
            case DiceType.RiskX2:
                finalSteps = result * 2;
                // Tandai BattleManager agar musuh lebih kuat run ini
                GameManager.Instance.BattleManager.SetRiskModifier(true);
                break;

            case DiceType.Blessing:
                // 30% peluang langkah bonus +1
                if (UnityEngine.Random.value < 0.3f)
                {
                    finalSteps += 1;
                    GameManager.Instance.UIManager.ShowFloatingText("Blessing! +1 langkah", Color.yellow);
                }
                break;

            case DiceType.Cursed:
                // 25% peluang kutukan tambah
                if (UnityEngine.Random.value < 0.25f)
                    GameManager.Instance.CurseManager.ApplyRandomCurse();
                break;

            default:
                // Normal & Safe — tidak ada efek tambahan
                break;
        }

        Debug.Log($"[DiceManager] Roll {type}: {result} → {finalSteps} langkah");
        OnDiceRolled?.Invoke(finalSteps, type);

        GameManager.Instance.ChangeState(GameManager.GameState.Moving);
        GameManager.Instance.PlayerController.MoveForward(finalSteps);
    }

    // ─── Unlock ───────────────────────────────────────────────────────────────
    public void UnlockDice(DiceType type)
    {
        var dice = allDice.Find(d => d.type == type);
        if (dice != null)
        {
            dice.isUnlocked = true;
            Debug.Log($"[DiceManager] Dadu di-unlock: {dice.displayName}");
        }
    }
}
