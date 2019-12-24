using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public static MainMenu instance = null;
    public GameObject introScreen, mainScreen, gameOverScreen;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    public void IntroScreen()
    {
        mainScreen.SetActive(false);
        introScreen.SetActive(true);
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("LevelWithEnemies");
    }
    public void CreateLevels()
    {
        SceneManager.LoadScene("LevelGenerator");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void GameOver()
    {
        gameOverScreen.SetActive(true);
    }
}
