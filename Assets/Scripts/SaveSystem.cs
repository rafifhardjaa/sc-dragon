using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using Playgama;
#endif

public class SaveSystem : MonoBehaviour
{
    private const string Key = "heroll_save";
    private string SavePath => Path.Combine(Application.persistentDataPath, "heroll_save.json");
    private SaveData data = new();

    public int MetaScales => data.metaScales;
    public int TotalRuns => data.totalRuns;
    public int HighestTileEver => data.highestTileEver;

    public void Load(Action onDone = null)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Bridge.storage.Get(Key, (success, json) =>
        {
            Apply(success ? json : null);
            onDone?.Invoke();
        });
#else
        Apply(File.Exists(SavePath) ? File.ReadAllText(SavePath) : null);
        onDone?.Invoke();
#endif
    }

    private void Apply(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            data = new SaveData();
            Debug.Log("[SaveSystem] Tidak ada save. Data baru dibuat.");
            return;
        }
        try
        {
            data = JsonUtility.FromJson<SaveData>(json);
            GameManager.Instance.RunManager.LoadUnlocks(data.unlockedIDs);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Gagal load: {e.Message}. Reset ke default.");
            data = new SaveData();
        }
    }

    public void Save()
    {
        try
        {
            data.unlockedIDs = GameManager.Instance.RunManager.GetUnlocksForSave();
            data.totalRuns = GameManager.Instance.RunManager.CurrentRunNumber;
            string json = JsonUtility.ToJson(data);
#if UNITY_WEBGL && !UNITY_EDITOR
            Bridge.storage.Set(Key, json, success =>
                Debug.Log($"[SaveSystem] Simpan ke Bridge: {success}"));
#else
            File.WriteAllText(SavePath, json);
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Gagal menyimpan: {e.Message}");
        }
    }

    public void AddMetaScales(int amount) => data.metaScales = Mathf.Max(0, data.metaScales + amount);

    public bool SpendMetaScales(int amount)
    {
        if (data.metaScales < amount) return false;
        data.metaScales -= amount;
        return true;
    }

    public void UpdateHighestTile(int tile)
    {
        if (tile > data.highestTileEver) data.highestTileEver = tile;
    }

    public void DeleteSave()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Bridge.storage.Delete(Key, _ => { });
#else
        if (File.Exists(SavePath)) File.Delete(SavePath);
#endif
        data = new SaveData();
    }
}

[Serializable]
public class SaveData
{
    public int metaScales = 0;
    public int totalRuns = 1;
    public int highestTileEver = 0;
    public List<string> unlockedIDs = new();
}
