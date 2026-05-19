using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] public Slider healthSlider;

    [Header("References")]
    [SerializeField] public Enemy enemyScript;

    [SerializeField] public float enemyHealth;
    private void Start()
    {
        // When the enemy spawns, whatever health it has is its maximum.
        // We set the slider's max limit to that starting health.
        healthSlider.maxValue = (float)enemyScript.currentHealth;
        enemyHealth = (float)enemyScript.currentHealth;
        healthSlider.value = (float)enemyScript.currentHealth;
    }

    public void EnemyIsDamged()
    {
        healthSlider.value = (float)enemyScript.currentHealth;
        IsEnemyDead();
    }
    void IsEnemyDead()
    {
        if(healthSlider.value <= 0)
        {
            Destroy(gameObject);
        }
    }
}