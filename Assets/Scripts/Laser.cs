using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{

    [SerializeField]
    private float speed = 8f;
    [SerializeField]
    private float damage = 1f;

    void Update()
    {
        Vector3 laserMovement = new Vector3(0,speed,0);
        transform.Translate(laserMovement*Time.deltaTime);

        if (transform.position.y >= 8)
        {
            if(transform.parent != null)
            {
                Destroy(transform.parent.gameObject);
            }
            Destroy(this.gameObject);
        }
    }

    public float CallDamage()
    {
        return damage;
    }
}
