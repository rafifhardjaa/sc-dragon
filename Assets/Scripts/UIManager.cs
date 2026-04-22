using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UIManager — Mengelola semua tampilan UI Heroll.
///
/// Tanggung jawab:
///   • Menampilkan/menyembunyikan panel (MainMenu, Battle, Shop, GameOver, dll)
///   • Update HUD (HP bar, sisik, tile index)
///   • Floating text (damage, heal, dll)
///   • Menghubungkan event game ke visual UI
/// </summary>
public class UIManager : MonoBehaviour
{
    // ─── Panels ───────────────────────────────────────────────────────────────
    [Header("Panels")]
    [SerializeField] private GameObject panelMainMenu;
    [SerializeField] private GameObject panelHUD;
    [SerializeField] private GameObject panelDiceSelect;
    [SerializeField] private GameObject panelBattle;
    [SerializeField] private GameObject panelShop;
    [SerializeField] private GameObject panelEvent;
    [SerializeField] private GameObject panelGameOver;
    [SerializeField] private GameObject panelVictory;
    [SerializeField] private GameObject panelRunSummary;

    // ─── HUD Elements ─────────────────────────────────────────────────────────
    [Header("HUD")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text scalesText;
    [SerializeField] private TMP_Text tileIndexText;
    [SerializeField] private TMP_Text runNumberText;
    [SerializeField] private Transform curseIconContainer;
    [SerializeField] private Transform blessingIconContainer;

    // ─── Battle UI ────────────────────────────────────────────────────────────
    [Header("Battle UI")]
    [SerializeField] private TMP_Text enemyNameText;
    [SerializeField] private Slider enemyHPBar;
    [SerializeField] private TMP_Text enemyHPText;
    [SerializeField] private Image enemySprite;
    [SerializeField] private Button btnAttack;
    [SerializeField] private Button btnDefend;
    [SerializeField] private Button btnFlee;

    // ─── Event UI ─────────────────────────────────────────────────────────────
    [Header("Event UI")]
    [SerializeField] private TMP_Text eventTitleText;
    [SerializeField] private TMP_Text eventFlavorText;
    [SerializeField] private Image eventIllustration;
    [SerializeField] private Transform choiceButtonContainer;
    [SerializeField] private Button choiceButtonPrefab;

    // ─── Game Over / Victory ──────────────────────────────────────────────────
    [Header("End Screens")]
    [SerializeField] private TMP_Text gameOverTileText;
    [SerializeField] private TMP_Text victoryRunText;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private TMP_Text metaScalesEarnedText;

    // ─── Floating Text ────────────────────────────────────────────────────────
    [Header("Floating Text")]
    [SerializeField] private FloatingText floatingTextPrefab;
    [SerializeField] private Canvas worldCanvas;

    // ─── Init ─────────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Subscribe ke event game
        PlayerController.OnHPChanged    += UpdateHPBar;
        PlayerController.OnPlayerMoved  += UpdateTileIndex;
        CurrencyManager.OnScalesChanged += UpdateScalesText;
        BattleManager.OnBattleStarted   += SetupBattleUI;
        BattleManager.OnEnemyDamaged    += UpdateEnemyHP;
        CurseManager.OnCurseApplied     += RefreshCurseIcons;
        BlessingManager.OnBlessingApplied += RefreshBlessingIcons;
        RunManager.OnRunEnded           += ShowRunSummary;
    }

    private void OnDestroy()
    {
        PlayerController.OnHPChanged    -= UpdateHPBar;
        PlayerController.OnPlayerMoved  -= UpdateTileIndex;
        CurrencyManager.OnScalesChanged -= UpdateScalesText;
        BattleManager.OnBattleStarted   -= SetupBattleUI;
        BattleManager.OnEnemyDamaged    -= UpdateEnemyHP;
        CurseManager.OnCurseApplied     -= RefreshCurseIcons;
        BlessingManager.OnBlessingApplied -= RefreshBlessingIcons;
        RunManager.OnRunEnded           -= ShowRunSummary;
    }

    // ─── Panel Display ────────────────────────────────────────────────────────
    private void HideAll()
    {
        panelMainMenu?.SetActive(false);
        panelHUD?.SetActive(false);
        panelDiceSelect?.SetActive(false);
        panelBattle?.SetActive(false);
        panelShop?.SetActive(false);
        panelEvent?.SetActive(false);
        panelGameOver?.SetActive(false);
        panelVictory?.SetActive(false);
        panelRunSummary?.SetActive(false);
    }

    public void ShowMainMenu()
    {
        HideAll();
        panelMainMenu?.SetActive(true);
    }

    public void ShowDiceSelection()
    {
        panelHUD?.SetActive(true);
        panelDiceSelect?.SetActive(true);
        panelBattle?.SetActive(false);
        panelShop?.SetActive(false);
        panelEvent?.SetActive(false);
    }

    public void ShowShopPanel()
    {
        panelShop?.SetActive(true);
    }

    public void ShowGameOverScreen()
    {
        panelHUD?.SetActive(false);
        panelGameOver?.SetActive(true);
        if (gameOverTileText != null)
            gameOverTileText.text = $"Tile tertinggi: {GameManager.Instance.RunManager.CurrentRunNumber}";
    }

    public void ShowVictoryScreen()
    {
        panelHUD?.SetActive(false);
        panelVictory?.SetActive(true);
    }

    // ─── HUD Updates ──────────────────────────────────────────────────────────
    private void UpdateHPBar(int current, int max)
    {
        if (hpBar != null) { hpBar.maxValue = max; hpBar.value = current; }
        if (hpText != null) hpText.text = $"{current}/{max}";
    }

    private void UpdateTileIndex(int index)
    {
        if (tileIndexText != null)
            tileIndexText.text = $"Tile {index + 1}/{GameManager.Instance.BoardManager.TotalTiles}";
    }

    private void UpdateScalesText(int amount)
    {
        if (scalesText != null)
            scalesText.text = $"🐉 {amount}";
    }

    // ─── Battle UI Setup ──────────────────────────────────────────────────────
    private void SetupBattleUI(EnemyData enemy)
    {
        panelBattle?.SetActive(true);
        panelDiceSelect?.SetActive(false);

        if (enemyNameText  != null) enemyNameText.text   = enemy.enemyName;
        if (enemySprite    != null) enemySprite.sprite   = enemy.sprite;
        if (enemyHPBar     != null) { enemyHPBar.maxValue = enemy.maxHP; enemyHPBar.value = enemy.maxHP; }
        if (enemyHPText    != null) enemyHPText.text     = $"{enemy.maxHP}/{enemy.maxHP}";

        // Wire up buttons
        btnAttack?.onClick.RemoveAllListeners();
        btnDefend?.onClick.RemoveAllListeners();
        btnFlee?.onClick.RemoveAllListeners();

        btnAttack?.onClick.AddListener(() => GameManager.Instance.BattleManager.PlayerAttack());
        btnDefend?.onClick.AddListener(() => GameManager.Instance.BattleManager.PlayerDefend());
        btnFlee?.onClick.AddListener(()   => GameManager.Instance.BattleManager.PlayerFlee());
    }

    private void UpdateEnemyHP(int newHP)
    {
        if (enemyHPBar != null) enemyHPBar.value = newHP;
        if (enemyHPText != null)
        {
            EnemyData e = GameManager.Instance.BattleManager.CurrentEnemy;
            if (e != null) enemyHPText.text = $"{Mathf.Max(0, newHP)}/{e.maxHP}";
        }
    }

    // ─── Event Panel ──────────────────────────────────────────────────────────
    public void ShowEventPanel(EventData eventData, Action onResolved)
    {
        panelEvent?.SetActive(true);
        if (eventTitleText  != null) eventTitleText.text  = eventData.eventTitle;
        if (eventFlavorText != null) eventFlavorText.text = eventData.flavorText;
        if (eventIllustration != null && eventData.illustration != null)
            eventIllustration.sprite = eventData.illustration;

        // Hapus tombol pilihan lama
        if (choiceButtonContainer != null)
        {
            foreach (Transform child in choiceButtonContainer)
                Destroy(child.gameObject);
        }

        // Buat tombol tiap pilihan
        foreach (var choice in eventData.choices)
        {
            var btn = Instantiate(choiceButtonPrefab, choiceButtonContainer);
            btn.GetComponentInChildren<TMP_Text>().text = choice.label;

            EventChoice captured = choice;
            btn.onClick.AddListener(() =>
            {
                ApplyEventChoice(captured);
                panelEvent?.SetActive(false);
                onResolved?.Invoke();
            });
        }
    }

    private void ApplyEventChoice(EventChoice choice)
    {
        switch (choice.effectType)
        {
            case EventChoice.EventEffectType.Heal:
                GameManager.Instance.PlayerController.Heal(choice.value);
                ShowFloatingText($"+{choice.value} HP", Color.green);
                break;
            case EventChoice.EventEffectType.Damage:
                GameManager.Instance.PlayerController.TakeDamage(choice.value);
                ShowFloatingText($"-{choice.value} HP", Color.red);
                break;
            case EventChoice.EventEffectType.GainScales:
                GameManager.Instance.CurrencyManager.AddScales(choice.value);
                ShowFloatingText($"+{choice.value} Sisik", Color.yellow);
                break;
            case EventChoice.EventEffectType.LoseScales:
                GameManager.Instance.CurrencyManager.SpendScales(choice.value);
                break;
            case EventChoice.EventEffectType.GainCurse:
                GameManager.Instance.CurseManager.ApplyRandomCurse();
                break;
            case EventChoice.EventEffectType.GainBlessing:
                GameManager.Instance.BlessingManager.ApplyBlessing(BlessingManager.BlessingType.DragonScale);
                break;
        }
    }

    // ─── Run Summary ──────────────────────────────────────────────────────────
    private void ShowRunSummary(RunSummary summary)
    {
        panelRunSummary?.SetActive(true);
        if (summaryText != null)
        {
            summaryText.text =
                $"Run #{summary.RunNumber}\n" +
                $"Tile tertinggi: {summary.HighestTile}\n" +
                $"Musuh dikalahkan: {summary.EnemiesDefeated}\n" +
                $"Sisik terkumpul: {summary.ScalesEarned}";
        }
    }

    // ─── Curse / Blessing Icons ───────────────────────────────────────────────
    private void RefreshCurseIcons(CurseManager.Curse curse)
    {
        // TODO: Instantiate ikon kutukan ke curseIconContainer
        Debug.Log($"[UIManager] Refresh ikon kutukan: {curse.displayName}");
    }

    private void RefreshBlessingIcons(BlessingManager.Blessing blessing)
    {
        // TODO: Instantiate ikon berkat ke blessingIconContainer
        Debug.Log($"[UIManager] Refresh ikon berkat: {blessing.displayName}");
    }

    // ─── Floating Text ────────────────────────────────────────────────────────
    public void ShowFloatingText(string message, Color color)
    {
        if (floatingTextPrefab == null || worldCanvas == null) return;

        FloatingText ft = Instantiate(floatingTextPrefab, worldCanvas.transform);
        ft.Play(message, color);
    }
}

// ─── Floating Text Helper ─────────────────────────────────────────────────────
/// <summary>Komponen teks mengambang di layar (damage, heal, dll).</summary>
public class FloatingText : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Animator animator;

    public void Play(string message, Color color)
    {
        if (label != null)
        {
            label.text  = message;
            label.color = color;
        }
        animator?.SetTrigger("Play");
        Destroy(gameObject, 1.5f);
    }
}
