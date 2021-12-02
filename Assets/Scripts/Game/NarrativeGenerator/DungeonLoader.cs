﻿using Game.NarrativeGenerator.Quests;
using MyBox;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.NarrativeGenerator
{
    public class DungeonLoader : MonoBehaviour
    {
        [SerializeField]
        private bool isArena;
    
        [field: SerializeField]
        public DungeonFileSo SelectedDungeon { get; set; }
        [field: SerializeField]
        public QuestLine LevelQuestLine { get; set; }
        [field: SerializeField]
        public bool IsLastQuestLine { get; set; }

        [SerializeField] 
        private SceneReference dungeonScene;
    
        public static event LevelLoadEvent LoadLevelEventHandler;

        private void Start()
        {
            if (!dungeonScene.IsAssigned)
            {
                dungeonScene.SceneName = null;
                dungeonScene.SceneName = "LevelWithEnemies";
            }
            if (isArena)
            {
                LoadLevel();
            }
        }

        /// Load the level from the given filename.
        public void LoadLevel()
        {
            Debug.Log("Loading Level: "+SelectedDungeon.name + " Questline: "+ LevelQuestLine.name);
            Debug.Log("Enemies in Quest Line: "+ LevelQuestLine.EnemyParametersForQuestLine.NEnemies);
            Debug.Log("Enemy Types in Quest Line: "+ LevelQuestLine.EnemyParametersForQuestLine.TotalByType.EnemiesByTypeDictionary.Count);
            LoadLevelEventHandler?.Invoke(this, new LevelLoadEventArgs(SelectedDungeon, LevelQuestLine, IsLastQuestLine));
            SceneManager.LoadScene(dungeonScene.SceneName);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                LoadLevel();
            }
        }
    }
}
