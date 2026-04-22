using System;
using System.Collections.Generic;
using UnityEngine;


// ─── Inventory System ─────────────────────────────────────────────────────────
/// <summary>
/// InventorySystem — Menyimpan artifact yang dimiliki pemain dalam satu run.
///
/// Artifact memberikan efek pasif atau aktif selama run berlangsung.
/// </summary>
public class InventorySystem : MonoBehaviour
{
    [Header("Artifact Database")]
    [SerializeField] private List<ArtifactData> artifactDatabase = new();

    private List<ArtifactData> heldArtifacts = new();
    public IReadOnlyList<ArtifactData> HeldArtifacts => heldArtifacts;

    public static event Action<ArtifactData> OnArtifactAdded;

    public void ResetForNewRun()
    {
        heldArtifacts.Clear();
    }

    public void AddArtifact(string artifactID)
    {
        ArtifactData data = artifactDatabase.Find(a => a.artifactID == artifactID);
        if (data == null)
        {
            Debug.LogWarning($"[InventorySystem] Artifact tidak ditemukan: {artifactID}");
            return;
        }

        // Artifact unik — cek duplikat
        if (data.isUnique && heldArtifacts.Exists(a => a.artifactID == artifactID))
        {
            Debug.Log($"[InventorySystem] Artifact unik sudah dimiliki: {artifactID}");
            return;
        }

        heldArtifacts.Add(data);
        ApplyPassiveEffect(data);
        OnArtifactAdded?.Invoke(data);
        Debug.Log($"[InventorySystem] Artifact ditambah: {data.artifactName}");
    }

    private void ApplyPassiveEffect(ArtifactData data)
    {
        if (data.passiveBonusHP    != 0) GameManager.Instance.PlayerController.AddMaxHP(data.passiveBonusHP);
        if (data.passiveBonusATK   != 0) GameManager.Instance.PlayerController.AddAttack(data.passiveBonusATK);
        if (data.passiveBonusDEF   != 0) GameManager.Instance.PlayerController.AddDefense(data.passiveBonusDEF);
    }

    public bool HasArtifact(string artifactID)
    {
        return heldArtifacts.Exists(a => a.artifactID == artifactID);
    }
}
