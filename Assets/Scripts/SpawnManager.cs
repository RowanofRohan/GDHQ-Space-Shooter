using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private bool canSpawn = true;
    [SerializeField]
    private float spawnDelay = 1.0f;
    
    [SerializeField]
    private float leftScreenBound = -9.2f;
    [SerializeField]
    private float rightScreenBound = 9.2f;
    [SerializeField]
    private float upperScreenBound = 8.0f;

    [SerializeField]
    private Vector3 spawnLocation = new Vector3(0,0,0);

    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private GameObject enemyContainer;
    [SerializeField]
    private GameObject powerupPrefab;
    [SerializeField]
    private GameObject powerupContainer;

    [SerializeField]
    private float minimumSpawnDelay = 7.0f;
    [SerializeField]
    private float maximumSpawnDelay = 13.0f;


    void Start()
    {
        StartCoroutine(EnemySpawnRoutine());
        StartCoroutine(PowerupSpawnRoutine());
    }

    void Update()
    {
    }

    IEnumerator EnemySpawnRoutine()
    {
        while(canSpawn)
        {
            float randomX = Random.Range(leftScreenBound,rightScreenBound);
            spawnLocation = new Vector3(randomX, upperScreenBound - 0.0001f, 0);
            GameObject newEnemy = Instantiate(enemyPrefab,spawnLocation,Quaternion.identity);
            newEnemy.transform.parent = enemyContainer.transform;
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    IEnumerator PowerupSpawnRoutine()
    {
        while(canSpawn)
        {
            float randomDelay = Random.Range(minimumSpawnDelay,maximumSpawnDelay);
            yield return new WaitForSeconds(randomDelay);
            
            float randomX = Random.Range(leftScreenBound,rightScreenBound);
            spawnLocation = new Vector3(randomX, upperScreenBound - 0.0001f, 0);
            GameObject newPowerup = Instantiate(powerupPrefab,spawnLocation,Quaternion.identity);
            newPowerup.transform.parent = powerupContainer.transform;
        }
    }

    public void GameOver()
    {
        canSpawn = false;
    }
}