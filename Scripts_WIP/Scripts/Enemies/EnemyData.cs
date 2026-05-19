using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public double baseHealth; // "Old Number"
    public double baseDamage;
    public double currencyReward;
    
    

}
