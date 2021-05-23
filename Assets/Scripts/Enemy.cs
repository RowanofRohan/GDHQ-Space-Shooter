using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float speed = 4.0f;

    [SerializeField]
    private float upperScreenBound = 8.0f;
    [SerializeField]
    private float lowerScreenBound = -6.0f;
    [SerializeField]
    private float leftScreenBound = -9.2f;
    [SerializeField]
    private float rightScreenBound = 9.2f;


    void Update()
    {
        Vector3 enemyMovement = new Vector3(0,speed*-1,0);
        transform.Translate(enemyMovement*Time.deltaTime);

        if (transform.position.y <= lowerScreenBound)
        {
            float randomX = Random.Range(leftScreenBound,rightScreenBound);
            transform.position = new Vector3(randomX,upperScreenBound - 0.0001f,0);
        }
        //Destroys enemy if it somehow flies off the top of the screen; not in use
        //else if (transform.position.y >= upperScreenBound + 0.001f)
        //{
        //    Destroy(this.gameObject);
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.tag == "Laser")
        {
            Destroy(other.gameObject);
            Destroy(this.gameObject);
        }
        else if (other.transform.tag == "Player")
        {
            Player player = other.transform.GetComponent<Player>();
            if (player != null)
            {
                player.Damage();
            }
            Destroy(this.gameObject);
        }
    }
}
