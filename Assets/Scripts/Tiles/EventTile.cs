using UnityEngine;

// ─── Event Tile ───────────────────────────────────────────────────────────────
/// <summary>Tile event acak — pilihan yang memengaruhi run.</summary>
public class EventTile : TileBase
{
    [SerializeField] private EventData[] possibleEvents;

    public override void OnPlayerEnter()
    {
        if (possibleEvents == null || possibleEvents.Length == 0)
        {
            FinishTileEvent();
            return;
        }

        EventData ev = possibleEvents[Random.Range(0, possibleEvents.Length)];
        GameManager.Instance.UIManager.ShowEventPanel(ev, OnEventResolved);
    }

    private void OnEventResolved()
    {
        FinishTileEvent();
    }
}
