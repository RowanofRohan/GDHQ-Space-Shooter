using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{

    [SerializeField]
    private float rotationSpeed = 3.0f;

    [SerializeField]
    private GameObject explosionPrefab;

    private SpawnManager spawnManager;
    private Collider2D asteroidCollider;
    private Player player;

    // Start is called before the first frame update
    void Start()
    {
        spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        if (spawnManager == null)
        {
            Debug.LogError("Spawn Manager is NULL");
        }
        asteroidCollider = gameObject.GetComponent<Collider2D>();
        if (asteroidCollider == null)
        {
            Debug.LogError("Cannot find Asteroid Collder!");
        }
        player = GameObject.Find("Player").GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("Cannot find player!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0,0,rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.transform.tag == "Laser")
        {
            Laser laser = other.transform.GetComponent<Laser>();
            if(laser != null)
            {
                Destroy(other.gameObject);
                DestroyTrigger();
            }
        }
        else if (other.transform.tag == "Player")
        {
            if (player != null)
            {
                player.Damage();
            }
            DestroyTrigger();
        }
    }

    private void DestroyTrigger()
    {
        GameObject explosion = Instantiate(explosionPrefab,transform.position, Quaternion.identity);
        asteroidCollider.enabled = false;
        spawnManager.StartSpawning();
        Destroy(explosion.gameObject, 1.5f);
        Destroy(this.gameObject,0.5f);
    }

}


