using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField]
    private float speed = 2.0f;
    [SerializeField]
    private int powerupID = 0;
    [SerializeField]
    private AudioClip audioClip;
    [SerializeField]
    private float dropRatio = 20.0f;
    [SerializeField]
    private bool hazardFlag = false;
    [SerializeField]
    private GameObject explosionPrefab;
    //[SerializeField]
    //private bool magnetized = false;
    [SerializeField]
    private float magneticBoost = 1.5f;

    private Player player;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("Cannot find player!");
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.C) && hazardFlag == false)
        {
            PickupCollect();
        }
        else
        {   
            //magnetized = false;
            Vector3 powerupMovement = new Vector3(0,speed*-1,0);
            transform.Translate(powerupMovement*Time.deltaTime);
        }

        if (transform.position.y <= -6.0f)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.tag == "Player")
        {
            Player player = other.transform.GetComponent<Player>();
            if (player != null)
            {
                player.collectPowerup(powerupID);
            }
            AudioSource.PlayClipAtPoint(audioClip,transform.position,1.0f);
            Destroy(this.gameObject);
        }
        else if(other.transform.tag == "Laser")
        {
            Laser laser = other.transform.GetComponent<Laser>();
            GiantLaser giantLaser = other.transform.GetComponent<GiantLaser>();
            bool laserAllegience = false;
            if(laser != null)
            {
                //False means non-hostile (player controlled)
                laserAllegience = laser.CallAllegiance();
                if (laserAllegience == false && hazardFlag == true)
                {
                    Destroy(other.gameObject);
                    GameObject explosion = Instantiate(explosionPrefab,transform.position, Quaternion.identity);
                    Destroy(explosion.gameObject,2.0f);
                    Destroy(this.gameObject);
                }
                else if (laserAllegience == true && hazardFlag == false)
                {
                    Destroy(other.gameObject);
                    Destroy(this.gameObject);
                }
            }
            else if(giantLaser != null)
            {
                laserAllegience = giantLaser.CallAllegiance();
                if (laserAllegience == false && hazardFlag == true)
                {
                    GameObject explosion = Instantiate(explosionPrefab,transform.position, Quaternion.identity);
                    Destroy(explosion.gameObject,2.0f);
                    Destroy(this.gameObject);
                }
                else if (laserAllegience == true && hazardFlag == false)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }

    private void PickupCollect()
    {
        //magnetized = true;
        Vector3 pickupMovement = player.transform.position - transform.position;
        transform.Translate(pickupMovement.normalized*speed*magneticBoost*Time.deltaTime);
    }

    public float CallDropChance()
    {
        return dropRatio;
    }

    public bool hazardCheck()
    {
        return hazardFlag;
    }
}