using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBoss : MonoBehaviour
{
    //DEBUG Control
    [SerializeField]
    private bool nextPhaseDebug = false;
    [SerializeField]
    private bool prevPhaseDebug = false;

    //Movement Controls
    [SerializeField]
    private int movementID = 0;
    [SerializeField]
    private float currentSpeed = 0.0f;
    private float moveTimeTracker = 0.0f;
    private float accelerationFactor = 0.0f;
    private float currentMoveDuration = 0.0f;

    //Movement Patterning Support
    [SerializeField]
    private Vector3 spawnLocation;

    [SerializeField]
    private GameObject[] transitionContainers;
    private TransitionController data;
    [SerializeField]
    private int currentTransition = 0;
    [SerializeField]
    private int currentStep = 0;

    //Internal Variables
    private Vector3 startPosition;
    private Vector3 destination;
    // private bool isDying = false;
    private bool isChangingPhases = true;

    //Health
    [SerializeField]
    private float maxHealth = 100.0f;
    [SerializeField]
    private float currentHealth;

    //Object/Audio Handles
    private SpawnManager spawnManager;
    private Player player;
    [SerializeField]
    private UIManager uiManager;
    [SerializeField]
    private GameObject projectileContainer;

    [SerializeField]
    private GameObject enemyWave;
    [SerializeField]
    private GameObject laserPrefab;
    [SerializeField]
    private GameObject giantLaserPrefab;
    [SerializeField]
    private GameObject minePrefab;
    [SerializeField]
    private GameObject bossMissilePrefab;
    [SerializeField]
    private GameObject explosionPrefab;

    private AudioSource audioSource;
    [SerializeField]
    private AudioClip explosionSound;
    [SerializeField]
    private AudioClip rumblingSound;
    [SerializeField]
    private AudioClip laserSound;
    [SerializeField]
    private AudioClip missileSound;

    //Component Handles
    [SerializeField]
    private BossComponent[] phase1Components;
    [SerializeField]
    private BossComponent[] phase2Components;
    [SerializeField]
    private BossComponent[] phase3Components;

    // [SerializeField]
    // private int killScore = 2000;

    //ATTACK AI/PATTERNING GOES HERE


    

    // Start is called before the first frame update
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
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("Boss Audio source is NULL");
        }

        destination = transform.position;
        
        InitializeMovement();
        InitializeHealth();
    }

    // Update is called once per frame
    void Update()
    {
        if(isChangingPhases == true)
        {
            MovementController();
        }
        DebugController();
        //AI CODE GOES HERE
    }

    private void DebugController()
    {
        //when debug boxes are ticked, triggers attacks and movements, then unticks them.
        if(nextPhaseDebug == true)
        {
            nextPhaseDebug = false;
            if (isChangingPhases == true)
            {
                currentTransition += 1;
            }
            EndPhase();
            InitializeMovement();
        }
        if(prevPhaseDebug == true)
        {
            prevPhaseDebug = false;
            if(isChangingPhases == false)
            {
                currentTransition -= 1;
            }
            EndPhase();
        }
    }

    private void InitializeMovement()
    {
        data = transitionContainers[currentTransition].GetComponent<TransitionController>();
        currentStep = 0;
        isChangingPhases = true;
        NextMoveStep();
    }

    private void InitializeHealth()
    {
        maxHealth = 0.0f;
        for(int i = 0; i < phase1Components.Length; i++)
        {
            maxHealth += phase1Components[i].GetMaxHealth();
        }
        for(int i = 0; i < phase2Components.Length; i++)
        {
            maxHealth += phase2Components[i].GetMaxHealth();
        }
        for(int i = 0; i < phase3Components.Length; i++)
        {
            maxHealth += phase3Components[i].GetMaxHealth();
        }
        currentHealth = maxHealth;
        SendHealthUpdate();
    }

    private void NextMoveStep()
    {
        if(currentStep < data.GetLength())
        {
            movementID = data.GetMovementID(currentStep);
            moveTimeTracker = 0.0f;
            startPosition = transform.position;
            destination = data.GetDestination(currentStep);
            currentMoveDuration = data.GetMoveduration(currentStep);
            accelerationFactor = data.GetAcceleration(currentStep);
            currentStep += 1;
        }
        else
        {
            StartPhase();
        }
    }

    private void StartPhase()
    {
        currentTransition += 1;
        isChangingPhases = false;
        switch(currentTransition)
        {
            case 0:
                //Should never be possible
                break;
            case 1: 
                for(int i = 0; i < phase1Components.Length; i++)
                {
                    if(phase1Components[i].GetDestroyable() == true)
                    {
                        phase1Components[i].SetInvuln(false);
                    }
                }
                //START ATTACK PATTERNS
                break;
            case 2: 
                for(int i = 0; i < phase2Components.Length; i++)
                {
                    if(phase2Components[i].GetDestroyable() == true)
                    {
                        phase2Components[i].SetInvuln(false);
                    }
                }
                //START ATTACK PATTERNS
                break;
            case 3: 
                for(int i = 0; i < phase3Components.Length; i++)
                {
                    if(phase3Components[i].GetDestroyable() == true)
                    {
                        phase3Components[i].SetInvuln(false);
                    }
                }
                //START ATTACK PATTERNS
                break;
            default:
                break;
        }
    }

    private void EndPhase()
    {
        foreach (Transform child in projectileContainer.transform)
        {
            Destroy(child.gameObject);
        }

        InitializeMovement();
        //Projectile Cleanup
        //Cleanup Coroutines for Attack Patterns
    }

    private void MovementController()
    {
        switch(movementID)
        {
            case 0:
                //Halts
                currentSpeed = 0;
                HaltController();
                break;
            case 1:
                //Regular Movement
                DefaultMovement();
                break;
            case 2:
                //Accelerating Movement
                AcceleratingMovement();
                break;
            default:
                Debug.Log("Bad Movement ID!");
                break;
        }
    }

    private void HaltController()
    {
        moveTimeTracker += Time.deltaTime;
        transform.position = destination;
        if(moveTimeTracker >= currentMoveDuration)
        {
            NextMoveStep();
        }
    }

    private void DefaultMovement()
    {
        //ID: 1
        moveTimeTracker += Time.deltaTime;
        float totalDistance = Vector3.Distance(startPosition, destination);
        if(moveTimeTracker <= currentMoveDuration)
        {
            float ratio = moveTimeTracker/currentMoveDuration;
            Vector3 direction = destination - startPosition;
            currentSpeed = (totalDistance/currentMoveDuration) * direction.normalized.y ;
            Vector3 bossMovement = new Vector3(0,currentSpeed,0);
            transform.Translate(bossMovement * Time.deltaTime);
        }
        if(moveTimeTracker > currentMoveDuration || totalDistance <= Vector3.Distance(startPosition, transform.position))
        {
            transform.position = destination;
            NextMoveStep();
        }
    }    

    private void AcceleratingMovement()
    {
        //ID: 2
        float totalDistance = Vector3.Distance(startPosition, destination);
        moveTimeTracker += Time.deltaTime;
        if(moveTimeTracker <= currentMoveDuration)
        {
            Vector3 bossMovement = new Vector3(0,currentSpeed,0);
            transform.Translate(bossMovement*Time.deltaTime);
            currentSpeed += accelerationFactor * Time.deltaTime;
        }
        if(moveTimeTracker > currentMoveDuration || totalDistance <= Vector3.Distance(startPosition, transform.position))
        {
            transform.position = destination;
            NextMoveStep();
        }
    }

    private void SendHealthUpdate()
    {
        //Sends health to UI manager on every frame
    }

    public void TakeDamage(float damage)
    {
        //Called whenever a component takes damage; updates current HP
        //Also checks how many components are remaining and triggers the next phase change if necessary
        //Can De-shield radar scanner once X components are destroyed
        
        SendHealthUpdate();
    }

    public void CheckPhaseChange()
    {
        bool readytoChange = true;
        switch(currentTransition)
        {
            case 0:
                //Intro phase
                break;
            case 1:
                for(int i = 0; i < phase1Components.Length; i++)
                {
                    if(phase1Components[i].GetState() != true && phase1Components[i].GetDestroyable() == true)
                    {
                        i = phase1Components.Length;
                        readytoChange = false;
                    }
                }
                break;
            case 2:
                for(int i = 0; i < phase2Components.Length; i++)
                {
                    if(phase2Components[i].GetState() != true && phase2Components[i].GetDestroyable() == true)
                    {
                        i = phase2Components.Length;
                        readytoChange = false;
                    }
                }
                break;
            case 3:
                for(int i = 0; i < phase3Components.Length; i++)
                {
                    if(phase3Components[i].GetState() != true && phase3Components[i].GetDestroyable() == true)
                    {
                        i = phase3Components.Length;
                        readytoChange = false;
                    }
                }
                break;
            default:
                Debug.Log("How did you even get here?");
                break;
        }
        if(readytoChange == true)
        {
            EndPhase();
        }
    }

    // public Player GetPlayer()
    // {
    //     return player;
    // }

    //UNIVERSAL ATTACK CONTROLLERS
    
    private void SummonBackup()
    {
        //Summons a short wave of enemies
        //Used in all three phases
    }

    private void MegaLaserBarrage()
    {
        //fires giant laser barrages converging towards center
    }

    private void WaveLasers()
    {
        //Short attack firing waves of lasers from small turrets
        //Used in stages 1 and 2; direction reversed in stage 2
    }


    //Rework tracking: 
    //Whenever needing to track player, handle it on boss and just point at player.
    //Use global cooldowns for certain tracking (e.g. multiple missile turrets)
    private void StartTracking()
    {
        //tells a turret to start tracking player, with a specific fire mode
    }

    private void StopTracking()
    {
        //Stops a turret from tracking to start a new attack
    }

    //STAGE 1 ATTACK CONTROLLERS

    private void MissileBarrage1()
    {
        //Fires missiles sequentially outwards from launchers, then converging from sides
    }

    //Additional stage 1 attacks:
    //Summon Backup
    //Medium turret tracks and fires dumb missiles
    //Mega Laser Barrage

    //STAGE 2 ATTACK CONTROLLERS

    private void LaserDiamond()
    {
        //Giant turrets fire large laser diamond around player
        //Followed up by converging laser rings OR enemy summon OR Wave Lasers OR multiple of these

    }

    //Additional stage 2 attacks:
    //Medium turrets track and dumbfire missiles
    //Large turrets will track player and fire giant lasers/laser bursts
    //giant lasers can be all at once, sequential, and sometimes randomly during other attacks

    //STAGE 3 ATTACK CONTROLLERS

    private void SideLaserBarrage()
    {
        //fires walls of lasers from either side
    }

    private void LaserSpiral()
    {
        //small turrets start sending out spirals of lasers
    }

    private void MissileSweep()
    {
        //Fires a sweep of accelerating missiles from missile bay
    }

    //Additionals:
    //Mines will drop periodically but consistently
    //Mega Laser Barrage
    //Medium turrets will constantly fire missiles
    //Large turrets can track and fire giant lasers/laser blasts
    //Small turrets can track and fire lasers

    //Initialization function used by Components
    public GameObject GetPrefab(int prefabID)
    {
        switch(prefabID)
        {
            case 0:
                return explosionPrefab;
            case 1:
                return laserPrefab;
            case 2:
                return bossMissilePrefab;
            case 3:
                return minePrefab;
            case 4: 
                return giantLaserPrefab;
            default:
                return null;
        }
    }
}
