﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public GameObject introScreen, introScreen2, mainScreen, gameOverScreen, difficultySelect, weaponSelect;

    public void IntroScreen()
    {
        //mainScreen.SetActive(false);
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

    public void RetryLevel()
    {
        GameManager.instance.createMaps = false;
        PlayerProfile.instance.OnRetry();
        SceneManager.LoadScene("LevelWithEnemies");
    }

    public void DifficultySelect()
    {
        weaponSelect.SetActive(false);
        difficultySelect.SetActive(true);
    }

    public void WeaponSelect()
    {
        introScreen2.SetActive(false);
        weaponSelect.SetActive(true);
    }

    public void SecondIntro()
    {
        introScreen.SetActive(false);
        introScreen2.SetActive(true);
    }

    protected void OnEnable()
    {
        WeaponLoaderBHV.loadDifficultySelect += DifficultySelect;
    }
}
