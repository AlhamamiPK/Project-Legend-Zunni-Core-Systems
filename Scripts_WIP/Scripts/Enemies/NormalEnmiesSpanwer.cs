using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
public class NormalEnmiesSpanwer : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] public GameObject[] enemies;
    [SerializeField] public int preLoadedEnemies = 5; // This is how many total enemies we want to pre-load into the pool at the start
    // Keep in mind it"s differnt For each stage it increasesssss
    //This holds all the inactive enemies we create
    [SerializeField] private List<GameObject> enemyPool = new List<GameObject>();

    [SerializeField] public GameObject[] bosses;
    [Header("Spawn Settings")]
    [SerializeField] private Transform playersTransform;
    [SerializeField] private Vector3 SpawnOffSet;

    [Header("State Checks")]
    [SerializeField]public static bool bossIsSpawned = false;
    
    
    [Header("Loot Hero Spawning Settings")]
    [SerializeField] private float spawnDistance = 10f; // How far the player must move to trigger a new batch
    [SerializeField] private int minBatchSize = 4;      // Minimum enemies in a line
    [SerializeField] private int maxBatchSize = 8;      // Maximum enemies in a line
    [SerializeField] private float distanceBetweenEnemies = 1.5f; // Space between enemies in the same batch
    [Header("Spawn Prevention Checks")]
    [SerializeField] private LayerMask layersToAvoid; // Set this to INCLUDE both "Shop" and "Enemy" layers!
    [SerializeField] private float enemySpawnRadius = 1f; // Make this roughly the width of your enemy
    [Header("Tracking (Do Not Edit)")]
    // NEW: This keeps track of how many enemies are left to kill in the current batch
    public int enemiesAliveInBatch = 0;
    // This remembers where the player was the last time we spawned a batch
    private float lastSpawnXPos;
   



    private void Start()
    {
        if (playersTransform != null)
        {
            //  lastSpawnXPos = playersTransform.position.x;
            SpawnPlayer();
            SpawnBatch();
        }
        else
        {
            SpawnPlayer();
            SpawnBatch();
        }
        //for (int i = 0; i < preLoadedEnemies; i++)
        //{
        //    int random = Random.Range(0,enemies.Length);
        //    GameObject newEnemyClone = Instantiate(enemies[random], transform);
        //    // FIX: Initialize the enemy so it actually has health! 
        //    // (I put '5' here for stage 5, you can change this later to your stage variable)
        //    Enemy enemyScript = newEnemyClone.GetComponent<Enemy>();
        //    if (enemyScript != null)
        //    {
        //        enemyScript.Initialize(5);
        //    }
        //    newEnemyClone.SetActive(false);
        //    enemyPool.Add(newEnemyClone);
        //}
    }
    private void Awake()
    {
        if(playersTransform == null)
         {
            SpawnPlayer();
        
        }
        
        
       // if(NormalEnmiesSpanwer.)
    }
    private void Update()
    {
        
    }
    private void SpawnPlayer()
    {
        if (playersTransform != null)
        {
            playersTransform = PlayerStats.Instance.transform;
        }
        else
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            playersTransform = playerObj.transform;
        }
    }
    //private GameObject GetPooledEnemy()
    //{
    //    for (int i = 0;i < enemyPool.Count; i++)
    //    {
    //        if (!enemyPool[i].activeInHierarchy)
    //        {
    //            return enemyPool[i];
    //        }
    //    } return null;
    //}

    private void SpawnBatch()
    {
       // GameManager.instance.SpawnShop();
        if (bossIsSpawned == true) return;
        int randomNumber = Random.Range(minBatchSize, maxBatchSize + 1);
        enemiesAliveInBatch = randomNumber;

        float currentXOffset = SpawnOffSet.x;

        for (int i = 0; i < randomNumber; i++)
        {
            int randomIndex = Random.Range(0, enemies.Length);
            GameObject enemyPrefabToSpawn = enemies[randomIndex];

            // 1. Calculate our initial guess for where to spawn
            float spawnY = playersTransform.position.y + SpawnOffSet.y;
            Vector3 spawnPosition = new Vector3(playersTransform.position.x + currentXOffset, spawnY, 0f);

            // 2. THE FIX: Check if this specific spot is touching a Shop or another Enemy
            int safetyCounter = 0; // This prevents the game from freezing if it gets stuck

            // As long as the spot is blocked, keep pushing the spawn position further to the right!
            while (Physics2D.OverlapCircle(spawnPosition, enemySpawnRadius, layersToAvoid) && safetyCounter < 50)
            {
                currentXOffset += distanceBetweenEnemies; // Bump it to the right
                spawnPosition = new Vector3(playersTransform.position.x + currentXOffset, spawnY, 0f);
                safetyCounter++;
            }

            // 3. We found a clear, empty spot! Now we can safely spawn the enemy.
            Quaternion spawnRotation = Quaternion.Euler(0, 180, 0);
            GameObject newEnemyClone = Instantiate(enemyPrefabToSpawn, spawnPosition, spawnRotation, transform);
            //newEnemyClone.SetActive(true);
            Enemy enemyScript = newEnemyClone.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.Initialize(GameManager.instance.currentStageIndex); // (Change '5' to your stage variable later)
            }

            // 4. Add the normal distance gap so the next enemy in the loop doesn't spawn inside this one
            currentXOffset += distanceBetweenEnemies;
        }


        //for (int i = 0; i < randomNumber; i++)
        //{
        //    // 1. Pick a random enemy prefab from your array
        //    int randomIndex = Random.Range(0, enemies.Length);
        //    GameObject enemyPrefabToSpawn = enemies[randomIndex];

        //    // 2. Calculate the spawn position.
        //    float spawnX = playersTransform.position.x + SpawnOffSet.x + (i * distanceBetweenEnemies);
        //    float spawnY = playersTransform.position.y + SpawnOffSet.y;
        //    Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
        //    Quaternion spawnRotation = Quaternion.Euler(0, 180, 0);

        //    // 3. INSTANTIATE the enemy directly into the world!
        //    // This creates a brand new clone from the prefab.
        //    GameObject newEnemyClone = Instantiate(enemyPrefabToSpawn, spawnPosition, spawnRotation, transform);

        //    // 4. Initialize the enemy so it actually has health
        //    Enemy enemyScript = newEnemyClone.GetComponent<Enemy>();
        //    if (enemyScript != null)
        //    {
        //        enemyScript.Initialize(5); // (Change '5' to your stage variable later)
        //    }

        //}

    }
    public void EnemyDefeated()
    {
        // Minus 1 from the counter
        GameManager.instance.OnEnemyKilled();
        enemiesAliveInBatch = enemiesAliveInBatch - 1;

        // If the counter reaches 0 (or below, just in case), spawn the next batch!
        if (enemiesAliveInBatch <= 0)
        {
            enemiesAliveInBatch = 0; // Keep it clean at 0
            SpawnBatch();
        }
    }
}
