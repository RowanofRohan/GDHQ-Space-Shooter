using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //Movement Controller
    [SerializeField]
    private float speed = 4.0f;
    [SerializeField]
    private int movementID = 0;
    [SerializeField]
    private bool randomRespawn = true;

    //Additional Move Parameters
    [SerializeField]
    private float accelerationFactor = 0.5f;
    
    private Vector3 destination;

    //Health
    [SerializeField]
    private float maxHealth = 5.0f;
    private float currentHealth;

    //Item Drops
    [SerializeField]
    private float dropRatePercentage = 20.0f;

    //Screen Bounds
    [SerializeField]
    private float upperScreenBound = 8.0f;
    [SerializeField]
    private float lowerScreenBound = -6.0f;
    [SerializeField]
    private float leftScreenBound = -9.2f;
    [SerializeField]
    private float rightScreenBound = 9.2f;

    //Ojbject Handles
    private Animator enemyAnimator;
    private Collider2D enemyCollider;
    private SpawnManager spawnManager;
    private Player player;

    [SerializeField]
    private int killScore = 100;

    //Audio handles
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip explosionSound;
    
    //Laser Components
    [SerializeField]
    private GameObject laserPrefab;
    [SerializeField]
    private float laserCooldown = 3.0f;

    //Fire Controller
    private float canFire = -1.0f;
    [SerializeField]
    private float laserMinCD = 3.0f;
    [SerializeField]
    private float laserMaxCD = 7.0f;
    [SerializeField]
    private AudioClip laserShot;

    void Start()
    {
        currentHealth = maxHealth;
        spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        if (spawnManager == null)
        {
            Debug.LogError("Spawn Manager is NULL");
        }
        player = GameObject.Find("Player").GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("Cannot find player!");
        }
        enemyAnimator = gameObject.GetComponent<Animator>();
         if (enemyAnimator == null)
        {
            Debug.LogError("Cannot find animator!");
        }
        enemyCollider = gameObject.GetComponent<Collider2D>();
         if (enemyCollider == null)
        {
            Debug.LogError("Cannot find Collider!");
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("Player audio source is NULL");
        }
        canFire = Time.time + laserCooldown;
        destination = transform.position;
    }

    void Update()
    {
        MovementController();
        FireController();

        //Destroys enemy if it somehow flies off the top of the screen; not in use
        //else if (transform.position.y >= upperScreenBound + 0.001f)
        //{
        //    Destroy(this.gameObject);
        //}
    }

    public void SetMovementID(int newMoveID)
    {
        movementID = newMoveID;
    }

    public void SetMovementParameters(bool respawnType = randomRespawn, float newSpeed = speed, float newAcceleration = accelerationFactor)
    {
        randomRespawn = respawnType;
        speed = newSpeed;
        accelerationFactor = newAcceleration;
    }

    private void MovementController()
    {
        switch(movementID)
        {
            case 0:
                //Default Movement
                DefaultMovement();
                break;
            case 1:
                //Burst Movement
                BurstMovement();
                break;
            case 2:
                //Serpentine
                SerpentinMovement();
                break;
            case 3:
                //Circular
                CircularMovement();
                break;
            case 4:
                //Reverse Movement
                ReverseMovement();
                break;
        }
    }

    private void DefaultMovement()
    {
        Vector3 enemyMovement = new Vector3(0,speed*-1,0);
        transform.Translate(enemyMovement*Time.deltaTime);

        if (transform.position.y <= lowerScreenBound)
        {
            if (randomRespawn = true)
            {
                float randomX = Random.Range(leftScreenBound,rightScreenBound);
                transform.position = new Vector3(randomX,upperScreenBound - 0.0001f,0);
            }
            else
            {
                transform.position = new Vector3(transform.position.x,upperScreenBound - 0.0001f,0);
            }
        }
    }

    private void BurstMovement()
    {
        StartCoroutine(BurstMoveTimer());
        //transform
    }

    private IEnumerator BurstMoveTimer()
    {
        //destination
        yield return null;
    }

    private void SerpentinMovement()
    {

    }

    private void CircularMovement()
    {

    }

    private void ReverseMovement()
    {

    }

    private void FireController()
    {
        if (Time.time > canFire)
        {
            laserCooldown = Random.Range(laserMinCD,laserMaxCD);
            canFire = Time.time + laserCooldown;
            GameObject newLaser = Instantiate(laserPrefab, transform.position + new Vector3 (0,-0.7f,0), Quaternion.identity);
            newLaser.GetComponent<Laser>().SetHostile(true);
            audioSource.clip = laserShot;
            audioSource.Play();
            Debug.Break();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.transform.tag == "Laser")
        {
            Laser laser = other.transform.GetComponent<Laser>();
            if(laser != null)
            {
                if (other.GetComponent<Laser>().CallAllegiance() == false)
                {
                    currentHealth -= laser.CallDamage();
                    Destroy(other.gameObject);
                    if (currentHealth <= 0.0001f)
                    {
                        KillTrigger();
                        DeathTrigger();
                    }
                }
            }
        }
        else if (other.transform.tag == "Player")
        {
            if (player != null)
            {
                player.Damage();
            }
            DeathTrigger();
        }
    }

    private void KillTrigger()
    {
        if (player != null)
        {
            player.ScoreUpdate(killScore);
        }
        float dropCheck = Random.Range(0,100);
        if(dropCheck <= dropRatePercentage)
        {
            Vector3 deathLocation = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
            spawnManager.SpawnPowerup(deathLocation);
        }
    }

    private void DeathTrigger()
    {
        enemyAnimator.SetTrigger("onEnemyDeath");
        audioSource.clip = explosionSound;
        audioSource.Play();
        enemyCollider.enabled = false;
        lowerScreenBound = -60000.0f;
        Destroy(this.gameObject, 2.65f);
    }
}
