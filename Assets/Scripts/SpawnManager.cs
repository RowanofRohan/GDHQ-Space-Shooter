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
    private GameObject[] powerups;
    [SerializeField]
    private GameObject[] pickups;
    [SerializeField]
    private GameObject powerupContainer;

    [SerializeField]
    private float minimumPickupSpawnDelay = 7.0f;
    [SerializeField]
    private float maximumPickupSpawnDelay = 13.0f;
    [SerializeField]
    private float initialSpawnDelay = 3.0f;


    void Start()
    {
        canSpawn = false;
    }

    void Update()
    {
    }

    IEnumerator EnemySpawnRoutine()
    {
        yield return new WaitForSeconds(initialSpawnDelay);
        while(canSpawn)
        {
            float randomX = Random.Range(leftScreenBound,rightScreenBound);
            spawnLocation = new Vector3(randomX, upperScreenBound - 0.0001f, 0);
            GameObject newEnemy = Instantiate(enemyPrefab,spawnLocation,Quaternion.identity);
            newEnemy.transform.parent = enemyContainer.transform;
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    IEnumerator PickupSpawnRoutine()
    {
        while(canSpawn)
        {
            float randomDelay = Random.Range(minimumPickupSpawnDelay,maximumPickupSpawnDelay);
            yield return new WaitForSeconds(randomDelay);
         
            if(canSpawn)
            {
                float randomX = Random.Range(leftScreenBound,rightScreenBound);
                spawnLocation = new Vector3(randomX, upperScreenBound - 0.0001f, 0);
                int pickupRandomizer = Random.Range(0,pickups.Length);
                if(pickups[pickupRandomizer] != null)
                {
                    GameObject newPickup = Instantiate(pickups[pickupRandomizer],spawnLocation,Quaternion.identity);
                    newPickup.transform.parent = powerupContainer.transform;
                }
            }
        }
    }

    public void SpawnPowerup(Vector3 spawnLocation)
    {
        if(canSpawn)
        {
            int powerupRandomizer = Random.Range(0,powerups.Length);
            if(powerups[powerupRandomizer] != null)
            {
                GameObject newPowerup = Instantiate(powerups[powerupRandomizer],spawnLocation,Quaternion.identity);
                newPowerup.transform.parent = powerupContainer.transform;
            }
        }
        
    }

    public void StartSpawning()
    {
        canSpawn = true;
        StartCoroutine(PickupSpawnRoutine());
        StartCoroutine(EnemySpawnRoutine());
    }

    public void GameOver()
    {
        canSpawn = false;
    }
}
