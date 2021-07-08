using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private bool isGameOver;

    // Start is called before the first frame update
    void Start()
    {
        isGameOver = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver && Input.GetKeyDown("r"))
        {
            isGameOver = false;
            SceneManager.LoadScene("Game");
        }
    }

    public void GameOver()
    {
        isGameOver = true;
    }
}
