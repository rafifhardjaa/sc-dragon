using UnityEngine;

// ─── Curse Tile ───────────────────────────────────────────────────────────────
/// <summary>Tile kutukan — menambah kutukan acak ke pemain.</summary>
public class CurseTile : TileBase
{
    public override void OnPlayerEnter()
    {
        GameManager.Instance.CurseManager.ApplyRandomCurse();
        FinishTileEvent();
    }
}
