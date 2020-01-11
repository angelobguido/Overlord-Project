using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public GameObject introScreen, mainScreen, gameOverScreen;

    public void IntroScreen()
    {
        mainScreen.SetActive(false);
        introScreen.SetActive(true);
    }
    public void PlayGame()
    {
        GameManager.instance.createMaps = true;
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
    public void PlayLoadedLevel()
    {
        GameManager.instance.createMaps = false;
        SceneManager.LoadScene("LevelWithEnemies");
    }
}
