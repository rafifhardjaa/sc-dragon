using UnityEngine;

/// <summary>
/// TileBase — Kelas abstrak dasar untuk semua tile di papan Heroll.
///
/// Setiap tile memiliki:
///   • Indeks posisi di papan
///   • Tipe tile (untuk UI/visual)
///   • Method OnPlayerEnter yang dipanggil saat pion tiba
/// </summary>
public abstract class TileBase : MonoBehaviour
{
    public enum TileType { Normal, Battle, Shop, Event, Boss, Rest, Curse }

    [Header("Tile Info")]
    [SerializeField] private TileType tileType;
    [SerializeField] private SpriteRenderer tileSprite;

    public int TileIndex { get; private set; }
    public TileType Type => tileType;

    public virtual void Initialize(int index)
    {
        TileIndex = index;
    }

    /// <summary>
    /// Dipanggil GameManager.OnPlayerLanded() → GameManager.ChangeState(TileEvent).
    /// Subclass mengimplementasikan logika tile di sini.
    /// </summary>
    public abstract void OnPlayerEnter();

    /// <summary>Dipanggil setelah semua animasi/event tile selesai.</summary>
    protected void FinishTileEvent()
    {
        GameManager.Instance.OnTileEventFinished();
    }
}

