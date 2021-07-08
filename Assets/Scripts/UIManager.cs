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

    [SerializeField]
    private Image healthImage;
    [SerializeField]
    private Sprite[] healthSprites;

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