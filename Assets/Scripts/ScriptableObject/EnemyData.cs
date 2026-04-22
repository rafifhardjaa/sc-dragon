using UnityEngine;

// ─── Enemy Data ───────────────────────────────────────────────────────────────
/// <summary>Data musuh — dibuat sebagai ScriptableObject di Unity Editor.</summary>
[CreateAssetMenu(fileName = "EnemyData", menuName = "Heroll/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Musuh";
    public Sprite sprite;
    [TextArea] public string description;

    [Header("Stats")]
    public int maxHP = 10;
    public int attackPower = 3;
    public int defense = 0;

    [Header("Rewards")]
    public int scalesReward = 3;
    public int xpReward = 1;

    [Header("Special")]
    public bool isBoss = false;
    public float specialAbilityChance = 0f; // 0 = tidak ada
}
