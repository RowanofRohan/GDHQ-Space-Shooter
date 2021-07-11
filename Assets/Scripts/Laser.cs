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

    void Update()
    {
        Vector3 laserMovement = new Vector3(0,speed,0);
        if(hostile == false)
        {
            transform.Translate(laserMovement*Time.deltaTime);
        }
        else
        {
            transform.Translate (laserMovement*-1*Time.deltaTime);
        }
        if (transform.position.y >= 8 || transform.position.y <= -8)
        {
            if(transform.parent != null)
            {
                Destroy(transform.parent.gameObject);
            }
            Destroy(this.gameObject);
        }
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
}
