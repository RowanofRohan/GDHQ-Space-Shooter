using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{

    [SerializeField]
    private float speed = 8f;
    [SerializeField]
    private float damage = 1f;
    [SerializeField]
    private bool hostile = false;

    [SerializeField]
    private float fireAngle = 0.0f;
    private Quaternion rotation;
    private float timeTracker = 0.0f;
    [SerializeField]
    private float laserLifespan = 10.0f;

    [SerializeField]
    private int laserTypeID = 0;
    //0: Default Lasers
    //1: Dumbfire Missiles
    //2: Giant Lasers

    [SerializeField]
    private GameObject explosionPrefab;
    [SerializeField]
    private ParticleSystem emitter;
    [SerializeField]
    private float accelerationFactor = 0.0f;
    [SerializeField]
    private float topSpeed = 24.0f;

    void Update()
    {
        if(laserTypeID == 0)
        {
            MovementController();
        }
        if(laserTypeID == 1)
        {
            MissileController();
        }
    }

    // Original Movement Code; Deprecated
    // private void DefaultController()
    // {
    //     Vector3 laserMovement = new Vector3(0,speed,0);
    //     if(hostile == false)
    //     {
    //         transform.Translate(laserMovement*Time.deltaTime);
    //     }
    //     else
    //     {
    //         transform.Translate (laserMovement*-1*Time.deltaTime);
    //     }
    //     if (transform.position.y >= 8 || transform.position.y <= -8)
    //     {
    //         if(transform.parent != null)
    //         {
    //             Destroy(transform.parent.gameObject);
    //         }
    //         Destroy(this.gameObject);
    //     }
    // }

    private void MovementController()
    {
        timeTracker += Time.deltaTime;
        if(timeTracker >= laserLifespan)
        {
            Destroy(this.gameObject);
        }
        Vector3 laserMovement = new Vector3(0,speed, 0);
        rotation = Quaternion.Euler(0,0,fireAngle);
        transform.rotation = rotation;
        transform.Translate(laserMovement * Time.deltaTime);
    }

    private void MissileController()
    {
        speed += accelerationFactor * Time.deltaTime;
        if(speed >= topSpeed)
        {
            speed = topSpeed;
        }
        MovementController();
    }

    public void SetDamage(float damageCheck)
    {
        damage = damageCheck;
    }

    public float CallDamage()
    {
        return damage;
    }

    public void SetHostile(bool hostility)
    {
        hostile = hostility;
    }

    public bool CallAllegiance()
    {
        return hostile;
    }

    public void SetLifespan(float newLife)
    {
        laserLifespan = newLife;
    }

    public void SetAngle(float newAngle)
    {
        fireAngle = newAngle;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetBaseProperties(float newSpeed, float newAngle, float newLife)
    {
        SetSpeed(newSpeed);
        SetAngle(newAngle);
        SetLifespan(newLife);
    }

    public void SetLaserID(int newID)
    {
        laserTypeID = newID;
    }

    public int GetLaserID()
    {
        return laserTypeID;
    }

    public void SetMissileProperties(float newAccel, float newTopSpeed)
    {
        topSpeed = newTopSpeed;
        accelerationFactor = newAccel;
    }

    public void ResetTimer()
    {
        timeTracker = 0.0f;
    }

    public void Explode()
    {
        if(emitter != null && explosionPrefab != null)
        {
            emitter.transform.parent = null;
            Destroy(emitter.transform.gameObject, 1.0f);
            GameObject explosion = Instantiate(explosionPrefab,transform.position, Quaternion.identity);
            Destroy(explosion.gameObject, 1.5f);
            Destroy(this.gameObject); 
        }
    }
}
