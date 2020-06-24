using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelLoaderButtonBHV : MonoBehaviour
{
    string levelFile;
    int levelDifficulty;
    Button button;

    public delegate void LoadLevelButtonEvent(string fileName, int difficulty);
    public static event LoadLevelButtonEvent loadLevelButtonEvent;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnConfirmLevel);
    }

    protected void OnEnable()
    {
        LevelSelectButtonBHV.selectLevelButtonEvent += PrepareLevel;
    }

    protected void PrepareLevel(LevelConfigSO levelConfigSO)
    {
        levelFile = levelConfigSO.fileName;
        levelDifficulty = levelConfigSO.enemy;
    }

    void OnConfirmLevel()
    {
        loadLevelButtonEvent(levelFile, levelDifficulty);
    }
}
