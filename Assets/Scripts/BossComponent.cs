using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossComponent : MonoBehaviour
{
    //Meta Information
    // [SerializeField]
    // private int componentID = 0;
    [SerializeField]
    private bool isDestructable = true;

    //Health
    [SerializeField]
    private float maxHealth = 30.0f;
    [SerializeField]
    private float currentHealth = 0.0f;
    [SerializeField]
    private bool isDestroyed = false;
    [SerializeField]
    private bool isInvulnerable = true;

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
    }

    void Update()
    {
        
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    private void TakeDamage(float damageTaken)
    {
        if(isInvulnerable == false)
        {
            currentHealth -= damageTaken;
            //Send Health Update Here
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
        //try to spawn powerup
        //Set children damage fx to active
        //Need to still create them as children
    }

    public bool GetState()
    {
        return isDestroyed;
    }

    public bool GetDestroyable()
    {
        return isDestructable;
    }

    // public int GetID()
    // {
    //     return componentID;
    // }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.transform.tag == "Laser" && isInvulnerable == false)
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
}
