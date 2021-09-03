using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("General")]
    [SerializeField]
    private FinalBoss boss;
    [SerializeField]
    private int currentPhase = 0;
    [SerializeField]
    private int[] lastAttacks = {-1, -1};
    private WaitForSeconds attackDelay = new WaitForSeconds(0.0f);
    private float globalCooldownTimer = 0.0f;
    private float globalRecoveryTimer = 0.0f;

    //UNIVERSALLY USED PARAMETERS
    [Space(20)]
    [Header("Tracking Mega Laser Parameters")]
    [SerializeField]
    private int[] trackingTracerPulseCounts;
    [SerializeField]
    private float[] trackingTracerPulseDurations;
    [SerializeField]
    private float[] trackingTracerDelays;
    [SerializeField]
    private float[] trackingGigaLaserDurations;
    [SerializeField]
    private float[] trackingMegaLaserTrackTimes;
    
    [Space(10)]
    [Header("Dumb Missile Parameters")]
    [SerializeField]
    private float[] missileStartSpeed = {0.0f,0.0f,0.0f};
    [SerializeField]
    private float[] missileAccel = {6.0f,7.0f,8.0f};
    [SerializeField]
    private float[] missileTopSpeed = {24.0f,28.0f,32.0f};
    [SerializeField]
    private float missileLifetime = 5.0f;

    //PHASE 1 ID's: 
    //0: Summon
    //1: Missile Barrage
    //2: Mega Barrage
    //3: Spiral
    //4: Mega Sweep
    //5: Dumbfire Missile
    //6: Dumbfire Mega Laser
    [Space(20)]
    [Header("PHASE 1")]
    [SerializeField]
    private float cooldownPhase1Global;
    [SerializeField]
    private float[] cooldownsPhase1;
    //[SerializeField]
    //private int[] priorityPhase1;
    //[SerializeField]
    //private float[] frequencyPhase1;
    [SerializeField]
    private float[] recoveryPhase1;
    private float[] currentCooldownsP1;

    [Space(10)]
    [SerializeField]
    private float reverseConvergenceFrequencyPhase1 = 0.7f;


    //PHASE 2 ID's: 
    //0: Summon
    //1: Diamond Lasers
    //2: Sequential Mega Lasers
    //3: Simultaneous Mega Lasers
    //4: Spiral
    //5: Laser Ring
    //6: Mega Sweep
    //7: Dumbfire Mega Laser
    //8: Dumbfire Missile
    //9: Dumbfire Laser Burst
    [Space(20)]
    [Header("PHASE 2")]
    [SerializeField]
    private float cooldownPhase2Global;
    [SerializeField]
    private float[] cooldownsPhase2;
    //[SerializeField]
    //private int[] priorityPhase2;
    //[SerializeField]
    //private float[] frequencyPhase2;
    [SerializeField]
    private float[] recoveryPhase2;
    private float[] currentCooldownsP2;
    [SerializeField]
    private bool sequentialAttackActive = false;

    [SerializeField]
    private float[] sequentialLaserDelays;
    [SerializeField]
    private float[] sequentialLaserTrackTimes;
    [SerializeField]
    private float simulLaserDelay;
    [SerializeField]
    private float reverseConvergenceFrequencyPhase2 = 0.5f;


    //PHASE 3 ID's:
    //0: Summon
    //1: Mega Barrage
    //2: Mega Sweep
    //3: Missile Sweep
    //4: Spiral
    //5: Laser Ring
    //6: Dumbfire Laser Burst
    //7: Dumbfire Missile
    //8: Dumbfire Mega Laser
    //9: Dumbfire Small Lasers
    //10: Mine
    [Space(20)]
    [Header("PHASE 3")]
    [SerializeField]
    private float cooldownPhase3Global;
    [SerializeField]
    private float[] cooldownsPhase3;
    //[SerializeField]
    //private int[] priorityPhase3;
    //[SerializeField]
    //private float[] frequencyPhase3;
    [SerializeField]
    private float[] recoveryPhase3;
    private float[] currentCooldownsP3;
    [SerializeField]
    private float reverseConvergenceFrequencyPhase3 = 0.5f;

    void Start()
    {
        InitializeCooldowns();
    }

    void Update()
    {
        RunCooldowns();
    }

    private IEnumerator Phase1AIController()
    {
        //Debug.Log("Started Phase 1");
        boss.StartTracking(2);
        StartCoroutine(Phase1DumbMissileController());
        while(currentPhase == 1)
        {
            if(globalRecoveryTimer > 0.0f)
            {
                yield return null;
            }
            else
            {
                int randomAttack = 0;

                if(lastAttacks[0] == 4 || lastAttacks[1] == 4)
                {
                    int[] validAttacks = {0, 1, 3, 4, 6};
                    randomAttack = RandomizeAttack(validAttacks, lastAttacks.Length);
                }
                else
                {
                    int[] validAttacks = {0, 1, 2, 3, 4, 6};
                    randomAttack = RandomizeAttack(validAttacks, lastAttacks.Length);
                }

                if(currentCooldownsP1[randomAttack] > 0.0f)
                {
                    yield return null;
                }
                else
                {
                    bool attackSuccessful = false;
                    switch(randomAttack)
                    {
                        case 0: 
                        //Summons Backup
                            if(boss.GetTurretState(4) == false)
                            {
                                boss.AICommander(2);
                                attackSuccessful = true;
                            }
                            break;
                        case 1:
                            boss.AICommander(5);
                            attackSuccessful = true;
                            break;
                        case 2: 
                            bool flipflop = (Random.value > reverseConvergenceFrequencyPhase1);
                            boss.AICommander(3, flipflop);
                            attackSuccessful = true;
                            break;
                        case 3: 
                            if(boss.GetTurretState(0) == false || boss.GetTurretState(1) == false)
                            {
                                boss.AICommander(4);
                                attackSuccessful = true;
                            }
                            break;
                        case 4: 
                            boss.AICommander(9);
                            attackSuccessful = true;
                            break;
                        case 5:
                            //EMPTY: DUMB MISSILES NORMALLY GO HERE
                            attackSuccessful = false;
                            break;
                        case 6: 
                            if(boss.GetTurretState(3) == false)
                            {
                                StartTrackingMegaController(3, 0);
                                attackSuccessful = true;
                            }
                            break;
                    }
                    if(attackSuccessful)
                    {
                        currentCooldownsP1[randomAttack] = cooldownsPhase1[randomAttack];
                        ManageAttackList(randomAttack);
                        yield return CompareCooldowns(recoveryPhase1[randomAttack], cooldownPhase1Global);
                    }
                    else
                    {
                        yield return null;
                    }
                }
            }
        }
    }

    //0: SUMMON
    //1: DIAMOND
    //2: SEQUENTIAL MEGA
    //3: SIMUL MEGA
    //4: SPIRAL
    //5: LASER RING
    //6: MEGA SWEEP
    //7: DUMB MEGA
    //8: DUMB MISSILE
    //9: DUMB BURST
    private IEnumerator Phase2AIController()
    {
        boss.StartTracking(4);
        boss.StartTracking(5);
        StartCoroutine(Phase2DumbMissileController());
        StartCoroutine(Phase2LargeTurretController());
        while(currentPhase == 2)
        {
            if(globalRecoveryTimer > 0.0f)
            {
                yield return null;
            }
            else
            {
                int randomAttack = -1;
                if(TurretsBusy())
                {
                    int[] validAttacks = {0, 4, 5};
                    randomAttack = RandomizeAttack(validAttacks, lastAttacks.Length);
                }
                else
                {
                    int[] validAttacks = {0, 1, 2, 3, 4, 5, 6};
                    randomAttack = RandomizeAttack(validAttacks, lastAttacks.Length);
                }

                if(currentCooldownsP2[randomAttack] > 0.0f)
                {
                    yield return null;
                }
                else
                {
                    bool attackSuccessful = false;
                    switch(randomAttack)
                    {
                        case 0: 
                            if(boss.GetTurretState(8) == false)
                            {
                                boss.AICommander(2);
                                attackSuccessful = true;
                            }
                            break;
                        case 1:
                            if(boss.GetTurretState(0) == false || boss.GetTurretState(1) == false || boss.GetTurretState(2) == false || boss.GetTurretState(3))
                            {
                                boss.AICommander(6);
                                attackSuccessful = true;
                            }
                            break;
                        case 2: 
                            if(boss.GetTurretState(0) == false || boss.GetTurretState(1) == false || boss.GetTurretState(2) == false || boss.GetTurretState(3))
                            {
                                bool flopflip = (Random.value > reverseConvergenceFrequencyPhase2);
                                if(!flopflip)
                                {
                                    if(boss.GetTurretState(3) == false)
                                    {
                                        StartCoroutine(TrackingMegaController(3, trackingTracerPulseCounts[1], trackingTracerPulseDurations[1], sequentialLaserDelays[0], trackingGigaLaserDurations[1], sequentialLaserTrackTimes[0]));
                                    }
                                    if(boss.GetTurretState(1) == false)
                                    {
                                        StartCoroutine(TrackingMegaController(1, trackingTracerPulseCounts[1], trackingTracerPulseDurations[1], sequentialLaserDelays[1], trackingGigaLaserDurations[1], sequentialLaserTrackTimes[1]));
                                    }
                                    if(boss.GetTurretState(0) == false)
                                    {
                                        StartCoroutine(TrackingMegaController(0, trackingTracerPulseCounts[1], trackingTracerPulseDurations[1], sequentialLaserDelays[2], trackingGigaLaserDurations[1], sequentialLaserTrackTimes[2]));
                                    }
                                    if(boss.GetTurretState(2) == false)
                                    {
                                        StartCoroutine(TrackingMegaController(2, trackingTracerPulseCounts[1], trackingTracerPulseDurations[1], sequentialLaserDelays[3], trackingGigaLaserDurations[1], sequentialLaserTrackTimes[3]));
                                    }
                                }
                                else
                                {
                                    if(boss.GetTurretState(2) == false)
                                    {
                                        StartCoroutine(TrackingMegaController(2, trackingTracerPulseCounts[1], trackingTracerPulseDurations[1], sequentialLaserDelays[0], trackingGigaLaserDurations[1], sequentialLaserTrackTimes[0]));
                                    }
                                    if(boss.GetTurretState(0) == false)
                                    {
                                        StartCoroutine(TrackingMegaController(1, trackingTracerPulseCounts[1], trackingTracerPulseDurations[1], sequentialLaserDelays[1], trackingGigaLaserDurations[1], sequentialLaserTrackTimes[1]));
                                    }
                                    if(boss.GetTurretState(1) == false)
                                    {
                                        StartCoroutine(TrackingMegaController(0, trackingTracerPulseCounts[1], trackingTracerPulseDurations[1], sequentialLaserDelays[2], trackingGigaLaserDurations[1], sequentialLaserTrackTimes[2]));
                                    }
                                    if(boss.GetTurretState(3) == false)
                                    {
                                        StartCoroutine(TrackingMegaController(3, trackingTracerPulseCounts[1], trackingTracerPulseDurations[1], sequentialLaserDelays[3], trackingGigaLaserDurations[1], sequentialLaserTrackTimes[3]));
                                    }
                                }
                                attackSuccessful = true;
                            }
                            break;
                        case 3:
                            if(boss.GetTurretState(0) == false || boss.GetTurretState(1) == false || boss.GetTurretState(2) == false || boss.GetTurretState(3))
                            {
                                if(boss.GetTurretState(0) == false)
                                {
                                    StartCoroutine(TrackingMegaController(0, trackingTracerPulseCounts[1], trackingTracerPulseDurations[1], simulLaserDelay, trackingGigaLaserDurations[1], trackingMegaLaserTrackTimes[1]));
                                }
                                if(boss.GetTurretState(1) == false)
                                {
                                    StartCoroutine(TrackingMegaController(1, trackingTracerPulseCounts[1], trackingTracerPulseDurations[1], simulLaserDelay, trackingGigaLaserDurations[1], trackingMegaLaserTrackTimes[1]));
                                }
                                if(boss.GetTurretState(2) == false)
                                {
                                    StartCoroutine(TrackingMegaController(2, trackingTracerPulseCounts[1], trackingTracerPulseDurations[1], simulLaserDelay, trackingGigaLaserDurations[1], trackingMegaLaserTrackTimes[1]));
                                }
                                if(boss.GetTurretState(3) == false)
                                {
                                    StartCoroutine(TrackingMegaController(3, trackingTracerPulseCounts[1], trackingTracerPulseDurations[1], simulLaserDelay, trackingGigaLaserDurations[1], trackingMegaLaserTrackTimes[1]));
                                }
                                attackSuccessful = true;
                            }
                            break;
                        case 4: 
                            if(boss.GetTurretState(6) == false || boss.GetTurretState(7) == false)
                            {
                                boss.AICommander(4);
                                attackSuccessful = true;
                            }
                            break;
                        case 5:
                            boss.AICommander(7);
                            attackSuccessful = true;
                            break;
                        case 6: 
                            bool flipflop = (Random.value > 0.5f);
                            if((!flipflop && (boss.GetTurretState(0) == false || boss.GetTurretState(1) == false)) || (flipflop && (boss.GetTurretState(1) == false || boss.GetTurretState(2) == false)))
                            {
                                boss.AICommander(9, flipflop);
                                attackSuccessful = true;
                            }
                            else if(boss.GetTurretState(0) == true && boss.GetTurretState(1) == true && (boss.GetTurretState(2) == false || boss.GetTurretState(3) == false))
                            {
                                boss.AICommander(9, true);
                                attackSuccessful = true;
                            }
                            else if(boss.GetTurretState(2) == true && boss.GetTurretState(3) == true && (boss.GetTurretState(0) == false || boss.GetTurretState(1) == false))
                            {
                                boss.AICommander(9, !true);
                                attackSuccessful = true;
                            }
                            break;
                        default: 
                            Debug.Log("Something has gone horribly wrong...");
                            break;
                    }
                    if(attackSuccessful)
                    {
                        currentCooldownsP2[randomAttack] = cooldownsPhase2[randomAttack];
                        ManageAttackList(randomAttack);
                        yield return CompareCooldowns(recoveryPhase2[randomAttack], cooldownPhase2Global);
                    }
                    else
                    {
                        yield return null;
                    }
                }
            }
        }
    }


    //0: SUMMON
    //1: MEGA BARRAGE
    //2: MEGA SWEEP
    //3: MISSILE SWEEP
    //4: SPIRAL
    //5: LASER RING
    //6: DUMB BURST
    //7: DUMB MISSILE
    //8: DUMB MEGA
    //9: MINE
    private IEnumerator Phase3AIController()
    {
        boss.StartTracking(2);
        boss.StartTracking(3);
        boss.StartTracking(4);
        boss.StartTracking(5);
        StartCoroutine(Phase3DumbMissileController());
        StartCoroutine(Phase3LargeTurretController());
        StartCoroutine(Phase3MineController());
        while(currentPhase == 3)
        {
            if(globalRecoveryTimer > 0.0f)
            {
                yield return null;
            }
            else
            {
                int randomAttack = -1;
                if(TurretsBusy())
                {
                    int[] validAttacks = {0, 1, 3, 4, 5};
                    randomAttack = RandomizeAttack(validAttacks, lastAttacks.Length);
                }
                else
                {
                    int[] validAttacks = {0, 1, 2, 3, 4, 5};
                    randomAttack = RandomizeAttack(validAttacks, lastAttacks.Length);
                }

                if(currentCooldownsP3[randomAttack] > 0.0f)
                {
                    yield return null;
                }
                else
                {
                    bool attackSuccessful = false;
                    bool flipflop = false;
                    switch(randomAttack)
                    {
                        case 0: 
                            if(boss.GetTurretState(10) == false)
                            {
                                boss.AICommander(2);
                                attackSuccessful = true;
                            }
                            break;
                        case 1:
                            flipflop = (Random.value > reverseConvergenceFrequencyPhase3);
                            boss.AICommander(3, flipflop);
                            attackSuccessful = true;
                            break;
                        case 2: 
                            if(boss.GetTurretState(0) == false || boss.GetTurretState(1) == false)
                            {
                                boss.AICommander(9);
                                attackSuccessful = true;
                            }
                            break;
                        case 3:
                            if(boss.GetTurretState(12) == false)
                            {
                                boss.AICommander(8);
                                attackSuccessful = true;
                            }
                            break;
                        case 4: 
                            flipflop = (Random.value > 0.5f);
                            if((!flipflop && (boss.GetTurretState(9) == false || boss.GetTurretState(7) == false)) || (flipflop && (boss.GetTurretState(8) == false || boss.GetTurretState(6) == false)))
                            {
                                boss.AICommander(4, flipflop);
                                attackSuccessful = true;
                            }
                            else if(boss.GetTurretState(9) == true && boss.GetTurretState(7) == true && (boss.GetTurretState(8) == false || boss.GetTurretState(6) == false))
                            {
                                boss.AICommander(4, true);
                                attackSuccessful = true;
                            }
                            else if(boss.GetTurretState(8) == true && boss.GetTurretState(6) == true && (boss.GetTurretState(9) == false || boss.GetTurretState(7) == false))
                            {
                                boss.AICommander(4, !true);
                                attackSuccessful = true;
                            }
                            break;
                        case 5:
                            boss.AICommander(7);
                            attackSuccessful = true;
                            break;
                        default: 
                            Debug.Log("Something has gone horribly wrong...");
                            break;
                    }
                    if(attackSuccessful)
                    {
                        currentCooldownsP2[randomAttack] = cooldownsPhase2[randomAttack];
                        ManageAttackList(randomAttack);
                        yield return CompareCooldowns(recoveryPhase2[randomAttack], cooldownPhase2Global);
                    }
                    else
                    {
                        yield return null;
                    }
                }
            }
        }
    }

    private int RandomizeAttack(int[] attackArray, int attacksToCheck)
    {
        //BETTER METHOD?: Int array indexing which attacks are off cooldown
        //Check against array and only randomize if there are attacks that aren't on cooldown
        //while (randomAttack == lastAttacks[0])
        //{
        //    randomAttack = Random.Range(0, cooldownsPhase1.Length);
        //}
        bool foundAttack = false;
        int newAttack = lastAttacks[0];
        for(int i = 0; i < 100 && foundAttack == false; i++)
        {
            newAttack = attackArray[Random.Range(0, attackArray.Length)];
            foundAttack = true;
            for(int j = 0; j < attacksToCheck; j++)
            {
                if(newAttack == lastAttacks[j])
                {
                    foundAttack = false;
                }
            }
        }
        if(foundAttack)
        {
            return newAttack;
        }
        else
        {
            Debug.Log("Failed to find new attack!");
            return lastAttacks[0];
        }
    }

    private void ManageAttackList(int i)
    {
        for(int j = lastAttacks.Length - 1; j > 0; j--)
        {
            lastAttacks[j] = lastAttacks[j-1];
        }
        lastAttacks[0] = i;
    }

    private WaitForSeconds CompareCooldowns(float newCD, float globalCD)
    {
        if(newCD >= globalCD)
        {
            return new WaitForSeconds(newCD);
        }
        else
        {
            return new WaitForSeconds(globalCD);
        }
    }

    private IEnumerator Phase1DumbMissileController()
    {
        //Turret 2
        //Debug.Log("Started Missiles Firing");
        while(currentPhase == 1)
        {
            yield return new WaitForSeconds(cooldownsPhase1[5]);
            if(boss.GetTurretState(2) == false)
            {
                Laser newMissile = boss.FireTurret(2, 3).GetComponent<Laser>();
                InitializeMissile(newMissile, missileStartSpeed[0], missileTopSpeed[0], missileAccel[0], missileLifetime);
            }
        }
    }

    private IEnumerator Phase2DumbMissileController()
    {
        bool flipflop = (Random.value > 0.5f);
        // if(flipflop)
        // {
        //     boss.StartTracking(4);
        // }
        // else
        // {
        //     boss.StartTracking(5);
        // }
        //Turrets 4 and 5 are Medium
        bool check4 = boss.GetTurretState(4);
        bool check5 = boss.GetTurretState(5);
        while(currentPhase == 2 && (check4 == false || check5 == false))
        { 
            check4 = boss.GetTurretState(4);
            check5 = boss.GetTurretState(5);
            if(check4 == false && check5 == false)
            {
                yield return new WaitForSeconds(cooldownsPhase2[8]);
            }
            else
            {
                yield return new WaitForSeconds(cooldownsPhase2[8] * 2);
            }
            if((flipflop || check5 == true) && check4 == false)
            {
                Laser newMissile = boss.FireTurret(4, 3).GetComponent<Laser>();
                InitializeMissile(newMissile, missileStartSpeed[1], missileTopSpeed[1], missileAccel[1], missileLifetime);
            }
            if((!flipflop || check4) && check5 == false)
            {
                Laser newMissile = boss.FireTurret(5, 3).GetComponent<Laser>();
                InitializeMissile(newMissile, missileStartSpeed[1], missileTopSpeed[1], missileAccel[1], missileLifetime);
            }
            flipflop = !flipflop;
        }
    }

    [Header("Phase 2 Dumbfire Parameters")]
    [Space(20)]
    [SerializeField]
    private float p2LaserBurstTrackTime = 2.0f;
    [SerializeField]
    private float p2LaserBurstStagger = 0.2f;
    [SerializeField]
    private int p2LaserBurstShots = 4;
    [SerializeField]
    private float p2BurstFrequency = 0.8f;
    [SerializeField]
    private float p2BurstLaserDelay = 2.0f;
    [SerializeField]
    private float p2BurstLaserSpeed = 4.0f;
    [SerializeField]
    private float p2DestroyedTurretWait = 2.0f;

    private IEnumerator Phase2LargeTurretController()
    {
        //Right side: 0 and 2
        //Left side: 1 and 3
        //Top: 0 and 1
        //Center: 2 and 3
        bool[] check = {boss.GetTurretState(0), boss.GetTurretState(1), boss.GetTurretState(2), boss.GetTurretState(3)};
        int[] turretArray = {0, 1, 2, 3};
        float laserBurstWait = cooldownsPhase2[9];
        float megaTrackerWait = cooldownsPhase2[7];
        int i = 0;
        while(currentPhase == 2 && (check[0] == false || check[1] == false || check[2] == false || check[3] == false))
        {
            check = CheckTurrets(check, turretArray);
            laserBurstWait -= Time.deltaTime;
            megaTrackerWait -= Time.deltaTime;
            if(!TurretsBusy() && (laserBurstWait <= 0.0f || megaTrackerWait <= 0.0f))
            {
                bool bigLaser = (Random.value > p2BurstFrequency);
                if(check[i] == false)
                {
                    if(bigLaser && megaTrackerWait <= 0.0f)
                    {
                        StartTrackingMegaController(turretArray[i], 1);
                        laserBurstWait = cooldownsPhase2[9];
                        megaTrackerWait = cooldownsPhase2[7];
                    }
                    else if(laserBurstWait <= 0.0f)
                    {
                        StartCoroutine(LaserBurst(turretArray[i], p2LaserBurstShots, p2LaserBurstStagger, p2BurstLaserSpeed, p2LaserBurstTrackTime, p2BurstLaserDelay));
                        laserBurstWait = cooldownsPhase2[9];
                        megaTrackerWait = cooldownsPhase2[9];
                    }
                }
                else
                {
                    if(laserBurstWait < p2DestroyedTurretWait)
                    {
                        laserBurstWait = p2DestroyedTurretWait;
                    }
                    if(megaTrackerWait < p2DestroyedTurretWait)
                    {
                        megaTrackerWait = p2DestroyedTurretWait;
                    }
                }
                i++;
                i = i % check.Length;
            }
            yield return null;
        }
    }

    [Space(20)]
    [Header("Phase 3 Parameters")]
    [SerializeField]
    private float p3MissileDestroyedDelay = 1.0f;
    [SerializeField]
    private float p3BurstFrequency = 0.5f;
    [SerializeField]
    private float p3LaserBurstTrackTime = 2.0f;
    [SerializeField]
    private float p3LaserBurstStagger = 0.15f;
    [SerializeField]
    private int p3LaserBurstShots = 6;
    [SerializeField]
    private float p3BurstLaserDelay = 2.0f;
    [SerializeField]
    private float p3BurstLaserSpeed = 5.0f;
    [SerializeField]
    private float p3DestroyedTurretWait = 2.0f;


    private IEnumerator Phase3DumbMissileController()
    {
        bool[] check = {boss.GetTurretState(2), boss.GetTurretState(3), boss.GetTurretState(4), boss.GetTurretState(5)};
        int[] turretArray = {2, 3, 4, 5};
        float missileWait = cooldownsPhase3[7];
        int i = 0;
        while(currentPhase == 3 && (check[0] == false || check[1] == false || check[2] == false || check[3] == false))
        {
            check = CheckTurrets(check, turretArray);
            missileWait -= Time.deltaTime;
            if(missileWait <= 0.0f)
            {
                if(check[i] == false)
                {
                    Laser newMissile = boss.FireTurret(turretArray[i], 3).GetComponent<Laser>();
                    InitializeMissile(newMissile, missileStartSpeed[1], missileTopSpeed[1], missileAccel[1], missileLifetime);
                    missileWait = cooldownsPhase3[7];
                }
                else
                {
                    if(missileWait < p3MissileDestroyedDelay)
                    {
                        missileWait = p3MissileDestroyedDelay;
                    }
                }
                i++;
                i = i % check.Length;
            }
            yield return null;
        }
    }

    private IEnumerator Phase3LargeTurretController()
    {
        bool[] check = {boss.GetTurretState(0), boss.GetTurretState(1)};
        int[] turretArray = {0, 1};
        float laserBurstWait = cooldownsPhase3[6];
        float megaTrackerWait = cooldownsPhase3[8];
        int i = 0;
        while(currentPhase == 3 && (check[0] == false || check[1] == false))
        { 
            check = CheckTurrets(check, turretArray);
            laserBurstWait -= Time.deltaTime;
            megaTrackerWait -= Time.deltaTime;
            if(!TurretsBusy() && (laserBurstWait <= 0.0f || megaTrackerWait <= 0.0f))
            {
                bool bigLaser = (Random.value > p3BurstFrequency);
                if(check[i] == false)
                {
                    if(bigLaser && megaTrackerWait <= 0.0f)
                    {
                        StartTrackingMegaController(turretArray[i], 2);
                        laserBurstWait = cooldownsPhase3[6];
                        megaTrackerWait = cooldownsPhase3[8];
                    }
                    else if(laserBurstWait <= 0.0f)
                    {
                        StartCoroutine(LaserBurst(turretArray[i], p3LaserBurstShots, p3LaserBurstStagger, p3BurstLaserSpeed, p3LaserBurstTrackTime, p3BurstLaserDelay));
                        laserBurstWait = cooldownsPhase3[6];
                        megaTrackerWait = cooldownsPhase3[6];
                    }
                }
                else
                {
                    if(laserBurstWait < p3DestroyedTurretWait)
                    {
                        laserBurstWait = p3DestroyedTurretWait;
                    }
                    if(megaTrackerWait < p3DestroyedTurretWait)
                    {
                        megaTrackerWait = p3DestroyedTurretWait;
                    }
                }
                i++;
                i = i % check.Length;
            }
            yield return null;
        }
    }

    private IEnumerator Phase3MineController()
    {
        int i = Random.Range(0, 2);
        while(currentPhase == 3 && boss.GetTurretState(11) == false)
        {
            if(boss.GetTurretState(11) == false)
            {
                boss.FireTurret(11, 2, i);
            }
            i++;
            i = i % 2;
            yield return new WaitForSeconds(cooldownsPhase3[9]);
        }
    }

    private bool[] CheckTurrets(bool[] checkArray, int[] turretArray)
    {
        for(int i = 0; i < checkArray.Length; i++)
        {
            checkArray[i] = boss.GetTurretState(turretArray[i]);
        }
        return checkArray;
    }

    private bool TurretsBusy()
    {
        if(boss.AreTurretsBusy() == true || sequentialAttackActive)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private IEnumerator LaserBurst(int turretID, int laserCount, float laserStagger, float laserSpeed, float trackTime, float laserDelay)
    {
        if(boss.GetTurretState(turretID) == false)
        {
            sequentialAttackActive = true;
            boss.StartTracking(turretID, trackTime);
            yield return new WaitForSeconds(laserDelay);
            for(int i = 0; i < laserCount && boss.GetTurretState(turretID) == false; i++)
            {
                Laser newLaser = boss.FireTurret(turretID, 0).GetComponent<Laser>();
                newLaser.SetSpeed(laserSpeed);
                yield return new WaitForSeconds(laserStagger);
            }
            sequentialAttackActive = false;
        }
    }

    private void StartTrackingMegaController(int turretID, int phase)
    {
        StartCoroutine(TrackingMegaController(turretID, trackingTracerPulseCounts[phase], trackingTracerPulseDurations[phase], trackingTracerDelays[phase], trackingGigaLaserDurations[phase], trackingMegaLaserTrackTimes[phase]));
    }

    private IEnumerator TrackingMegaController(int turretID, int pulseCount, float pulseSpeed, float tracerDelay, float laserDuration, float trackTime)
    {
        if(boss.GetTurretState(turretID) == false)
        {
            sequentialAttackActive = true;
            boss.StartTracking(turretID, trackTime);
            yield return new WaitForSeconds(tracerDelay);
            if(boss.GetTurretState(turretID) == false)
            {
                Laser tracer = boss.FireTurret(turretID, 4).GetComponent<Laser>();
                boss.TurretWatch(turretID, tracer);
                StartCoroutine(boss.TracerPulse(pulseCount, pulseSpeed, tracer));
                yield return new WaitForSeconds(pulseSpeed * pulseCount);
                StartCoroutine(boss.TracerPulse(pulseCount, pulseSpeed/2, tracer));
                yield return new WaitForSeconds(pulseSpeed*pulseCount/2);
                if(tracer != null)
                {
                    Destroy(tracer.gameObject);
                }
                if(boss.GetTurretState(turretID) == false)
                {
                    boss.StopTracking(turretID);
                    Laser newGigaLaser = boss.FireTurret(turretID, 1).GetComponent<Laser>();
                    boss.TurretWatch(turretID, newGigaLaser);
                    StartCoroutine(boss.TracerPulse(1, laserDuration*1.5f, newGigaLaser));
                    yield return new WaitForSeconds(laserDuration);
                    if(newGigaLaser != null)
                    {
                        Destroy(newGigaLaser.gameObject);
                    }
                }
            }
            sequentialAttackActive = false;
        }
    }

    private void InitializeMissile(Laser missile, float speed1, float speed2, float accel, float lifespan)
    {
        missile.SetSpeed(speed1);
        missile.SetLifespan(lifespan);
        missile.SetMissileProperties(accel, speed2);
    }

    private void InitializeCooldowns()
    {
        currentCooldownsP1 = new float[cooldownsPhase1.Length];
        currentCooldownsP2 = new float[cooldownsPhase2.Length];
        currentCooldownsP3 = new float[cooldownsPhase3.Length];

        for(int i = 0; i < currentCooldownsP1.Length; i++)
        {
            currentCooldownsP1[i] = 0.0f;
        }

        for(int i = 0; i < currentCooldownsP2.Length; i++)
        {
            currentCooldownsP2[i] = 0.0f;
        }

        for(int i = 0; i < currentCooldownsP3.Length; i++)
        {
            currentCooldownsP3[i] = 0.0f;
        }
    }

    private void RunCooldowns()
    {
        if(AttackEnabled(currentPhase))
        {
            globalCooldownTimer -= Time.deltaTime;
            switch(currentPhase)
            {
                case 1: 
                    for(int i = 0; i < currentCooldownsP1.Length; i++)
                    {
                        currentCooldownsP1[i] -= Time.deltaTime;
                    }
                    break;
                case 2:
                    for(int i = 0; i < currentCooldownsP2.Length; i++)
                    {
                        currentCooldownsP2[i] -= Time.deltaTime;
                    }
                    break;
                case 3:
                    for(int i = 0; i < currentCooldownsP3.Length; i++)
                    {
                        currentCooldownsP3[i] -= Time.deltaTime;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private bool AttackEnabled(int i)
    {
        if(boss.GetPhase() == i && boss.CanAttack() == true)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void StartPhase(int newPhase)
    {
        //Debug.Log("Started New Phase");
        for(int i = 0; i < lastAttacks.Length; i++)
        {
            lastAttacks[i] = -1;
        }
        currentPhase = newPhase;
        switch(newPhase)
        {
            case 1:
                StartCoroutine(Phase1AIController());
                break;
            case 2:
                StartCoroutine(Phase2AIController());
                break;
            case 3:
                StartCoroutine(Phase3AIController());
                break;
            default:
                Debug.Log("Catastrophic Failure?!");
                break;
        }
    }

    public void EndPhase()
    {
        StopAllCoroutines();
        globalCooldownTimer = 0.0f;
        globalRecoveryTimer = 0.0f;
    }
}
