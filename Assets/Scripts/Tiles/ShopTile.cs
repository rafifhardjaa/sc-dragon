using UnityEngine;

// ─── Shop Tile ────────────────────────────────────────────────────────────────
/// <summary>Tile toko — membuka ShopManager.</summary>
public class ShopTile : TileBase
{
    public override void OnPlayerEnter()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Shopping);
    }
}
