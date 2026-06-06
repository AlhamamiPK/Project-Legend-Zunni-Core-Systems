using UnityEngine;
/// <summary>
/// StageData: Defines the configuration for a single stage: name, number, scene objects, and win condition.
/// Create via: Assets --> Create --> Game --> Stage Data
/// </summary>
[CreateAssetMenu(fileName ="New Stage",menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Display name shown in the UI.")]
    public string stageName;
    [Tooltip("Stage index used for progression logic.")]
    public int stageNumber;

    [Header("Scene Objects")]
    public GameObject parallaxBackGround;
    public GameObject enemySpawners;
    public GameObject bossSpawner;
    public GameObject missoShop;

    [Header("Win Condition")]
    [Tooltip("Enemies to kill before the boss spawns.")]
    public int enemiesToKillBeforeBosses;
}
