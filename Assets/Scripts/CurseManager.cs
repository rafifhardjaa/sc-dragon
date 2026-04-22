using System;
using System.Collections.Generic;
using UnityEngine;


// ─── Curse Manager ────────────────────────────────────────────────────────────
/// <summary>
/// CurseManager — Mengelola sistem kutukan Heroll.
///
/// Kutukan adalah efek negatif yang menumpuk selama run.
/// Setiap kutukan memiliki efek berbeda (damage, tile skip, dll).
/// </summary>
public class CurseManager : MonoBehaviour
{
    public enum CurseType
    {
        WeakenedBody,   // -2 DEF permanen run ini
        BlindedEyes,    // Hasil dadu tersembunyi
        HeavyBurden,    // -1 langkah tiap roll
        PoisonedBlood,  // Kehilangan 1 HP tiap tile
        GreedyCurse     // Toko harga 20% lebih mahal
    }

    [Serializable]
    public class Curse
    {
        public CurseType type;
        public string displayName;
        [TextArea] public string description;
    }

    [Header("Curse Pool")]
    [SerializeField] private List<Curse> cursePrefabs = new();

    private List<Curse> activeCurses = new();
    public IReadOnlyList<Curse> ActiveCurses => activeCurses;
    public int CurseCount => activeCurses.Count;

    public static event Action<Curse> OnCurseApplied;
    public static event Action<Curse> OnCurseRemoved;

    public void ApplyRandomCurse()
    {
        float chanceMultiplier = GameManager.Instance.RunManager.CurseChanceModifier;
        if (UnityEngine.Random.value > chanceMultiplier)
        {
            Debug.Log("[CurseManager] Kutukan berhasil dihindari (resistensi).");
            return;
        }

        if (cursePrefabs.Count == 0) return;

        Curse curse = cursePrefabs[UnityEngine.Random.Range(0, cursePrefabs.Count)];
        ApplyCurse(curse);
    }

    public void ApplyCurse(Curse curse)
    {
        // Terapkan efek langsung
        switch (curse.type)
        {
            case CurseType.WeakenedBody:
                GameManager.Instance.PlayerController.AddDefense(-2);
                break;
            case CurseType.HeavyBurden:
                // Ditangani di DiceManager saat roll
                break;
        }

        activeCurses.Add(curse);
        OnCurseApplied?.Invoke(curse);
        Debug.Log($"[CurseManager] Kutukan ditambah: {curse.displayName}");
    }

    public bool HasCurse(CurseType type)
    {
        return activeCurses.Exists(c => c.type == type);
    }

    /// <summary>Modifikasi damage masuk berdasarkan kutukan aktif.</summary>
    public int ModifyIncomingDamage(int baseDamage)
    {
        // Contoh: kutukan tertentu bisa meningkatkan damage masuk
        return baseDamage;
    }

    public void RemoveCurse(CurseType type)
    {
        Curse toRemove = activeCurses.Find(c => c.type == type);
        if (toRemove != null)
        {
            activeCurses.Remove(toRemove);
            OnCurseRemoved?.Invoke(toRemove);
            Debug.Log($"[CurseManager] Kutukan dihapus: {toRemove.displayName}");
        }
    }

    public void ClearAll()
    {
        activeCurses.Clear();
    }
}
