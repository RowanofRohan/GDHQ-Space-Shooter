using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //Linear Movement
    [SerializeField]
    private int movementID = 0;
    [SerializeField]
    private float verticalSpeed = 4.0f;
    [SerializeField]
    private float horizontalSpeed = 0.0f;
    [SerializeField]
    private float accelerationFactor = 0.5f;
    [SerializeField]
    private int respawnType = 0;

    //Specialized Movement
    [SerializeField]
    private float initialAngle = 0.0f;
    [SerializeField]
    private float currentDirection = 0.0f;
    [SerializeField]
    private float movementDelay = 0.0f;
    private bool timerActive = false;
    [SerializeField]
    private float moveTimeTracker = 0.0f;
    private bool isDirectionReversed = false;
    private int currentCycle = 0;
    [SerializeField]
    private int maxCycles = 0;

    //Wave System Support and Movement Pattern Support
    [SerializeField]
    private Vector3 spawnLocation;
    [SerializeField]
    private int[] movementIDList;
    [SerializeField]
    private float[] initialDelays;
    [SerializeField]
    private float[] vertSpeedList;
    [SerializeField]
    private float[] horzSpeedList;
    [SerializeField]
    private float[] moveAngleList;
    [SerializeField]
    private float[] moveDelayList;
    [SerializeField]
    private float[] accelerationList;
    [SerializeField]
    private bool[] reverseDirectionList;
    [SerializeField]
    private Vector3[] teleportLocations;
    [SerializeField]
    private int[] respawnTypeList;

    //Internal Variables
    private Vector3 destination;
    Quaternion rotation;
    private bool isDying = false;

    //Aggression Support
    [SerializeField]
    private bool rammingSpeed = false;
    [SerializeField]
    private bool isAggro = false;
    [SerializeField]
    private float aggroVelocity = 6.0f;
    [SerializeField]
    private Vector3 targetDirection;

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

    //Object Handles
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
    private float altFire = -1.0f;
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
        transform.position = spawnLocation;
        destination = transform.position;

        InitializeMovement();
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

    private void InitializeMovement()
    {
        float currentDelay = 0.0f;
        for(int i = 0; i< movementIDList.Length; i++)
        {
            currentDelay += initialDelays[i];
            if(movementIDList[i] != 5)
            {
                StartCoroutine(MovementChanger(movementIDList[i], currentDelay, vertSpeedList[i], horzSpeedList[i], moveAngleList[i], moveDelayList[i], accelerationList[i], reverseDirectionList[i], respawnTypeList[i]));
            }
            else
            {
                StartCoroutine(TeleportController(currentDelay, teleportLocations[i],respawnTypeList[i]));
            }
        }
    }

    private IEnumerator MovementChanger(int newID, float startDelay, float newVertSpeed, float newHorzSpeed, float newAngle, float newBurstDelay, float newAcceleration, bool directionCheck, int newRespawn)
    {
        int cycleCheck = currentCycle;
        yield return new WaitForSeconds(startDelay);
        if(cycleCheck == currentCycle)
        {
            movementID = newID;
            verticalSpeed = newVertSpeed;
            initialAngle = newAngle;
            movementDelay = newBurstDelay;
            accelerationFactor = newAcceleration;
            horizontalSpeed = newHorzSpeed;
            isDirectionReversed = directionCheck;
            moveTimeTracker = 0.0f;
            respawnType = newRespawn;
        }
    }

    private IEnumerator TeleportController(float startDelay, Vector3 newLocation, int newRespawn)
    {
        int cycleCheck = currentCycle;
        yield return new WaitForSeconds(startDelay);
        if(cycleCheck == currentCycle)
        {
            respawnType = newRespawn;
            spawnLocation = newLocation;
            TeleportMovement();
        }
    }

    private void MovementController()
    {
        if(rammingSpeed == false || isAggro == false)
        {
            switch(movementID)
            {
                case 0:
                    //Halts
                    break;
                case 1:
                    //Default Movement
                    DefaultMovement();
                    break;
                case 2:
                    //Burst Movement
                    BurstMovement();
                    break;
                case 3:
                    //Serpentine
                    SerpentineMovement();
                    break;
                case 4:
                    //Circular
                    CircularMovement();
                    break;
                case 5:
                    //Teleport; This is handled primarily by the pattern controller and should never be called here. 
                    TeleportMovement();
                    break;
                default:
                    Debug.Log("Bad Movement ID!");
                    break;
            }
        }
        else
        {
            RammingAttack();
        }
    }

    private void ScreenBoundCheck()
    {        
        if (transform.position.y <= lowerScreenBound)
        {
            Vector3 distancetoGo;
            switch(respawnType)
            {
                case 0:
                    //Do nothing; relies on teleports. Will be the most common type for patterned enemies.
                    break;
                case 1:
                    //Respawns at top of screen in same X.
                    distancetoGo = destination - gameObject.transform.position;
                    transform.position = new Vector3(transform.position.x,upperScreenBound - 0.0001f,0);
                    destination = gameObject.transform.position + distancetoGo;
                    break;
                case 2:
                    //Respawns at top of screen in random X.
                    distancetoGo = destination - gameObject.transform.position;
                    float randomX = Random.Range(leftScreenBound,rightScreenBound);
                    transform.position = new Vector3(randomX,upperScreenBound - 0.0001f,0);
                    destination = gameObject.transform.position + distancetoGo;
                    break;
                case 3:
                    //Respawns at top of screen, and re-initializes movement. Should be used in place of 1 or 2 for all patterned enemies.
                    currentCycle += 1;
                    if(currentCycle >= maxCycles)
                    {
                        Despawn();
                    }
                    else
                    {
                        transform.position = new Vector3(transform.position.x,upperScreenBound - 0.0001f,0);
                        InitializeMovement();
                    }
                    break;
                case 4:
                    //Self-Destructs once off screen
                    Despawn();
                    break;
            }
        }
    }

    private void DefaultMovement()
    {
        //ID: 1
        //Recommended settings: Vertical speed 3
        Vector3 enemyMovement = new Vector3(0,verticalSpeed*-1,0);
        rotation = Quaternion.Euler(0,0,initialAngle);
        transform.Translate(rotation * enemyMovement * Time.deltaTime);
        destination = gameObject.transform.position;

        ScreenBoundCheck();
    }

    private void BurstMovement()
    {
        //ID: 2
        //Recommended settings: vertical speed 3, movement delay 2, acceleration 0.05
        if(timerActive != true)
        {
            StartCoroutine(BurstMoveTimer());
        }
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position,destination,accelerationFactor);
        ScreenBoundCheck();
    }

    private IEnumerator BurstMoveTimer()
    {
        timerActive = true;
        while(movementID == 2)
        {
            Vector3 enemyMovement = new Vector3(0,verticalSpeed*-1,0);
            rotation = Quaternion.Euler(0,0,initialAngle);
            destination = gameObject.transform.position + enemyMovement;
            yield return new WaitForSeconds(movementDelay);
        }
        timerActive = false;
    }

    private void SerpentineMovement()
    {
        //ID: 3
        //Recommended Settings: Vertical Speed 2, Horizontal Speed 5, Acceleration 0.45
        //Reversing direction alters initial direction
        currentDirection = Mathf.Lerp(horizontalSpeed,horizontalSpeed*-1,Mathf.PingPong(moveTimeTracker*accelerationFactor, 1));
        moveTimeTracker += Time.deltaTime;
        Vector3 enemyMovement = new Vector3(currentDirection,verticalSpeed*-1,0);
        if(isDirectionReversed)
        {
            enemyMovement.x = enemyMovement.x * -1;
        }
        transform.Translate(enemyMovement*Time.deltaTime);
        ScreenBoundCheck();

    }

    private void CircularMovement()
    {
        //ID: 4
        //Recommended Settings: Acceleration 0.15-0.2, Vertical Speed 3
        currentDirection = Mathf.Lerp(0.0f,360.0f,Mathf.Repeat(moveTimeTracker*accelerationFactor, 1)) - initialAngle;
        moveTimeTracker += Time.deltaTime;
        if(currentDirection < 0.0f)
        {
            currentDirection += 360.0f;
        }
        else if (currentDirection > 360.0f)
        {
            currentDirection -= 360.0f;
        }
        rotation = Quaternion.Euler(0,0,currentDirection);
        Vector3 enemyMovement = rotation * new Vector3(0,verticalSpeed*-1,0);
        if(isDirectionReversed)
        {
            enemyMovement.x = enemyMovement.x * -1;
        }
        transform.Translate(enemyMovement * Time.deltaTime);
        destination = gameObject.transform.position;

        ScreenBoundCheck();
    }

    private void TeleportMovement()
    {
        transform.position = spawnLocation;
        movementID = 0;
    }

    private void RammingAttack()
    {
        targetDirection = player.transform.position - gameObject.transform.position;
        transform.Translate (targetDirection.normalized * aggroVelocity *Time.deltaTime);
    }

    private void FireController()
    {
        if (Time.time > canFire)
        {
            laserCooldown = Random.Range(laserMinCD,laserMaxCD);
            canFire = Time.time + laserCooldown;
            FireLaser();
        }
        else
        {
            RaycastHit2D detect = Physics2D.Raycast(transform.position - new Vector3(0, 2, 0), new Vector3(0,-1,0));
            if(detect.collider != null)
            {
                if (detect.collider.transform.tag == "Powerup" && Time.time > altFire && detect.collider.transform.GetComponent<Powerup>().hazardCheck() == false)
                {
                    laserCooldown = Random.Range(laserMinCD,laserMaxCD);
                    altFire = Time.time + laserCooldown;
                    FireLaser();
                }
            }
        }
    }

    private void FireLaser()
    {
        GameObject newLaser = Instantiate(laserPrefab, transform.position + new Vector3 (0,-0.7f,0), Quaternion.identity);
        newLaser.GetComponent<Laser>().SetHostile(true);
        audioSource.clip = laserShot;
        audioSource.Play();
    }

    public void TakeDamage(float damageTaken)
    {
        currentHealth -= damageTaken;
        if (currentHealth <= 0.0001f)
        {
            KillTrigger();
            DeathTrigger();
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
                    float damageTemp = laser.CallDamage();
                    Destroy(other.gameObject);
                    TakeDamage(damageTemp);
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
        else if (other.transform.tag == "Aggro Radius" && isAggro == true)
        {
            rammingSpeed = true;
            //aggroDestination = destination;
            destination = other.transform.position;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if(other.transform.tag == "Laser")
        {
            GiantLaser giantLaser = other.transform.GetComponent<GiantLaser>();
            if (giantLaser != null)
            {
                if (giantLaser.CallAllegiance() == false)
                {
                    TakeDamage(giantLaser.CallDamage() * Time.deltaTime);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.transform.tag == "Aggro Radius")
        {
            rammingSpeed = false;
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
        isDying = true;
        enemyAnimator.SetTrigger("onEnemyDeath");
        audioSource.clip = explosionSound;
        audioSource.Play();
        enemyCollider.enabled = false;
        lowerScreenBound = -60000.0f;
        Destroy(this.gameObject, 2.65f);
    }

    private void Despawn()
    {
        isDying = true;
        movementID = 0;
        StopAllCoroutines();
        lowerScreenBound = -60000.0f;
        Destroy(this.gameObject,0.1f);
    }

    public bool DeathCheck()
    {
        return isDying;
    }
}
