using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float speed = 4.0f;
    [SerializeField]
    private float maxHealth = 5.0f;
    private float currentHealth;

    [SerializeField]
    private float dropRatePercentage = 20.0f;

    [SerializeField]
    private float upperScreenBound = 8.0f;
    [SerializeField]
    private float lowerScreenBound = -6.0f;
    [SerializeField]
    private float leftScreenBound = -9.2f;
    [SerializeField]
    private float rightScreenBound = 9.2f;

    
    private SpawnManager spawnManager;
    

    void Start()
    {
        currentHealth = maxHealth;
        spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        if (spawnManager == null)
        {
            Debug.LogError("Spawn Manager is NULL");
        }
    }

    void Update()
    {
        Vector3 enemyMovement = new Vector3(0,speed*-1,0);
        transform.Translate(enemyMovement*Time.deltaTime);

        if (transform.position.y <= lowerScreenBound)
        {
            float randomX = Random.Range(leftScreenBound,rightScreenBound);
            transform.position = new Vector3(randomX,upperScreenBound - 0.0001f,0);
        }
        //Destroys enemy if it somehow flies off the top of the screen; not in use
        //else if (transform.position.y >= upperScreenBound + 0.001f)
        //{
        //    Destroy(this.gameObject);
        //}
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.transform.tag == "Laser")
        {
            Laser laser = other.transform.GetComponent<Laser>();
            if(laser != null)
            {
                currentHealth -= laser.CallDamage();
                Destroy(other.gameObject);
                if (currentHealth <= 0.0001f)
                {
                    KillTrigger();
                    Destroy(this.gameObject);
                }
            }
        }
        else if (other.transform.tag == "Player")
        {
            Player player = other.transform.GetComponent<Player>();
            if (player != null)
            {
                player.Damage();
            }
            Destroy(this.gameObject);
        }
    }

    private void KillTrigger()
    {
        float dropCheck = Random.Range(0,100);
        if(dropCheck <= dropRatePercentage)
        {
            Vector3 deathLocation = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
            spawnManager.SpawnPowerup(deathLocation);
        }
    }
}
