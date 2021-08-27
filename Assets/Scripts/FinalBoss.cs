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
    [SerializeField]
    private bool changePhase = false;
    [SerializeField]
    private int whichPhase = -1;
    [SerializeField]
    private bool startAttack = false;
    [SerializeField]
    private int whichAttack = -1;
    [SerializeField]
    private int whichTurret = -1;

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
    [SerializeField]
    private GameObject tracerPrefab;

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
        InitializeComponents();
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
        if(changePhase == true)
        {
            changePhase = false;
            if(whichPhase != -1)
            {
                currentTransition = whichPhase - 1;
                EndPhase();
                currentStep = data.GetLength() - 1;
                NextMoveStep();
            }
        }
        if(startAttack == true)
        {
            startAttack = false;
            switch(whichAttack)
            {
                case 0:
                    StartTracking();
                    break;
                case 1:
                    StopTracking();
                    break;
                case 2:
                    StartCoroutine(SummonBackup());
                    break;
                case 3:
                    StartCoroutine(MegaLaserBarrage(reverseConvergence));
                    break;
                case 4:
                    WaveLasers();
                    break;
                case 5:
                    StartCoroutine(MissileBarrage());
                    break;
                case 6:
                    LaserDiamond();
                    break;
                case 7:
                    SideLaserBarrage();
                    break;
                case 8:
                    MissileSweep();
                    break;
                case 9:
                    MegaLaserSweep();
                    break;
                default:
                    break;
            }
        }
    }

    private void InitializeMovement()
    {
        data = transitionContainers[currentTransition].GetComponent<TransitionController>();
        currentStep = 0;
        isChangingPhases = true;
        NextMoveStep();
    }

    private void InitializeComponents()
    {
        maxHealth = 0.0f;
        for(int i = 0; i < phase1Components.Length; i++)
        {
            maxHealth += phase1Components[i].GetMaxHealth();
            phase1Components[i].SetPlayer(player);
        }
        for(int i = 0; i < phase2Components.Length; i++)
        {
            maxHealth += phase2Components[i].GetMaxHealth();
            phase2Components[i].SetPlayer(player);
        }
        for(int i = 0; i < phase3Components.Length; i++)
        {
            maxHealth += phase3Components[i].GetMaxHealth();
            phase3Components[i].SetPlayer(player);
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

    //UNIVERSAL ATTACK CONTROLLERS

    [SerializeField]
    private float summonDelay = 2.0f;
    [SerializeField]
    private float summonVulnerableWindow = 2.0f;
    [SerializeField]
    private Vector3 waveSpawn = new Vector3(0,8,0);


    private IEnumerator SummonBackup()
    {
        BossComponent component = null;
        switch(currentTransition)
        {
            case 1:
                component = phase1Components[4].GetComponent<BossComponent>();
                break;
            case 2:
                component = phase2Components[8].GetComponent<BossComponent>();
                break;
            case 3:
                component = phase3Components[10].GetComponent<BossComponent>();
                break;
            default:
                break;
        }
        if(component != null)
        {
            if(component.GetState() == false)
            {
                component.ShieldController(false);
                yield return new WaitForSeconds(summonDelay);
                if(component.GetState() == false)
                {
                    Instantiate(enemyWave, waveSpawn, Quaternion.identity);
                }
                yield return new WaitForSeconds(summonVulnerableWindow);
                if(component.GetState() == false)
                {
                    component.ShieldController(true);
                }
            }
        }
    }

    //Laser Positions are static; other variables account for phase 1 vs phase 3
    [SerializeField]
    private Vector3[] laserPositions;
    [SerializeField]
    private float[] tracerDuration;
    [SerializeField]
    private float[] gigaLaserDuration;
    [SerializeField]
    private float[] gigaLaserDelays;
    [SerializeField]
    private bool reverseConvergence = false;

    private IEnumerator MegaLaserBarrage(bool laserDirection)
    {
        int currentPhase = currentTransition;
        int i = 0;
        switch(currentTransition)
        {
            case 1:
                i = 0;
                break;
            case 3:
                i = 1;
                break;
            default:
                i = 0;
                break;
        }
        if(laserDirection == false)
        {
            StartCoroutine(GigaLaserController(i, laserPositions[0]));
            StartCoroutine(GigaLaserController(i, laserPositions[1]));
            yield return new WaitForSeconds(gigaLaserDelays[i]);
            if(currentPhase == currentTransition)
            {
                StartCoroutine(GigaLaserController(i, laserPositions[2]));
                StartCoroutine(GigaLaserController(i, laserPositions[3]));
            }
            yield return new WaitForSeconds(gigaLaserDelays[i]);
            if(currentPhase == currentTransition)
            {
                StartCoroutine(GigaLaserController(i, laserPositions[4]));
            }
        }
        else
        {
            StartCoroutine(GigaLaserController(i, laserPositions[4]));
            yield return new WaitForSeconds(gigaLaserDelays[i]);
            if(currentPhase == currentTransition)
            {
                StartCoroutine(GigaLaserController(i, laserPositions[3]));
                StartCoroutine(GigaLaserController(i, laserPositions[2]));
            }
            yield return new WaitForSeconds(gigaLaserDelays[i]);
            if(currentPhase == currentTransition)
            {
                StartCoroutine(GigaLaserController(i, laserPositions[1]));
                StartCoroutine(GigaLaserController(i, laserPositions[0]));
            }
        }  
    }

    private IEnumerator GigaLaserController(int j, Vector3 laserSpawn)
    {
        GameObject tracer = Instantiate(tracerPrefab, laserSpawn, Quaternion.identity);
        yield return new WaitForSeconds(tracerDuration[j]);
        if(tracer != null)
        {
            Destroy(tracer.gameObject);
            GameObject gigaLaser = Instantiate(giantLaserPrefab, laserSpawn, Quaternion.identity);
            StartCoroutine(TracerPulse(1, gigaLaserDuration[j]*1.5f,gigaLaser.GetComponent<Laser>()));
            yield return new WaitForSeconds(gigaLaserDuration[j]);
            if(gigaLaser != null)
            {
                Destroy(gigaLaser.gameObject);
            }
        }
    }

    [SerializeField]
    private int[] waveShots;
    [SerializeField]
    private float[] waveAngle;
    [SerializeField]
    private int[] waveSweeps;
    [SerializeField]
    private float[] waveShotDelay;
    [SerializeField]
    private float[] waveStartAngle;
    [SerializeField]
    private float[] waveInitialDelay;
    [SerializeField]
    private float[] waveInitialSpeed;
    [SerializeField]
    private float[] waveShotSpeed;
    [SerializeField]
    private bool alternateTurrets = false;

    private void WaveLasers()
    {
        int i = 0;
        BossComponent leftTurret = null;
        BossComponent rightTurret = null;
        switch(currentTransition)
        {
            case 1:
                i = 0;
                leftTurret = phase1Components[0].GetComponent<BossComponent>();
                rightTurret = phase1Components[1].GetComponent<BossComponent>();
                break;
            case 2:
                i = 1;
                leftTurret = phase2Components[7].GetComponent<BossComponent>();
                rightTurret = phase2Components[6].GetComponent<BossComponent>();
                break;
            case 3:
                i = 2;
                if(alternateTurrets == false)
                { 
                    leftTurret = phase3Components[9].GetComponent<BossComponent>();
                    rightTurret = phase3Components[7].GetComponent<BossComponent>();
                }
                else
                {
                    leftTurret = phase3Components[8].GetComponent<BossComponent>();
                    rightTurret = phase3Components[6].GetComponent<BossComponent>();
                }
                break;
            default:
                i = 0;
                break;
        }
        if(leftTurret != null && leftTurret.GetState() == false)
        {
            StartCoroutine(WaveLaserController(leftTurret, i, 1));
        }
        if(rightTurret != null && rightTurret.GetState() == false)
        {
            StartCoroutine(WaveLaserController(rightTurret, i, -1));
        }
    }

    private IEnumerator WaveLaserController(BossComponent turret, int j, int flip)
    {
        int currentPhase = currentTransition;
        turret.Swivel(waveStartAngle[j]*flip, waveInitialSpeed[j]);
        yield return new WaitForSeconds(waveInitialDelay[j]);
        turret.StopSwivel();
        float currentAngle = waveStartAngle[j]*flip;
        turret.transform.rotation = Quaternion.Euler(new Vector3(0,0,currentAngle));
        for(int l = 0; l < waveSweeps[j]; l++)
        {
            for(int k = 0; k < waveShots[j] && turret.GetState() == false; k++)
            {
                Laser newLaser = turret.Fire(laserPrefab).GetComponent<Laser>();
                newLaser.transform.SetParent(projectileContainer.transform);
                newLaser.SetSpeed(waveShotSpeed[j]);
                AudioSource.PlayClipAtPoint(laserSound, turret.transform.position,1.0f);
                currentAngle += waveAngle[j]*flip*-1;
                turret.Swivel(currentAngle, waveShotDelay[j]);
                yield return new WaitForSeconds(waveShotDelay[j]);
                turret.StopSwivel();
                turret.transform.rotation = Quaternion.Euler(new Vector3(0,0,currentAngle));
                //turret.transform.Rotate(new Vector3(0, 0, waveAngle[j] * flip * -1));
            }
            flip = flip * -1;
        }
    }
    
    [SerializeField]
    private float[] megaSweepStartAngle;
    [SerializeField]
    private float[] megaSweepEndAngle;
    [SerializeField]
    private float[] megaSweepSwivelWait;
    [SerializeField]
    private float[] megaSweepSwivelTime;
    [SerializeField]
    private float[] megaSweepDuration;
    [SerializeField]
    private int[] megaSweepTracerPulses;
    [SerializeField]
    private float[] megaSweepTracerPulseDuration;
    [SerializeField]
    private float[] megaSweepStartDelay;
    [SerializeField]
    private float[] megaSweepStartSpeed;

    private void MegaLaserSweep()
    {
        BossComponent turretL = null;
        BossComponent turretR = null;
        switch(currentTransition)
        {
            case 1:
                    turretL = phase2Components[1].GetComponent<BossComponent>();
                    if(turretL.GetState() == false)
                    {
                        StartCoroutine(MegaSweepController(turretL, 0, 1, megaSweepStartAngle[0], megaSweepEndAngle[0]));
                    }
                    turretR = phase2Components[0].GetComponent<BossComponent>();
                    if(turretR.GetState() == false)
                    {
                        StartCoroutine(MegaSweepController(turretR, 0, -1, megaSweepStartAngle[0], megaSweepEndAngle[0]));
                    }
                break;
            case 2:
                if(alternateTurrets == false)
                {
                    turretL = phase2Components[1].GetComponent<BossComponent>();
                    if(turretL.GetState() == false)
                    {
                        StartCoroutine(MegaSweepController(turretL, 1, 1, megaSweepStartAngle[1], megaSweepEndAngle[1]));
                    }
                    turretR = phase2Components[0].GetComponent<BossComponent>();
                    if(turretR.GetState() == false)
                    {
                        StartCoroutine(MegaSweepController(turretR, 1, -1, megaSweepStartAngle[1], megaSweepEndAngle[1]));
                    }
                }
                else
                {
                    turretL = phase2Components[3].GetComponent<BossComponent>();
                    if(turretL.GetState() == false)
                    {
                        StartCoroutine(MegaSweepController(turretL, 1, -1, megaSweepStartAngle[1] - 90.0f, megaSweepEndAngle[1] - 90.0f));
                    }
                    turretR = phase2Components[2].GetComponent<BossComponent>();
                    if(turretR.GetState() == false)
                    {
                        StartCoroutine(MegaSweepController(turretR, 1, 1, megaSweepStartAngle[1] - 90.0f, megaSweepEndAngle[1] - 90.0f));
                    }
                }
                break;
            case 3: 
                turretL = phase3Components[0].GetComponent<BossComponent>();
                if(turretL.GetState() == false)
                {
                    StartCoroutine(MegaSweepController(turretL, 2, 1, megaSweepStartAngle[2], megaSweepEndAngle[2]));
                }
                turretR = phase3Components[1].GetComponent<BossComponent>();
                if(turretR.GetState() == false)
                {
                    StartCoroutine(MegaSweepController(turretR, 2, -1, megaSweepStartAngle[2], megaSweepEndAngle[2]));
                }
                break;
            default:
                break;
        }
        //Sweeps mega lasers back and forth from large turrets
        //Can be used in all 3 phases
    }

    private IEnumerator MegaSweepController(BossComponent turret, int j, int flip, float startAngle, float endAngle)
    {
        int currentPhase = currentTransition;
        turret.Swivel(startAngle*flip, megaSweepStartSpeed[j]);
        yield return new WaitForSeconds(megaSweepStartDelay[j]);
        turret.StopSwivel();
        turret.transform.rotation = Quaternion.Euler(new Vector3(0,0,startAngle*flip));
        
        Laser tracer = turret.Fire(tracerPrefab).GetComponent<Laser>();
        StartCoroutine(TracerPulse(megaSweepTracerPulses[j], megaSweepTracerPulseDuration[j], tracer));
        StartCoroutine(turret.WatchLaser(tracer));
        yield return new WaitForSeconds(megaSweepTracerPulseDuration[j] * megaSweepTracerPulses[j]);
        Destroy(tracer.gameObject);
        if(turret.GetState() == false)
        {
            Laser newGigaLaser = turret.Fire(giantLaserPrefab).GetComponent<Laser>();
            StartCoroutine(turret.WatchLaser(newGigaLaser));
            newGigaLaser.transform.SetParent(turret.transform);
            yield return new WaitForSeconds(megaSweepSwivelWait[j]);
            turret.Swivel(endAngle*flip, megaSweepSwivelTime[j]);
            yield return new WaitForSeconds(megaSweepDuration[j] - megaSweepSwivelWait[j]);
            if(newGigaLaser != null)
            {
                Destroy(newGigaLaser.gameObject);
            }
        }
    }

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

    [SerializeField]
    private int missileBarrageCycles = 2;
    [SerializeField]
    private float missileBarrageStartSpeed = 4;
    [SerializeField]
    private float missileBarrageDelays = 0.2f;
    [SerializeField]
    private float missileBarrageTopSpeed = 4.0f;
    [SerializeField]
    private float missileBarrageAcceleration = 4.0f;
    [SerializeField]
    private float missileBarrageInitialDelay = 4.0f;
    [SerializeField]
    private Vector3[] missileSpawnLocations;
    [SerializeField]
    private GameObject fakeMissilePrefab;


    private IEnumerator MissileBarrage()
    {
        int currentPhase = currentTransition;
        BossComponent launcherTL = phase1Components[5].GetComponent<BossComponent>();
        BossComponent launcherTR = phase1Components[6].GetComponent<BossComponent>();
        BossComponent launcherBL = phase1Components[7].GetComponent<BossComponent>();
        BossComponent launcherBR = phase1Components[8].GetComponent<BossComponent>();
        StartCoroutine(FakeMissiles(launcherBL));
        StartCoroutine(FakeMissiles(launcherTL));
        StartCoroutine(FakeMissiles(launcherBR));
        StartCoroutine(FakeMissiles(launcherTR));
        yield return new WaitForSeconds(missileBarrageInitialDelay);
        for(int k = 0; k < missileBarrageCycles; k++)
        {
            for(int i = 0; i < missileSpawnLocations.Length && currentTransition == currentPhase; i += 0)
            {
                for(int j = 0; j < 4; j++)
                {
                    if(missileSpawnLocations[i] != null)
                    {
                        int flip = 1;
                        if(missileSpawnLocations[i].x < 0)
                        {
                            flip = -1;
                        }
                        Laser newMissile = Instantiate(bossMissilePrefab, missileSpawnLocations[i], Quaternion.Euler(new Vector3(0,0,90*flip))).GetComponent<Laser>();
                        newMissile.SetBaseProperties(missileBarrageStartSpeed, 90*flip, 10.0f);
                        newMissile.SetMissileProperties(missileBarrageAcceleration, missileBarrageTopSpeed);
                        newMissile.transform.SetParent(projectileContainer.transform);
                    }
                    i++;
                }
                yield return new WaitForSeconds(missileBarrageDelays);
            }
            yield return new WaitForSeconds(missileBarrageInitialDelay);
        }
    }
    
    private IEnumerator FakeMissiles(BossComponent turret)
    {
        for(int j = 0; j < missileBarrageCycles; j++)
        {
            for(int i = 0; i < turret.GetSpawnContainers().Length; i++)
            {
                Laser newMissile = turret.Fire(fakeMissilePrefab, i).GetComponent<Laser>();
                newMissile.transform.SetParent(projectileContainer.transform);
                yield return new WaitForSeconds(missileBarrageDelays);
            }
        }
    }

    //Additional stage 1 attacks:
    //Summon Backup
    //Medium turret tracks and fires dumb missiles
    //Mega Laser Barrage
    //Wave Lasers

    //STAGE 2 ATTACK CONTROLLERS

    [SerializeField]
    private float diamondTracerPulseDuration = 0.5f;
    [SerializeField]
    private int diamondTracerPulses = 2;
    [SerializeField]
    private float diamondLaserDuration = 5.0f;
    [SerializeField]
    private float turretAngles = 120.0f;
    private bool diamondActive = false;
    [SerializeField]
    private float diamondSwivelTime = 1.0f;

    private void LaserDiamond()
    {
        diamondActive = true;
        StartCoroutine(DiamondTimer());
        BossComponent turretTL = phase2Components[1].GetComponent<BossComponent>();
        BossComponent turretTR = phase2Components[0].GetComponent<BossComponent>();
        BossComponent turretBL = phase2Components[3].GetComponent<BossComponent>();
        BossComponent turretBR = phase2Components[2].GetComponent<BossComponent>();

        if(turretTL.GetState() == false)
        {
            turretTL.Swivel(turretAngles, diamondSwivelTime/4);
            StartCoroutine(DiamondController(turretTL));
        }
        if(turretTR.GetState() == false)
        {
            turretTR.Swivel(turretAngles * -1, diamondSwivelTime/4);
            StartCoroutine(DiamondController(turretTR));
        }
        if(turretBL.GetState() == false)
        {
            turretBL.Swivel(turretAngles * -1, diamondSwivelTime/4);
            StartCoroutine(DiamondController(turretBL));
        }
        if(turretBR.GetState() == false)
        {
            turretBR.Swivel(turretAngles, diamondSwivelTime/4);
            StartCoroutine(DiamondController(turretBR));
        }

        //Followed up by converging laser rings OR enemy summon OR Wave Lasers OR multiple of these

    }

    private IEnumerator DiamondController(BossComponent turret)
    {
        yield return new WaitForSeconds(diamondSwivelTime);
        Laser tracer = turret.Fire(tracerPrefab).GetComponent<Laser>();
        StartCoroutine(TracerPulse(diamondTracerPulses, diamondTracerPulseDuration, tracer));
        StartCoroutine(turret.WatchLaser(tracer));
        yield return new WaitForSeconds(diamondTracerPulseDuration * diamondTracerPulses);
        Destroy(tracer.gameObject);
        if(turret.GetState() == false)
        {
            Laser newGigaLaser = turret.Fire(giantLaserPrefab).GetComponent<Laser>();
            newGigaLaser.transform.position = turret.transform.position;
            StartCoroutine(turret.WatchLaser(newGigaLaser));
            yield return new WaitForSeconds(diamondLaserDuration);
            if(newGigaLaser != null)
            {
                Destroy(newGigaLaser.gameObject);
            }
        }
    }

    private IEnumerator TracerPulse(int pulses, float duration, Laser beam)
    {
        SpriteRenderer beamSprite = beam.GetComponent<SpriteRenderer>();
        for(int i = 0; i < pulses; i++)
        {
            float alpha = 1.0f;
            while(alpha > 0.0f && beam != null)
            {
                Color beamColor = Color.white;
                beamColor.a = alpha;
                beamSprite.color = beamColor;
                alpha -= Time.deltaTime/duration;
                yield return null;
            }
        }
    }

    private IEnumerator DiamondTimer()
    {
        yield return new WaitForSeconds(diamondLaserDuration + diamondTracerPulseDuration*diamondTracerPulses);
        diamondActive = false;
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
