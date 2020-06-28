using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanelBHV : MonoBehaviour, IMenuPanel
{
    [SerializeField]
    GameObject nextPanel;
    [SerializeField]
    string GameScene;


    public void GoToNext()
    {
        nextPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    public void GoToPrevious()
    {
        PlayerProfile.instance.OnMapComplete(false);
        gameObject.SetActive(false);
        SceneManager.LoadScene(GameScene);
    }
}
