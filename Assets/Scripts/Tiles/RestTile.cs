using UnityEngine;

// ─── Rest Tile ────────────────────────────────────────────────────────────────
/// <summary>Tile istirahat — memulihkan HP pemain.</summary>
public class RestTile : TileBase
{
    [SerializeField] private int healAmount = 3;

    public override void OnPlayerEnter()
    {
        GameManager.Instance.PlayerController.Heal(healAmount);
        GameManager.Instance.UIManager.ShowFloatingText($"+{healAmount} HP", Color.green);
        FinishTileEvent();
    }
}
