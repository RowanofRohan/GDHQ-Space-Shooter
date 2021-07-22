using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantLaser : MonoBehaviour
{
    [SerializeField]
    private float damage = 3f;
    [SerializeField]
    private bool hostile = false;


    // Update is called once per frame
    void Update()
    {
        
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
