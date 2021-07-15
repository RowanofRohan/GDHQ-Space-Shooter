using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private GameManager gameManager;
    
    [SerializeField]
    private Text scoreText;
    private int score = 0;

    [SerializeField]
    private Text gameOvertext;
    [SerializeField]
    private float gameOverFlickerTimer = 1.0f;
    [SerializeField]
    private Text restartText;

    //Health Handles
    [SerializeField]
    private Image healthImage;
    [SerializeField]
    private Sprite[] healthSprites;

    //Shield Handles
    [SerializeField]
    private Image shieldStatus;

    //Thruster Bar Handles
    [SerializeField]
    private Image thrusterStatus;
    [SerializeField]
    private Color thrusterFullColor = new Vector4(0,155, 236,1);
    [SerializeField]
    private Color thrusterPartialColor = Color.yellow;
    [SerializeField]
    private Color thrusterEmptyColor = Color.red;

    //Ammo Handles
    [SerializeField]
    private Text ammoText;
    [SerializeField]
    private Image ammoImage;
    private int ammoCount = 15;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Game Manager is NULL");
        }
        scoreText.text = "Score: " + score;
        gameOvertext.gameObject.SetActive(false);
        restartText.gameObject.SetActive(false);
        shieldStatus.gameObject.SetActive(false);
        ammoText.text = "x " + ammoCount;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateScore(int currentScore)
    {
        score = currentScore;
        scoreText.text = "Score: " + score;
    }

    public void UpdateHealth(int currentHealth)
    {
        healthImage.sprite = healthSprites[currentHealth];
    }

    public void UpdateShield(float shieldCurrentHealth, float shieldMaxhealth)
    {
        if (shieldCurrentHealth != 0 && shieldMaxhealth !=0)
        {
            shieldStatus.gameObject.SetActive(true);
            shieldStatus.fillAmount = shieldCurrentHealth/shieldMaxhealth;
        }
        else
        {
            shieldStatus.fillAmount = 0;
            shieldStatus.gameObject.SetActive(false);
        }
    }

    public void UpdateThruster(float thrusterCurrentDuration, float thrusterMaxDuration, float thrusterMinimumCharge)
    {
        if (thrusterMaxDuration != 0.0f)
        {
            float thrusterPercentage = thrusterCurrentDuration/thrusterMaxDuration;
            thrusterStatus.fillAmount = thrusterPercentage;
            if (thrusterPercentage >= 0.5f)
            {
                thrusterStatus.color = thrusterFullColor;
            }
            else if (thrusterCurrentDuration >= thrusterMinimumCharge)
            {
                thrusterStatus.color = thrusterPartialColor;
            }
            else
            {
                thrusterStatus.color = thrusterEmptyColor;
            }
        }
    }

    public void UpdateAmmo(int currentAmmo)
    {
        ammoCount = currentAmmo;
        ammoText.text = "x " + ammoCount;
    }

    public void GameOver()
    {
        restartText.gameObject.SetActive(true);
        gameManager.GameOver();
        StartCoroutine(GameOverFlicker());
    }

    IEnumerator GameOverFlicker()
    {
        while(true)
        {
            gameOvertext.gameObject.SetActive(true);
            yield return new WaitForSeconds(gameOverFlickerTimer);
            gameOvertext.gameObject.SetActive(false);
            yield return new WaitForSeconds(gameOverFlickerTimer);
        }
    }
}
