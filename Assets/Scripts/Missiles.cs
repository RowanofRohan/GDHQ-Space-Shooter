using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missiles : MonoBehaviour
{
    [SerializeField]
    private float missileSpeed = 15.0f;
    [SerializeField]
    private float accelTime = 0.5f;
    private float currentTime = 0.0f;
    [SerializeField]
    private GameObject target;
    [SerializeField]
    private GameObject crosshair;
    [SerializeField]
    private float damage = 3f;

    private float initialDirection = 0.0f;

    private Vector3 targetPosition;
    private Vector3 targetDirection;
    private bool targetDead;
    [SerializeField]
    private float angle;

    private Enemy enemyScript;
    private BossComponent bossScript;

    [SerializeField]
    private ParticleSystem emitter;

    //Explosion
    [SerializeField]
    private GameObject explosionPrefab;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        MovementController();
    }

    private void MovementController()
    {
        if(enemyScript != null)
        {
            targetDead = enemyScript.DeathCheck();
        }
        else if(bossScript != null)
        {
            targetDead = !bossScript.CanBeTargeted();
        }
        else
        {
            targetDead = true;
        }
        if (targetDead == false)
        {
            targetPosition = target.transform.position;
        }
        targetDirection = targetPosition - gameObject.transform.position;
        angle = Mathf.Atan2(targetDirection.y,targetDirection.x)*Mathf.Rad2Deg;
        currentTime += Time.deltaTime;
        float newAngle = Mathf.Lerp(initialDirection, angle - 90, currentTime/accelTime);
        if(Mathf.Abs(newAngle) > 0.0f)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0,0,newAngle));
        }
        Vector3 missileMovement = Vector3.up * missileSpeed;
        transform.Translate(missileMovement * Time.deltaTime);
        if( (missileSpeed * Time.deltaTime) > Vector3.Distance(targetPosition,transform.position) && targetDead == true)
        {
            Explode();
        }
    }

    public void SetTargets(GameObject newTarget, GameObject newCrosshair, float startingAngle)
    {
        target = newTarget;
        crosshair = newCrosshair;
        initialDirection = startingAngle;
        enemyScript = target.transform.GetComponent<Enemy>();
        bossScript = target.transform.GetComponent<BossComponent>();
    }

    public float CallDamage()
    {
        return damage;
    }
    
    public void SetDamage(float damageCheck)
    {
        damage = damageCheck;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.transform.tag == "Enemy" && other.transform.gameObject == target)
        {
            Enemy enemy = other.transform.GetComponent<Enemy>();
            if(enemy != null)
            {
                enemy.TakeDamage(damage);
                Explode();
            }
            else
            {
                BossComponent bossPiece = other.transform.GetComponent<BossComponent>();
                if(bossPiece != null)
                {
                    bossPiece.TakeDamage(damage);
                    Explode();
                }
            }
        }
    }

    private void Explode()
    {
        emitter.transform.parent = null;
        //emitter.emission.rateOverTime = 0;
        Destroy(emitter.transform.gameObject, 1.0f);
        GameObject explosion = Instantiate(explosionPrefab,transform.position, Quaternion.identity);
        Destroy(explosion.gameObject, 1.5f);
        Destroy(crosshair.gameObject);
        Destroy(this.gameObject);
    }
}
