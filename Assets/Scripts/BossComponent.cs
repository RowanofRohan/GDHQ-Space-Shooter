using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossComponent : MonoBehaviour
{
    [SerializeField]
    private bool isDestructable = true;
    [SerializeField]
    private bool hasShield = false;
    [SerializeField]
    private GameObject[] projectileSpawnContainers;

    //Health
    [SerializeField]
    private float maxHealth = 30.0f;
    [SerializeField]
    private float currentHealth = 0.0f;
    [SerializeField]
    private bool isDestroyed = false;
    [SerializeField]
    private bool isInvulnerable = true;
    [SerializeField]
    private bool isShielded = false;

    //Flashing FX Data
    [SerializeField]
    private float flashDuration = 0.2f;
    [SerializeField]
    private float flashCooldown = 0.5f;
    private float canFlash = -0.1f;

    //Object Handles
    private FinalBoss boss;
    private GameObject prefabContainer;
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private Sprite[] damageSprites;
    private Player player;

    //Boss Controls
    [SerializeField]
    private bool isTracking = false;
    [SerializeField]
    private float trackingStickiness = 0.9f;
    [SerializeField]
    private float rotationTime = 0.25f;
    [SerializeField]
    private float trackingTime = 5.0f;
    private float timeTracker = 0.0f;
    private Quaternion rotation;
    private float angle;
    [SerializeField]
    private bool isSwivelling = false;
    [SerializeField]
    float tempVelocity = 0.0f;

    //Debug Controls
    [SerializeField]
    private float debugAngle;
    [SerializeField]
    private bool debugSmoothRotate = false;
    [SerializeField]
    private GameObject debugPrefab;
    [SerializeField]
    private bool fireSomething = false;
    [SerializeField]
    private bool changeShieldState = false;

    void Start()
    {
        currentHealth = maxHealth;
        boss = GameObject.Find("Destroyer_Boss").GetComponent<FinalBoss>();
        prefabContainer = boss.GetPrefab(0);
        isDestroyed = false;
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if(spriteRenderer == null)
        {
            Debug.Log("Sprite Renderer Missing!");
        }
        if (hasShield == true)
        {
            isShielded = true;
        }
        isSwivelling = false;
    }

    void Update()
    {
        if(isTracking == true)
        {
            TrackPlayer();
        }
        else if (isSwivelling == true)
        {
            SmoothRotate();
        }
        DebugController();
    }

    private void DebugController()
    {
        if(debugSmoothRotate == true)
        {
            debugSmoothRotate = false;
            Swivel(debugAngle, rotationTime);
        }
        if(fireSomething == true)
        {
            fireSomething = false;
            Fire(debugPrefab);
        }
        if(changeShieldState == true)
        {
            changeShieldState = false;
            ShieldController(!isShielded);
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public void TakeDamage(float damageTaken)
    {
        if(isInvulnerable == false)
        {
            currentHealth -= damageTaken;
            boss.TakeDamage(damageTaken);
            if (currentHealth <= 0)
            {
                DeathTrigger();
            }
            else if (Time.time >= canFlash)
            {
                StartCoroutine(DamageFlash());
            }
        }
    }

    private IEnumerator DamageFlash()
    {
        canFlash = Time.time + flashCooldown;
        if(isDestroyed == false)
        {
            spriteRenderer.sprite = damageSprites[1];
        }
        yield return new WaitForSeconds(flashDuration);
        if(isDestroyed == false)
        {
            spriteRenderer.sprite = damageSprites[0];
        }
    }

    private void DeathTrigger()
    {
        isDestroyed = true;
        isInvulnerable = true;
        GameObject deathExplosion = Instantiate(boss.GetPrefab(0), transform.position, Quaternion.identity);
        Destroy(deathExplosion.gameObject, 2.0f);
        spriteRenderer.sprite = damageSprites[2];
        boss.CheckPhaseChange();
        //try to spawn powerup via function in boss?
        //Set children damage fx to active
        //Need to still create them as children
    }

    public bool GetState()
    {
        return isDestroyed;
    }

    public bool CanBeTargeted()
    {
        if(isDestroyed == true || isInvulnerable == true || isDestructable == false)
        {
            return false;
        }
        else 
        {
            return true;
        }
    }

    public bool GetDestroyable()
    {
        return isDestructable;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.transform.tag == "Laser" && isInvulnerable == false)
        {
            Laser laser = other.transform.GetComponent<Laser>();
            if(laser != null)
            {
                if (other.GetComponent<Laser>().CallAllegiance() == false)
                {
                    if(isShielded == false)
                    {
                        float damageTemp = laser.CallDamage();
                        Destroy(other.gameObject);
                        TakeDamage(damageTemp);
                    }
                    else
                    {
                        Destroy(other.gameObject);
                    }
                }
            }
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if(other.transform.tag == "Laser" && isInvulnerable == false)
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

    public void SetPrefab(GameObject newPrefab)
    {
        prefabContainer = newPrefab;
    }

    public void SetInvuln(bool newState)
    {
        isInvulnerable = newState;
    }

    public void SetPlayer(Player newPlayer)
    {
        player = newPlayer;
    }

    public void StartTracking()
    {
        isTracking = true;
        timeTracker = 0.0f;
    }

    public void StopTracking()
    {
        isTracking = false;
    }

    public void FindPlayer()
    {
        Vector3 targetDirection = player.transform.position - gameObject.transform.position;
        angle = Mathf.Atan2(targetDirection.y,targetDirection.x)*Mathf.Rad2Deg - 90;
        transform.rotation = Quaternion.Euler(new Vector3(0,0,angle));
    }

    public void TrackPlayer()
    {
        Vector3 targetDirection = player.transform.position - gameObject.transform.position;
        angle = Mathf.Atan2(targetDirection.y,targetDirection.x)*Mathf.Rad2Deg - 90;
        //float newAngle = Mathf.LerpAngle(transform.rotation.eulerAngles.z, angle, trackingStickiness/100);
        float newAngle = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.z, angle, ref tempVelocity, trackingStickiness);
        transform.rotation = Quaternion.Euler(new Vector3(0,0,newAngle));
        if(trackingTime > 0)
        {
            timeTracker += Time.deltaTime;
            if(timeTracker >= trackingTime)
            {
                isTracking = false;
            }
        }
    }

    private void SmoothRotate()
    {
        float newAngle = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.z, angle, ref tempVelocity, rotationTime);
        if(Mathf.Abs(Mathf.DeltaAngle(newAngle, angle)) <= 0.01)
        {
            isSwivelling = false;
            transform.rotation = Quaternion.Euler(new Vector3(0,0,angle));
        }
        else
        {
            transform.rotation = Quaternion.Euler(new Vector3(0,0,newAngle));
        }
    }

    public void Swivel(float finalAngle, float rotateTime)
    {
        rotationTime = rotateTime;
        StopTracking();
        isSwivelling = true;
        angle = finalAngle;
        timeTracker = 0.0f;
    }
    
    public void StopSwivel()
    {
        isSwivelling = false;
    }

    public void Point(float finalAngle)
    {
        StopTracking();
        isSwivelling = false;
        transform.rotation = Quaternion.Euler(new Vector3(0,0,angle));
    }

    public GameObject Fire(GameObject prefabObject, int location = 0)
    {
        GameObject newObject = Instantiate(prefabObject, projectileSpawnContainers[location].transform.position, this.transform.rotation);
        newObject.GetComponent<Laser>().SetAngle(transform.rotation.eulerAngles.z);
        return newObject;
    }

    public void ShieldController(bool shieldCheck)
    {
        isShielded = shieldCheck;
        if(transform.Find("Boss Shield") != null)
        {
            transform.Find("Boss Shield").gameObject.SetActive(isShielded);
        }
    }

    public GameObject[] GetSpawnContainers()
    {
        return projectileSpawnContainers;
    }

    public IEnumerator WatchLaser(Laser gigaLaser)
    {
        while(gigaLaser != null)
        {
            if(isDestroyed == true)
            {
                Destroy(gigaLaser.gameObject);
            }
            else
            {
                yield return null;
            }
        }
    }
}
