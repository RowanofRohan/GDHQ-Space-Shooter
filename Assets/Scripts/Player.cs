using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Player : MonoBehaviour
{
    //Movement
    [SerializeField]
    private float horizontalSpeed = 3.5f;
    [SerializeField]
    private float verticalSpeed = 3.5f;

    //Screen Bounds
    [SerializeField]
    private float upperScreenBound = 6f;
    [SerializeField]
    private float lowerScreenBound = -4f;
    [SerializeField]
    private float leftScreenBound = -9.2f;
    [SerializeField]
    private float rightScreenBound = 9.2f;

    //Fire Cotroller
    [SerializeField]
    private float laserCooldown = 0.2f;
    private float canFire = -1f;
    [SerializeField]
    private int maxAmmo = 15;
    [SerializeField]
    private int currentAmmo = 0;
    [SerializeField]
    private bool infiniteAmmo = false;
    [SerializeField]
    private bool consumeAmmo = false;
    [SerializeField]
    private bool laserOverride = false;
    [SerializeField]
    private float stunTimer = 1.0f;

    //Triple Shot
    [SerializeField]
    private bool tripleShot = false;
    [SerializeField]
    private float tripleShotDuration = 5.0f;

    //Giant Laser
    [SerializeField]
    private bool giantLaser = false;
    [SerializeField]
    private float giantLaserDuration = 5.0f;
    
    //SpeedBoost
    [SerializeField]
    private bool speedBoost = false;
    [SerializeField]
    private float speedBoostDuration = 5.0f;
    [SerializeField]
    private float speedBoostIntensity = 1.5f;
    
    //Shields
    [SerializeField]
    private bool shieldsUp = false;
    [SerializeField]
    private float shieldDuration = 10.0f;
    [SerializeField]
    private float shieldHealth = 3.0f;
    private float shieldCurrentHealth = 3.0f;

    //Shield Visuals
    [SerializeField]
    private GameObject shieldVisualizer;
    private SpriteRenderer shieldRenderer;
    [SerializeField]
    private Color shieldStrongColor = new Vector4(1,1,1,0.75f);
    [SerializeField]
    private Color shieldWeakcolor = new Vector4(1,0,0,0.75f);

    //Thrusters
    [SerializeField]
    private float thrusterMultiplier = 1.5f;
    [SerializeField]
    private float thrusterMaxDuration = 100.0f;
    private float thrusterCurrentDuration = 0.0f;
    [SerializeField]
    private float thrusterDrainRate = 40.0f;
    [SerializeField]
    private float thrusterRechargeRate = 10.0f;
    [SerializeField]
    private float thrusterMinimumCharge = 25.0f;
    private bool canUsethrusters = true;
    [SerializeField]
    private float thrusterChargeDelay = 1.0f;

    private bool canChargeThrusters = true;

    //Thruster Visuals
    // [SerializeField]
    // private GameObject thrusterVisualizer;
    // [SerializeField]
    // private float thrusterMinY = 0.4f;
    // [SerializeField]
    // private float thrusterMaxY = 0.8f;
    // [SerializeField]
    // private float thrusterStartY = -1.375f;
    // [SerializeField]
    // private float thrusterEndY = -2.75f;

    //Health _ Lives
    [SerializeField]
    private int playerHealth = 3;
    [SerializeField]
    private int maxHealth = 3;
    [SerializeField]
    private GameObject leftDamageFire;
    [SerializeField]
    private GameObject rightDamageFire;

    //Missiles
    [SerializeField]
    private int currentMissiles = 3;
    [SerializeField]
    private int maxMissiles = 3;
    [SerializeField]
    private float missileCooldown = 3.0f;
    [SerializeField]
    private int maxMissileTargets = 4;
    private float canMissile = -1.0f;

    [SerializeField]
    private float missileLaunchAngle = 45;
    [SerializeField]
    private float missileLaunchDuration = 1.0f;

    //Projectile Handles
    [SerializeField]
    private GameObject laserPrefab;
    [SerializeField]
    private GameObject tripleShotPrefab;
    [SerializeField]
    private GameObject giantLaserPrefab;
    [SerializeField]
    private GameObject missilePrefab;
    [SerializeField]
    private GameObject targetCrosshairPrefab;
    
    //Spawn Manager Handle
    private SpawnManager spawnManager;

    //UI Handles
    [SerializeField]
    private UIManager uiManager;
    [SerializeField]
    private int score = 0;

    //Audio Handles
    [SerializeField]
    private AudioSource playerAudio;
    [SerializeField]
    private AudioClip laserShot;
    [SerializeField]
    private AudioClip laserEmptySound;

    //Explosion Handles
    [SerializeField]
    private GameObject explosionPrefab;
    
    void Start()
    {
        transform.position = new Vector3(0,-3,0);
        spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        if (spawnManager == null)
        {
            Debug.LogError("Spawn Manager is NULL");
        }
        shieldRenderer = shieldVisualizer.GetComponent<SpriteRenderer>();
        shieldRenderer.color = shieldStrongColor;
        shieldVisualizer.SetActive(shieldsUp);
        giantLaserPrefab.SetActive(false);
        playerAudio = GetComponent<AudioSource>();
        if (playerAudio == null)
        {
            Debug.LogError("Player audio source is NULL");
        }
        else
        {
            playerAudio.clip = laserShot;
        }
        leftDamageFire.SetActive(false);
        rightDamageFire.SetActive(false);

        thrusterCurrentDuration = thrusterMaxDuration;
        canChargeThrusters = true;
        canUsethrusters = true;

        currentAmmo = maxAmmo;
        infiniteAmmo = false;
        consumeAmmo = true;
        SendAmmoUpdate();
    }

    void Update()
    {
        MovementController();
        FireController();
        MissileController();
    }
    
    void MovementController()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput*horizontalSpeed,verticalInput*verticalSpeed,0);

        if (ThrusterController())
        {
            movement = movement*thrusterMultiplier;
        }

        uiManager.UpdateThruster(thrusterCurrentDuration,thrusterMaxDuration,thrusterMinimumCharge);

        if(speedBoost == true)
        {
            movement = movement*speedBoostIntensity;
        }
        
        transform.Translate(movement*Time.deltaTime);
        //thrusterFX();

        //Screen bounding with Clamp; does not wrap
        transform.position = new Vector3(Mathf.Clamp(transform.position.x,leftScreenBound,rightScreenBound),Mathf.Clamp(transform.position.y,lowerScreenBound,upperScreenBound),0);

        //Screen wrapping code for x implemented but not used. Introduced small additions to float to prevent flickering if perfectly on border of bound.
        //if (transform.position.x <= leftScreenBound)
        //{
        //    transform.position = new Vector3(rightScreenBound - 0.0001f,transform.position.y,0);
        //}
        //else if(transform.position.x >= rightScreenBound)
        //{
        //    transform.position = new Vector3(leftScreenBound + 0.0001f,transform.position.y,0);
        //}
    }

    private bool ThrusterController()
    {
        if(Input.GetKeyUp(KeyCode.LeftShift) == true)
        {
            if(canChargeThrusters == true && canUsethrusters == true)
            {
                StartCoroutine(ThrusterDelay());
            }
        }
        if(Input.GetKey(KeyCode.LeftShift) == false)
        {
            if (canChargeThrusters)
            {
                thrusterCurrentDuration += thrusterRechargeRate*Time.deltaTime;
                if (thrusterCurrentDuration >= thrusterMaxDuration)
                {
                    thrusterCurrentDuration = thrusterMaxDuration;
                }
            }
            if(thrusterCurrentDuration >= thrusterMinimumCharge)
            {
                canUsethrusters = true;
            }
            else
            {
                canUsethrusters = false;
            }
            return false;
        }
        else
        {
            if(canUsethrusters)
            {
                thrusterCurrentDuration -= thrusterDrainRate * Time.deltaTime;
                if(thrusterCurrentDuration <= 0.0f)
                {
                    canUsethrusters = false;
                    thrusterCurrentDuration = 0.0f;
                    if(canChargeThrusters)
                    {
                        StartCoroutine(ThrusterDelay());
                    }
                }
                return true;
            }
            else
            {
                if(canChargeThrusters)
                {
                    thrusterCurrentDuration += thrusterRechargeRate*Time.deltaTime;
                    if (thrusterCurrentDuration >= thrusterMaxDuration)
                    {
                        thrusterCurrentDuration = thrusterMaxDuration;
                    }
                }
                if(thrusterCurrentDuration >= thrusterMinimumCharge)
                {
                    canUsethrusters = true;
                }
                return false;
            }
        }

    }

    IEnumerator ThrusterDelay()
    {
        canChargeThrusters = false;
        yield return new WaitForSeconds(thrusterChargeDelay);
        canChargeThrusters = true;
    }

    void FireController()
    {
        if (laserOverride != true && giantLaser != true)
        {
            if (Input.GetKey(KeyCode.Space) && Time.time > canFire)
            {
                canFire = Time.time + laserCooldown;
                if(currentAmmo > 0)
                {
                    playerAudio.clip = laserShot;
                    if (tripleShot == true)
                    {
                        Instantiate(tripleShotPrefab, transform.position + new Vector3 (0,0,0), Quaternion.identity);
                    }
                    else 
                    {
                        Instantiate(laserPrefab, transform.position + new Vector3(0,0.7f,0), Quaternion.identity);
                    }
                    if(infiniteAmmo != true && consumeAmmo != true)
                    {
                        currentAmmo -= 1;
                    }
                    SendAmmoUpdate();
                }
                else
                {
                    playerAudio.clip = laserEmptySound;
                }
                playerAudio.Play();
            }
        }
    }

    void MissileController()
    {
        if (Input.GetKeyDown(KeyCode.E) == true)
        {
            if(currentMissiles >= 1 && Time.time > canMissile)
            {
                canMissile = Time.time + missileCooldown;

                GameObject[] targets = GameObject.FindGameObjectsWithTag("Enemy");
                int livingTargets = 0;
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i].transform.GetComponent<Enemy>().DeathCheck() == false)
                    {
                        livingTargets += 1;
                    }
                }
                GameObject[] sortedTargets = new GameObject[livingTargets];

                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i].GetComponent<Enemy>().DeathCheck() == false)
                    {
                        for (int j = 0; j < sortedTargets.Length; j++)
                        {
                            if(sortedTargets[j] == null)
                            {
                                sortedTargets[j] = targets[i];
                                j = sortedTargets.Length;
                            }
                            else if (Vector3.Distance(targets[i].transform.position, transform.position) <= Vector3.Distance(sortedTargets[j].transform.position, transform.position))
                            {
                                for(int k = sortedTargets.Length - 1; k > j; k--)
                                {
                                    if(sortedTargets[k-1] != null)
                                    {
                                        sortedTargets[k] = sortedTargets[k-1];
                                    }
                                }
                                sortedTargets[j] = targets[i];
                                j = sortedTargets.Length;
                            }
                        }
                    }
                }


                int currentMissileTargets = maxMissileTargets;
                if (sortedTargets.Length < maxMissileTargets)
                {
                    currentMissileTargets = sortedTargets.Length;
                }

                if (sortedTargets.Length > 0)
                {
                    float minLaunchAngle = -1*missileLaunchAngle*maxMissileTargets/2;
                    float maxLaunchAngle = missileLaunchAngle*maxMissileTargets/2;
                    float missileAngle;
                    int currentTarget; 

                    for(int i = 0; i < maxMissileTargets; i++)
                    {
                        float ratio;
                        float currentWait;
                        if(maxMissileTargets > 1)
                        {
                            ratio = (float)i/(maxMissileTargets-1);
                            currentWait = Mathf.Lerp(0.0f, missileLaunchDuration, ratio);
                        }
                        else
                        {
                            ratio = 0.5f;
                            currentWait = 0.0f;
                        }
                        missileAngle = Mathf.LerpAngle(minLaunchAngle, maxLaunchAngle, ratio)*-1;

                        currentTarget = i % (sortedTargets.Length);

                        if (sortedTargets[currentTarget] == null)
                        {
                            Debug.Log("Sorted Slot " + currentTarget + " is NULL!");
                        }
                        
                        StartCoroutine(SpawnMissile(sortedTargets[currentTarget],missileAngle,currentWait));
                    }
                }
                if(currentMissileTargets >= 1)
                {
                    currentMissiles -= 1;
                    SendMissileUpdate();
                }
                else
                {
                    playerAudio.clip = laserEmptySound;
                    playerAudio.Play();
                }
            }
            else
            {
                playerAudio.clip = laserEmptySound;
                playerAudio.Play();
            }
        }
    }

    private IEnumerator SpawnMissile(GameObject enemyTarget, float launchAngle, float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject newMissile = Instantiate(missilePrefab, transform.position, Quaternion.identity);
        GameObject newCrosshair = Instantiate(targetCrosshairPrefab, enemyTarget.transform.position, Quaternion.identity);
        newMissile.GetComponent<Missiles>().SetTargets(enemyTarget,newCrosshair,launchAngle);
        newCrosshair.transform.SetParent(enemyTarget.transform);
    }

    public void Damage()
    {
        if(shieldsUp == true)
        {
            shieldCurrentHealth = shieldCurrentHealth - 1.0f;

            ShieldColorController((shieldCurrentHealth-1)/(shieldHealth-1));
            uiManager.UpdateShield(shieldCurrentHealth, shieldHealth);

            if (shieldCurrentHealth <= 0)
            {
                shieldsUp = false;
                shieldVisualizer.SetActive(shieldsUp);
                //StopCoroutine(ShieldTimer());
            }
        }
        else
        {
            playerHealth -= 1;
            uiManager.UpdateHealth(playerHealth);
            DamageFX();

            if (playerHealth <= 0)
            {
                spawnManager.GameOver();
                uiManager.GameOver();
                GameObject explosion = Instantiate(explosionPrefab,transform.position, Quaternion.identity);
                Destroy(explosion.gameObject,2.0f);
                Destroy(this.gameObject);
            }
        }
    }

    private void DamageFX()
    {
        if (playerHealth == 2)
        {
            float randomSide = Random.Range(0.0f, 100.0f);
            if (randomSide <= 50.0f)
            {
                leftDamageFire.SetActive(true);
            }
            else
            {
                rightDamageFire.SetActive(true);
            }

        }
        else if (playerHealth <= 1)
        {
            leftDamageFire.SetActive(true);
            rightDamageFire.SetActive(true);
        }

    }

    private void RepairFX()
    {
        if(playerHealth == 2)
        {
            float randomSide = Random.Range(0.0f, 100.0f);
            if (randomSide <= 50.0f)
            {
                leftDamageFire.SetActive(false);
            }
            else
            {
                rightDamageFire.SetActive(false);
            }
        }
        else if (playerHealth >= 3)
        {
            leftDamageFire.SetActive(false);
            rightDamageFire.SetActive(false);
        }
    }

    public void collectPowerup(int powerupID)
    {
        switch(powerupID)
        {
            case 1:
                //Triple Shot ID: 1
                TripleShotController();
                break;
            case 2:
                //Speed Boost ID: 2
                SpeedBoostController();
                break;
            case 3:
                //Shield ID: 3
                ShieldController();
                break;
            case 4:
                //Ammo ID: 4
                AmmoController();
                break;
            case 5:
                //1UP ID: 5
                OneUPController();
                break;
            case 6:
                //Giant Laser ID: 6
                GiantLaserController();
                break;
            case 7:
                //Land Mine ID: 7
                LandMineController();
                break;
            case 8:
                //Missile Pickup ID: 8
                MissilePickup();
                break;
            default:
                Debug.Log("Bad ID!");
                break;
        }
    }

    private void TripleShotController()
    {
        if(tripleShot == true)
        {
            StopCoroutine(TripleShotTimer());
            StartCoroutine(TripleShotTimer());
        }
        else
        {
            tripleShot = true;
            StartCoroutine(TripleShotTimer());
        }
    }

    IEnumerator TripleShotTimer()
    {
        currentAmmo = maxAmmo;
        infiniteAmmo = true;
        SendAmmoUpdate();
        yield return new WaitForSeconds(tripleShotDuration);
        infiniteAmmo = false;
        tripleShot = false;
        SendAmmoUpdate();
    }

    private void GiantLaserController()
    {
        if(giantLaser == true)
        {
            StopCoroutine(GiantLaserTimer());
            StartCoroutine(GiantLaserTimer());
        }
        else
        {
            giantLaser = true;
            StartCoroutine(GiantLaserTimer());
        }
    }

    IEnumerator GiantLaserTimer()
    {
        while(laserOverride == true)
        {
            yield return null;
        }
        infiniteAmmo = true;
        laserOverride = true;
        giantLaserPrefab.SetActive(true);
        SendAmmoUpdate();
        yield return new WaitForSeconds(giantLaserDuration);
        infiniteAmmo = false;
        currentAmmo = maxAmmo;
        giantLaser = false;
        laserOverride = false;
        giantLaserPrefab.SetActive(false);
        SendAmmoUpdate();
    }

    private void LandMineController()
    {
        Damage();
        if(playerHealth > 0)
        {
            GameObject explosion = Instantiate(explosionPrefab,transform.position, Quaternion.identity);
            Destroy(explosion.gameObject,2.0f);
            StartCoroutine(StunPlayer());
        }
    }

    private IEnumerator StunPlayer()
    {
        if(laserOverride != true)
        {
            laserOverride = true;
            yield return new WaitForSeconds(stunTimer);
            laserOverride = false;
        }
        else
        {
            yield return null;
        }
    }

    private void MissilePickup()
    {
        currentMissiles = maxMissiles;
        SendMissileUpdate();
    }

    private void SendMissileUpdate()
    {
        uiManager.UpdateMissiles(currentMissiles);
    }

    private void SpeedBoostController()
    {
        if(speedBoost == true)
        {
            StopCoroutine(SpeedBoostTimer());
            StartCoroutine(SpeedBoostTimer());
        }
        else
        {
            speedBoost = true;
            StartCoroutine(SpeedBoostTimer());
        }
    }

    IEnumerator SpeedBoostTimer()
    {
        yield return new WaitForSeconds(speedBoostDuration);
        speedBoost = false;
    }

    private void ShieldController()
    {
        //Uses overall health, has no duration limit
        shieldsUp = true;
        shieldVisualizer.SetActive(shieldsUp);
        shieldCurrentHealth = shieldHealth;
        ShieldColorController(1.0f);
        uiManager.UpdateShield(shieldCurrentHealth, shieldHealth);

        
        //This if statement uses shield timers; ignored in favor of health system
        // if(shieldsUp == true)
        // {
        //     StopCoroutine(ShieldTimer());
        //     StartCoroutine(ShieldTimer());
        // }
        // else
        // {
        //     shieldsUp = true;
        //     StartCoroutine(ShieldTimer());
        // }
    }

    private void ShieldColorController(float shieldPercent)
    {
        shieldRenderer.color = Color.Lerp(shieldWeakcolor,shieldStrongColor,shieldPercent);
    }

    private void AmmoController()
    {
        currentAmmo = maxAmmo;
        SendAmmoUpdate();
    }

    private void OneUPController()
    {
        playerHealth += 1;
        if(playerHealth > maxHealth)
        {
            playerHealth = maxHealth;
        }
        uiManager.UpdateHealth(playerHealth);
        RepairFX();
    }

    private void SendAmmoUpdate()
    {
        if (infiniteAmmo != true)
        {
            uiManager.UpdateAmmo(currentAmmo,maxAmmo);
        }
        else
        {
            uiManager.UpdateAmmo(999,999);
        }
    }

    IEnumerator ShieldTimer()
    {
        shieldVisualizer.SetActive(shieldsUp);
        shieldCurrentHealth = shieldHealth;
        yield return new WaitForSeconds(shieldDuration);
        shieldsUp = false;
        shieldVisualizer.SetActive(shieldsUp);
    }

    public void ScoreUpdate(int addScore)
    {
        score += addScore;
        uiManager.UpdateScore(score);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.transform.tag == "Laser")
        {
            Laser laser = other.transform.GetComponent<Laser>();
            if (laser != null)
            {
                //Checks hostilitiy; "true" represents hostile
                if(other.GetComponent<Laser>().CallAllegiance() == true)
                {
                    Damage();
                    Destroy(other.gameObject);
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if(other.transform.tag == "Laser")
        {
            GiantLaser giantLaser = other.transform.GetComponent<GiantLaser>();
            if (giantLaser != null)
            {
                if (giantLaser.CallAllegiance() == true)
                {
                    Damage();
                }
            }
        }
    }

    public void GameStart()
    {
        consumeAmmo = false;
        SendAmmoUpdate();
    }
}
