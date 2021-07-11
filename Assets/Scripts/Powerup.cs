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
    }
}