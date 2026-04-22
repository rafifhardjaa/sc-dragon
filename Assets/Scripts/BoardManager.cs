using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BoardManager — Mengelola papan permainan Heroll.
///
/// Tanggung jawab:
///   • Generate papan tile secara prosedural tiap run
///   • Menyimpan referensi semua tile di papan
///   • Memberikan tile berdasarkan indeks atau posisi
///   • Mengelola tile khusus (Start, Boss, Shop, dll)
/// </summary>
public class BoardManager : MonoBehaviour
{
    // ─── Config ───────────────────────────────────────────────────────────────
    [Header("Board Config")]
    [SerializeField] private int totalTiles = 100;
    [SerializeField] private Transform tileParent;

    [Header("Tile Prefabs")]
    [SerializeField] private TileBase normalTilePrefab;
    [SerializeField] private TileBase battleTilePrefab;
    [SerializeField] private TileBase shopTilePrefab;
    [SerializeField] private TileBase eventTilePrefab;
    [SerializeField] private TileBase bossTilePrefab;
    [SerializeField] private TileBase restTilePrefab;
    [SerializeField] private TileBase curseTilePrefab;

    // ─── Spawn Weights ────────────────────────────────────────────────────────
    [Header("Tile Spawn Weights (1-100)")]
    [SerializeField] private int weightBattle = 35;
    [SerializeField] private int weightShop = 10;
    [SerializeField] private int weightEvent = 20;
    [SerializeField] private int weightRest = 15;
    [SerializeField] private int weightCurse = 10;
    // Normal mengisi sisa

    // ─── Runtime State ────────────────────────────────────────────────────────
    private List<TileBase> tiles = new();

    // ─── Fixed Tile Positions ─────────────────────────────────────────────────
    private const int BOSS_TILE_INDEX = 99;   // Tile terakhir selalu Boss
    private const int SHOP_TILE_INDEX_1 = 24; // Shop pertama ~tile 25
    private const int SHOP_TILE_INDEX_2 = 49; // Shop kedua ~tile 50
    private const int SHOP_TILE_INDEX_3 = 74; // Shop ketiga ~tile 75

    // ─── Events ───────────────────────────────────────────────────────────────
    public static event Action OnBoardGenerated;

    // ─── Public Access ────────────────────────────────────────────────────────
    public int TotalTiles => totalTiles;
    public TileBase GetTile(int index)
    {
        if (index < 0 || index >= tiles.Count) return null;
        return tiles[index];
    }

    // ─── Generate Board ───────────────────────────────────────────────────────
    public void GenerateBoard()
    {
        ClearBoard();

        for (int i = 0; i < totalTiles; i++)
        {
            TileBase prefab = SelectTilePrefab(i);
            TileBase tile = Instantiate(prefab, tileParent);
            tile.Initialize(i);
            tiles.Add(tile);
        }

        Debug.Log($"[BoardManager] Papan di-generate: {totalTiles} tile.");
        OnBoardGenerated?.Invoke();
    }

    private TileBase SelectTilePrefab(int index)
    {
        // Tile pertama selalu "safe" (tidak ada event)
        if (index == 0) return normalTilePrefab;

        // Tile terakhir selalu Boss
        if (index == BOSS_TILE_INDEX) return bossTilePrefab;

        // Tile shop tetap
        if (index == SHOP_TILE_INDEX_1 || index == SHOP_TILE_INDEX_2 || index == SHOP_TILE_INDEX_3)
            return shopTilePrefab;

        // Tile 5 pertama aman (tutorial area)
        if (index < 5) return normalTilePrefab;

        return GetWeightedRandom();
    }

    private TileBase GetWeightedRandom()
    {
        int roll = UnityEngine.Random.Range(0, 100);
        int cursor = 0;

        cursor += weightBattle;
        if (roll < cursor) return battleTilePrefab;

        cursor += weightShop;
        if (roll < cursor) return shopTilePrefab;

        cursor += weightEvent;
        if (roll < cursor) return eventTilePrefab;

        cursor += weightRest;
        if (roll < cursor) return restTilePrefab;

        cursor += weightCurse;
        if (roll < cursor) return curseTilePrefab;

        return normalTilePrefab;
    }

    // ─── Clear ────────────────────────────────────────────────────────────────
    private void ClearBoard()
    {
        foreach (var tile in tiles)
        {
            if (tile != null) Destroy(tile.gameObject);
        }
        tiles.Clear();
    }
}