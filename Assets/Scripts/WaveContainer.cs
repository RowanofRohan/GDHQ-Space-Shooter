using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveContainer : MonoBehaviour
{
    [SerializeField]
    private GameObject[] enemyPrefabs;
    [SerializeField]
    private float[] firstSpawnDelays;
    [SerializeField]
    private Vector3[] firstSpawnLocations;
    [SerializeField]
    private float selfDestructTimer = 99.0f;
    
    private SpawnManager spawnManager;
    private GameObject enemyContainer;
    
    void Start()
    {
        spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        if (spawnManager == null)
        {
            Debug.LogError("Spawn Manager is NULL");
        }
        enemyContainer = GameObject.Find("Enemy Container");
        if (enemyContainer == null)
        {
            Debug.LogError("Could not find Enemy Container!");
        }
        transform.position = new Vector3(0,0,0);

        float currentDelay = 0.0f;
        for(int i = 0; i < enemyPrefabs.Length;i++)
        {
            currentDelay += firstSpawnDelays[i];
            StartCoroutine(SpawnEnemy(enemyPrefabs[i],currentDelay,firstSpawnLocations[i]));
        }

        StartCoroutine(SelfDestruct());
    }

    private IEnumerator SpawnEnemy(GameObject enemies, float delay, Vector3 location)
    {
        yield return new WaitForSeconds(delay);
        if(spawnManager.CheckSpawning())
        {
            GameObject newEnemy = Instantiate(enemies,location,Quaternion.identity);
            newEnemy.transform.parent = enemyContainer.transform;
        }
    }

    private IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(selfDestructTimer);
        Destroy(this.gameObject);
    }
}
