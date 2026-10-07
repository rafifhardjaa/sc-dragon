using System;
using UnityEngine;

/// <summary>
/// GameManager — Singleton utama Heroll.
/// Mengatur state mesin seluruh game dan menjadi titik referensi
/// bagi semua manager lain.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ─── State Machine ────────────────────────────────────────────────────────
    public enum GameState
    {
        MainMenu,
        DiceSelect,     // Pemain memilih jenis dadu sebelum giliran
        Moving,         // Pion sedang bergerak di papan
        TileEvent,      // Tile sedang diproses (decision, reward, dll)
        Battle,         // Sedang dalam pertarungan
        Shopping,       // Pemain di ShopTile
        Paused,
        GameOver,       // Pemain mati → run berakhir
        Victory         // Pemain menyelesaikan papan
    }

    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    // ─── Events ───────────────────────────────────────────────────────────────
    public static event Action<GameState> OnGameStateChanged;

    // ─── Manager References ───────────────────────────────────────────────────
    [Header("Manager References")]
    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public DiceManager DiceManager;
    public BattleManager BattleManager;
    public CurrencyManager CurrencyManager;
    public InventorySystem InventorySystem;
    public CurseManager CurseManager;
    public BlessingManager BlessingManager;
    public ShopManager ShopManager;
    public RunManager RunManager;
    public SaveSystem SaveSystem;
    public UIManager UIManager;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Load meta-progress dari save
        SaveSystem.Load(() => ChangeState(GameState.MainMenu));
    }

    // ─── State Transition ─────────────────────────────────────────────────────
    /// <summary>
    /// Satu-satunya cara mengubah state game.
    /// Semua sistem mendengarkan event OnGameStateChanged.
    /// </summary>
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);

        HandleStateEntry(newState);
    }

    private void HandleStateEntry(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                UIManager.ShowMainMenu();
                break;

            case GameState.DiceSelect:
                UIManager.ShowDiceSelection();
                break;

            case GameState.Moving:
                // MovementHandler akan memulai pergerakan saat state ini aktif
                break;

            case GameState.TileEvent:
                // TileBase.OnPlayerEnter() sudah menangani ini
                break;

            case GameState.Battle:
                BattleManager.StartBattle();
                break;

            case GameState.Shopping:
                ShopManager.OpenShop();
                break;

            case GameState.GameOver:
                HandleGameOver();
                break;

            case GameState.Victory:
                HandleVictory();
                break;
        }
    }

    // ─── Game Flow ────────────────────────────────────────────────────────────
    /// <summary>Dipanggil RunManager saat memulai run baru.</summary>
    public void StartNewRun()
    {
        BoardManager.GenerateBoard();
        PlayerController.ResetForNewRun();
        CurrencyManager.ResetForNewRun();
        InventorySystem.ResetForNewRun();
        CurseManager.ClearAll();
        BlessingManager.ClearAll();

        ChangeState(GameState.DiceSelect);
    }

    /// <summary>Dipanggil saat giliran pemain dimulai (setelah tile event selesai).</summary>
    public void BeginPlayerTurn()
    {
        ChangeState(GameState.DiceSelect);
    }

    /// <summary>Dipanggil MovementHandler setelah pion tiba di tile tujuan.</summary>
    public void OnPlayerLanded()
    {
        ChangeState(GameState.TileEvent);
    }

    /// <summary>Dipanggil tile/event saat semua interaksi selesai.</summary>
    public void OnTileEventFinished()
    {
        if (PlayerController.IsDead)
        {
            ChangeState(GameState.GameOver);
            return;
        }
        BeginPlayerTurn();
    }

    private void HandleGameOver()
    {
        // Simpan reward meta sebelum reset
        RunManager.OnRunFailed();
        SaveSystem.Save();
        UIManager.ShowGameOverScreen();
        AdManager.ShowInterstitial("game_over");
    }

    private void HandleVictory()
    {
        RunManager.OnRunCompleted();
        SaveSystem.Save();
        UIManager.ShowVictoryScreen();
        AdManager.ShowInterstitial("victory");
    }

    // ─── Utility ──────────────────────────────────────────────────────────────
    public bool IsState(GameState state) => CurrentState == state;

    public void QuitToMainMenu()
    {
        SaveSystem.Save();
        ChangeState(GameState.MainMenu);
    }

    private void OnApplicationQuit()
    {
        SaveSystem.Save();
    }
}