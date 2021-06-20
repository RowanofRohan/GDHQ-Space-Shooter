using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
{
    [SerializeField]
    private float horizontalSpeed = 3.5f;
    [SerializeField]
    private float verticalSpeed = 3.5f;

    [SerializeField]
    private float upperScreenBound = 6f;
    [SerializeField]
    private float lowerScreenBound = -4f;
    [SerializeField]
    private float leftScreenBound = -9.2f;
    [SerializeField]
    private float rightScreenBound = 9.2f;

    [SerializeField]
    private float laserCooldown = 0.2f;
    private float canFire = -1f;

    [SerializeField]
    private bool tripleShot = false;
    [SerializeField]
    private float tripleShotDuration = 5.0f;
    
    [SerializeField]
    private bool speedBoost = false;
    [SerializeField]
    private float speedBoostDuration = 5.0f;
    [SerializeField]
    private float speedBoostIntensity = 1.5f;
    
    [SerializeField]
    private bool shieldsUp = false;
    [SerializeField]
    private float shieldDuration = 10.0f;
    [SerializeField]
    private int shieldHealth = 1;
    private int shieldCurrentHealth = 1;
    [SerializeField]
    private GameObject shieldVisualizer;


    [SerializeField]
    private int playerHealth = 3;

    [SerializeField]
    private GameObject laserPrefab;
    [SerializeField]
    private GameObject tripleShotPrefab;
    
    private SpawnManager spawnManager;
    


    //Screen wrap code written but not used
    //[SerializeField]
    //private float leftScreenBound = -11.2f;
    //[SerializeField]
    //private float rightScreenBound = 11.2f;

    
    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0,-3,0);
        spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        if (spawnManager == null)
        {
            Debug.LogError("Spawn Manager is NULL");
        }
        shieldVisualizer.SetActive(shieldsUp);
    }

    // Update is called once per frame
    void Update()
    {
        MovementController();
        FireController();
    }
    
    void MovementController()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput*horizontalSpeed,verticalInput*verticalSpeed,0);

        if(speedBoost == true)
        {
            transform.Translate((movement*speedBoostIntensity)*Time.deltaTime);
        }
        else
        {
            transform.Translate(movement*Time.deltaTime);
        }

        //Screen bounding with Clamp; does not wrap
        transform.position = new Vector3(Mathf.Clamp(transform.position.x,leftScreenBound,rightScreenBound),Mathf.Clamp(transform.position.y,lowerScreenBound,upperScreenBound),0);

        //Screen wrapping code for x implemented but not used. Introduced small additions to float to prevent flickering if perfectly on border of bound.
        //if (transform.position.x <= leftScreenBound)
        //{
        //    transform.position = new Vector3(rightScreenBound - 0.0001f,transform.position.y,0);
        //}
        //else if(transform.position.x >= rightScreenBound)
        //{
        //    transform.position = new Vector3(leftScreenBound + 0.0001f,transform.position.y,0);
        //}
    }

    void FireController()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time > canFire)
        {
            canFire = Time.time + laserCooldown;
            if (tripleShot == true)
            {
                Instantiate(tripleShotPrefab, transform.position + new Vector3 (0,0,0), Quaternion.identity);
            }
            else 
            {
                Instantiate(laserPrefab, transform.position + new Vector3(0,0.7f,0), Quaternion.identity);
            }
        }
    }

    public void Damage()
    {
        if(shieldsUp == false)
        {
            playerHealth -= 1;

            if (playerHealth <= 0)
            {
                spawnManager.GameOver();
                Destroy(this.gameObject);
            }
        }
        else
        {
            shieldCurrentHealth -= 1;

            if (shieldCurrentHealth <= 0)
            {
                shieldsUp = false;
                shieldVisualizer.SetActive(shieldsUp);
                StopCoroutine(ShieldTimer());
            }
        }
    }

    public void collectPowerup(int powerupID)
    {
        switch(powerupID)
        {
            case 1:
                //Triple Shot ID: 1
                TripleShotController();
                break;
            case 2:
                //Speed Boost ID: 2
                SpeedBoostController();
                break;
            case 3:
                //Shield ID: 3
                ShieldController();
                break;
            default:
                Debug.Log("Bad ID!");
                break;
        }
    }

    private void TripleShotController()
    {
        if(tripleShot == true)
        {
            StopCoroutine(TripleShotTimer());
            StartCoroutine(TripleShotTimer());
        }
        else
        {
            tripleShot = true;
            StartCoroutine(TripleShotTimer());
        }
    }

    IEnumerator TripleShotTimer()
    {
        yield return new WaitForSeconds(tripleShotDuration);
        tripleShot = false;
    }

    private void SpeedBoostController()
    {
        if(speedBoost == true)
        {
            StopCoroutine(SpeedBoostTimer());
            StartCoroutine(SpeedBoostTimer());
        }
        else
        {
            speedBoost = true;
            StartCoroutine(SpeedBoostTimer());
        }
    }

    IEnumerator SpeedBoostTimer()
    {
        yield return new WaitForSeconds(speedBoostDuration);
        speedBoost = false;
    }

    private void ShieldController()
    {
        if(shieldsUp == true)
        {
            StopCoroutine(ShieldTimer());
            StartCoroutine(ShieldTimer());
        }
        else
        {
            shieldsUp = true;
            StartCoroutine(ShieldTimer());
        }
    }

    IEnumerator ShieldTimer()
    {
        shieldVisualizer.SetActive(shieldsUp);
        shieldCurrentHealth = shieldHealth;
        yield return new WaitForSeconds(shieldDuration);
        shieldsUp = false;
        shieldVisualizer.SetActive(shieldsUp);
    }

    // IEnumerator PowerupTimer(float powerupDuration)
    // {
    //     yield return new WaitForSeconds(powerupDuration);
    //     tripleShot = false;
    // }
}
