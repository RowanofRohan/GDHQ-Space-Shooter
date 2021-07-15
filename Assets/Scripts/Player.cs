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

    //Triple Shot
    [SerializeField]
    private bool tripleShot = false;
    [SerializeField]
    private float tripleShotDuration = 5.0f;
    
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

    //Projectile Handles
    [SerializeField]
    private GameObject laserPrefab;
    [SerializeField]
    private GameObject tripleShotPrefab;
    
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
        infiniteAmmo = true;
        SendAmmoUpdate();
        yield return new WaitForSeconds(tripleShotDuration);
        infiniteAmmo = false;
        currentAmmo = maxAmmo;
        tripleShot = false;
        SendAmmoUpdate();
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
            uiManager.UpdateAmmo(currentAmmo);
        }
        else
        {
            uiManager.UpdateAmmo(999);
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

    public void GameStart()
    {
        consumeAmmo = false;
        SendAmmoUpdate();
    }
}
