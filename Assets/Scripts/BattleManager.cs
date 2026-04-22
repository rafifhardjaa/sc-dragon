using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// BattleManager — Mengelola sistem pertarungan turn-based Heroll.
///
/// Alur pertarungan:
///   1. BattleTile / BossTile memanggil SetEnemy() lalu GameState → Battle
///   2. StartBattle() memulai giliran pemain
///   3. Pemain pilih aksi → PlayerAction()
///   4. Musuh balas → EnemyTurn()
///   5. Cek kondisi menang/kalah → EndBattle()
/// </summary>
public class BattleManager : MonoBehaviour
{
    // ─── Runtime Battle State ─────────────────────────────────────────────────
    [Header("Battle State (read-only)")]
    [SerializeField] private EnemyData currentEnemy;
    [SerializeField] private int enemyCurrentHP;
    [SerializeField] private bool isBossBattle;
    [SerializeField] private bool riskModifierActive;

    public EnemyData CurrentEnemy => currentEnemy;
    public int EnemyCurrentHP => enemyCurrentHP;
    public bool IsBossBattle => isBossBattle;

    // ─── Events ───────────────────────────────────────────────────────────────
    public static event Action<EnemyData> OnBattleStarted;
    public static event Action<int> OnEnemyDamaged;    // (newHP)
    public static event Action<int> OnPlayerDamaged;   // (damage dealt)
    public static event Action<bool> OnBattleEnded;    // (victory)

    // ─── Setup ────────────────────────────────────────────────────────────────
    public void SetEnemy(EnemyData enemy, bool isBoss = false)
    {
        currentEnemy = enemy;
        isBossBattle = isBoss;
        enemyCurrentHP = riskModifierActive
            ? Mathf.RoundToInt(enemy.maxHP * 1.5f)
            : enemy.maxHP;
    }

    public void SetRiskModifier(bool active)
    {
        riskModifierActive = active;
    }

    // ─── Battle Flow ──────────────────────────────────────────────────────────
    /// <summary>Dipanggil GameManager.HandleStateEntry(Battle).</summary>
    public void StartBattle()
    {
        if (currentEnemy == null)
        {
            Debug.LogError("[BattleManager] Tidak ada musuh! Battle dibatalkan.");
            GameManager.Instance.OnTileEventFinished();
            return;
        }

        riskModifierActive = false;
        OnBattleStarted?.Invoke(currentEnemy);
        Debug.Log($"[BattleManager] Battle dimulai vs {currentEnemy.enemyName} (HP:{enemyCurrentHP})");
    }

    // ─── Player Actions ───────────────────────────────────────────────────────
    /// <summary>Pemain memilih aksi Attack.</summary>
    public void PlayerAttack()
    {
        int dmg = GameManager.Instance.PlayerController.Attack;

        // Terapkan modifier artifact/blessing
        dmg = GameManager.Instance.BlessingManager.ModifyPlayerAttack(dmg);

        enemyCurrentHP -= dmg;
        OnEnemyDamaged?.Invoke(enemyCurrentHP);
        Debug.Log($"[BattleManager] Pemain serang: -{dmg}. Musuh HP: {enemyCurrentHP}");

        if (enemyCurrentHP <= 0)
        {
            EndBattle(victory: true);
            return;
        }

        StartCoroutine(EnemyTurnRoutine());
    }

    /// <summary>Pemain memilih aksi Defend — kurangi damage masuk.</summary>
    public void PlayerDefend()
    {
        // Satu giliran buff defense
        GameManager.Instance.PlayerController.AddDefense(2);
        Debug.Log("[BattleManager] Pemain bertahan: +2 DEF sementara.");

        StartCoroutine(EnemyTurnRoutine());
    }

    /// <summary>Pemain memilih kabur — hanya bisa jika bukan Boss.</summary>
    public bool PlayerFlee()
    {
        if (isBossBattle)
        {
            Debug.Log("[BattleManager] Tidak bisa kabur dari Boss!");
            return false;
        }

        // 50% kemungkinan kabur berhasil
        bool success = UnityEngine.Random.value > 0.5f;
        if (success)
        {
            Debug.Log("[BattleManager] Pemain berhasil kabur.");
            EndBattle(victory: false, fled: true);
        }
        else
        {
            Debug.Log("[BattleManager] Kabur gagal! Musuh menyerang.");
            StartCoroutine(EnemyTurnRoutine());
        }
        return success;
    }

    // ─── Enemy Turn ───────────────────────────────────────────────────────────
    private IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(0.8f);

        int dmg = currentEnemy.attackPower;
        // Kutukan bisa memodifikasi damage masuk
        dmg = GameManager.Instance.CurseManager.ModifyIncomingDamage(dmg);

        OnPlayerDamaged?.Invoke(dmg);
        GameManager.Instance.PlayerController.TakeDamage(dmg);

        Debug.Log($"[BattleManager] Musuh serang: -{dmg} ke pemain.");

        // Cek apakah pemain mati (sudah ditangani PlayerController)
    }

    // ─── End Battle ───────────────────────────────────────────────────────────
    private void EndBattle(bool victory, bool fled = false)
    {
        if (victory)
        {
            GameManager.Instance.RunManager.RecordEnemyDefeated();

            // Reward sisik dari musuh
            int scales = currentEnemy.scalesReward;
            GameManager.Instance.CurrencyManager.AddScales(scales);
            GameManager.Instance.RunManager.RecordScalesEarned(scales);

            // Boss → Victory state
            if (isBossBattle)
            {
                GameManager.Instance.ChangeState(GameManager.GameState.Victory);
                return;
            }

            Debug.Log($"[BattleManager] Musuh dikalahkan! +{scales} sisik.");
        }
        else
        {
            Debug.Log(fled ? "[BattleManager] Pemain kabur." : "[BattleManager] Pemain mati.");
        }

        OnBattleEnded?.Invoke(victory);
        GameManager.Instance.OnTileEventFinished();
    }
}
