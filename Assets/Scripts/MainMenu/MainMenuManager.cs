using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip buttonHoverSound;

    public void LoadGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void LoadBoss()
    {
        SceneManager.LoadScene("Boss_Game");
    }

    public void MouseButtonHoverSound()
    {
        AudioSource.PlayClipAtPoint(buttonHoverSound, Vector3.zero);
    }
}
