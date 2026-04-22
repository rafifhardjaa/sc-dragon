using UnityEngine;

public class NormalTile : TileBase
{
    public override void OnPlayerEnter()
    {
        // Tidak ada efek
        FinishTileEvent();
    }
}
