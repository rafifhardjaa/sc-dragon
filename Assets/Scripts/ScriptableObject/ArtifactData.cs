using UnityEngine;

// ─── Artifact Data ────────────────────────────────────────────────────────────
/// <summary>Data artifact — item pasif yang dipegang pemain selama run.</summary>
[CreateAssetMenu(fileName = "ArtifactData", menuName = "Heroll/Artifact Data")]
public class ArtifactData : ScriptableObject
{
    [Header("Identity")]
    public string artifactID;
    public string artifactName;
    [TextArea] public string description;
    public Sprite icon;
    public bool isUnique = true;

    [Header("Passive Bonuses")]
    public int passiveBonusHP  = 0;
    public int passiveBonusATK = 0;
    public int passiveBonusDEF = 0;

    [Header("Special Trigger")]
    public ArtifactTrigger trigger = ArtifactTrigger.None;
    public int triggerValue = 0;

    public enum ArtifactTrigger
    {
        None,
        OnEnemyKilled,
        OnTileEnter,
        OnDiceRoll,
        OnCurseApplied
    }
}
