using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private bool isGameOver;

    private Scene scene;

    // Start is called before the first frame update
    void Start()
    {
        isGameOver = false;
        scene = SceneManager.GetActiveScene();
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver && Input.GetKeyDown("r"))
        {
            isGameOver = false;
            SceneManager.LoadScene(scene.name);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    public void GameOver()
    {
        isGameOver = true;
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
