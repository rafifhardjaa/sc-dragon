using UnityEngine;

// ─── Boss Tile ────────────────────────────────────────────────────────────────
/// <summary>Tile boss — trigger pertarungan boss terakhir.</summary>
public class BossTile : TileBase
{
    [SerializeField] private EnemyData bossData;

    public override void OnPlayerEnter()
    {
        GameManager.Instance.BattleManager.SetEnemy(bossData, isBoss: true);
        GameManager.Instance.ChangeState(GameManager.GameState.Battle);
    }
}
