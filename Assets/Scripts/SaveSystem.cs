using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// SaveSystem — Menyimpan dan memuat meta-progression Heroll.
///
/// Yang disimpan (persists antar run):
///   • Total Sisik Naga meta (beda dari sisik in-run)
///   • Daftar konten yang sudah di-unlock
///   • Total run yang pernah dimainkan
///   • High score (tile tertinggi yang pernah dicapai)
///
/// Format: JSON file di Application.persistentDataPath.
/// </summary>
public class SaveSystem : MonoBehaviour
{
    // ─── Path ─────────────────────────────────────────────────────────────────
    private string SavePath => Path.Combine(Application.persistentDataPath, "heroll_save.json");

    // ─── In-Memory State ──────────────────────────────────────────────────────
    private SaveData data = new();

    // ─── Public Accessors ─────────────────────────────────────────────────────
    public int MetaScales => data.metaScales;
    public int TotalRuns => data.totalRuns;
    public int HighestTileEver => data.highestTileEver;

    // ─── Load ─────────────────────────────────────────────────────────────────
    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            data = new SaveData();
            Debug.Log("[SaveSystem] Tidak ada save file. Data baru dibuat.");
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"[SaveSystem] Save dimuat. Meta sisik: {data.metaScales}, Total run: {data.totalRuns}");

            // Kirim data unlock ke RunManager
            GameManager.Instance.RunManager.LoadUnlocks(data.unlockedIDs);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Gagal load save: {e.Message}. Reset ke default.");
            data = new SaveData();
        }
    }

    // ─── Save ─────────────────────────────────────────────────────────────────
    public void Save()
    {
        try
        {
            // Ambil data unlock terkini dari RunManager sebelum simpan
            data.unlockedIDs = GameManager.Instance.RunManager.GetUnlocksForSave();
            data.totalRuns = GameManager.Instance.RunManager.CurrentRunNumber;

            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[SaveSystem] Game tersimpan. Meta sisik: {data.metaScales}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Gagal menyimpan: {e.Message}");
        }
    }

    // ─── Mutation Methods ─────────────────────────────────────────────────────
    public void AddMetaScales(int amount)
    {
        data.metaScales = Mathf.Max(0, data.metaScales + amount);
    }

    public bool SpendMetaScales(int amount)
    {
        if (data.metaScales < amount) return false;
        data.metaScales -= amount;
        return true;
    }

    public void UpdateHighestTile(int tile)
    {
        if (tile > data.highestTileEver)
            data.highestTileEver = tile;
    }

    // ─── Delete Save ──────────────────────────────────────────────────────────
    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        data = new SaveData();
        Debug.Log("[SaveSystem] Save file dihapus.");
    }
}

// ─── Save Data Model ──────────────────────────────────────────────────────────
[Serializable]
public class SaveData
{
    public int metaScales = 0;
    public int totalRuns = 1;
    public int highestTileEver = 0;
    public List<string> unlockedIDs = new();
}