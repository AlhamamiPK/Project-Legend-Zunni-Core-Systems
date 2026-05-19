using UnityEngine;

[CreateAssetMenu(fileName ="New Stage",menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    public string stageName;
    public int stageNumber;
    public GameObject parallaxBackGround;
    public GameObject enemySpawners;
    public GameObject bossSpawner;
    public GameObject missoShop;
    public int enemiesToKillBeforeBosses;
}
