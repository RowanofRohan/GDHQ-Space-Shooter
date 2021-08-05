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

    void Update()
    {
        Vector3 powerupMovement = new Vector3(0,speed*-1,0);
        transform.Translate(powerupMovement*Time.deltaTime);

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
            if(laser != null)
            {
                bool laserAllegience = other.GetComponent<Laser>().CallAllegiance();
                //False means non-hostile (player controlled)
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
        }
    }

    public float CallDropChance()
    {
        return dropRatio;
    }
}