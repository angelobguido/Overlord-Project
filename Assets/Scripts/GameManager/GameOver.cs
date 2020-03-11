using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void QuitButton()
    {
        GameManager.instance.LoadForm();
    }

    public void RestartButton()
    {
        PlayerProfile.instance.OnMapComplete(false);
        GameManager.instance.RestartGame();
    }

    public void MainMenuButton()
    {
        GameManager.instance.MainMenu();
    }
}
