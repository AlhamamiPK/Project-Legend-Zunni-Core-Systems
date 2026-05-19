using UnityEngine;

public class EnemyTriggerThatSpawnsEnemies : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] Transform spawnPos;

    [Header("Enemy SetUp")]
    [Tooltip("0 for the first enemy in GameManager's list,1for the second etc")]
    public int enemyIndexToSpawn = 0;
    
    public Vector3 posOfSpawn;
    public bool hasTriggered = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        posOfSpawn = spawnPos.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered)
        {
            return;
        }
        if (collision.CompareTag("Player"))
        {
            hasTriggered = true;
            gameManager.SpawnSpecificEnemy(enemyIndexToSpawn, posOfSpawn);
            Destroy(gameObject);
        }
    }
}
