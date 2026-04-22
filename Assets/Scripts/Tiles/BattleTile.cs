using UnityEngine;

// ─── Battle Tile ──────────────────────────────────────────────────────────────
/// <summary>Tile pertempuran — trigger BattleManager.</summary>
public class BattleTile : TileBase
{
    [SerializeField] private EnemyData[] possibleEnemies;

    public override void OnPlayerEnter()
    {
        if (possibleEnemies == null || possibleEnemies.Length == 0)
        {
            Debug.LogWarning("[BattleTile] Tidak ada data musuh!");
            FinishTileEvent();
            return;
        }

        EnemyData enemy = possibleEnemies[Random.Range(0, possibleEnemies.Length)];
        GameManager.Instance.BattleManager.SetEnemy(enemy);
        GameManager.Instance.ChangeState(GameManager.GameState.Battle);
    }
}
