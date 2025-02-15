﻿using System;
using System.Collections;
using Game.Dialogues;
using Game.Events;
using Game.GameManager;
using Game.GameManager.Player;
using Game.LevelManager.DungeonLoader;
using Game.LevelManager.DungeonManager;
using Game.MenuManager;
using Game.NarrativeGenerator;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.DataCollection
{
    public class PlayerDataController : MonoBehaviour
    {
        public PlayerData CurrentPlayer { get; set; }
        private DungeonDataController _dungeonDataController;
        private const string PostDataURL = "http://damicore.icmc.usp.br/pag/data/upload.php?";
        private GameplayData _gameplayData;

        private void OnEnable()
        {
            DungeonLoader.StartMapEventHandler += OnMapStart;
            GameManagerSingleton.GameStartEventHandler += OnGameStart;
            HealthController.PlayerIsDamagedEventHandler += OnPlayerDamage;
            ProjectileController.EnemyHitEventHandler += IncrementCombo;
            ProjectileController.PlayerHitEventHandler += ResetCombo;
            BombController.PlayerHitEventHandler += ResetCombo;
            EnemyController.PlayerHitEventHandler += ResetCombo;
            TreasureController.TreasureCollectEventHandler += GetTreasure;
            KeyBhv.KeyCollectEventHandler += OnGetKey;
            EnemyController.KillEnemyEventHandler += OnKillEnemy;
            DialogueController.DialogueOpenEventHandler += OnInteractNPC;
            QuestGeneratorManager.ProfileSelectedEventHandler += OnProfileSelected;
            ExperimentController.ProfileSelectedEventHandler += OnExperimentProfileSelected;
            FormBhv.PreTestFormQuestionAnsweredEventHandler += OnPreTestFormAnswered;
            DoorBhv.KeyUsedEventHandler += OnKeyUsed;
            TriforceBhv.GotTriforceEventHandler += OnMapComplete;
            PlayerController.PlayerDeathEventHandler += OnDeath;
            GameOverPanelBhv.ToLevelSelectEventHandler += OnFormNotAnswered;
            GameOverPanelBhv.RestartLevelEventHandler += OnFormNotAnswered;
            FormBhv.PostTestFormQuestionAnsweredEventHandler += OnPostTestFormAnswered;
        }

        private void OnDisable()
        {
            DungeonLoader.StartMapEventHandler -= OnMapStart;
            GameManagerSingleton.GameStartEventHandler -= OnGameStart;
            HealthController.PlayerIsDamagedEventHandler -= OnPlayerDamage;
            ProjectileController.EnemyHitEventHandler -= IncrementCombo;
            ProjectileController.PlayerHitEventHandler -= ResetCombo;
            BombController.PlayerHitEventHandler -= ResetCombo;
            EnemyController.PlayerHitEventHandler -= ResetCombo;
            TreasureController.TreasureCollectEventHandler -= GetTreasure;
            KeyBhv.KeyCollectEventHandler -= OnGetKey;
            FormBhv.PreTestFormQuestionAnsweredEventHandler -= OnPreTestFormAnswered;
            DoorBhv.KeyUsedEventHandler -= OnKeyUsed;
            QuestGeneratorManager.ProfileSelectedEventHandler -= OnProfileSelected;
            ExperimentController.ProfileSelectedEventHandler -= OnExperimentProfileSelected;            
            EnemyController.KillEnemyEventHandler -= OnKillEnemy;
            DialogueController.DialogueOpenEventHandler -= OnInteractNPC;
            TriforceBhv.GotTriforceEventHandler -= OnMapComplete;
            PlayerController.PlayerDeathEventHandler -= OnDeath;
            FormBhv.PostTestFormQuestionAnsweredEventHandler -= OnPostTestFormAnswered;
        }

        private void Awake()
        {
            _gameplayData = new GameplayData();
        }

        private void Start()
        {
            _dungeonDataController = GetComponent<DungeonDataController>();
        }

        private void OnGameStart(object sender, EventArgs eventArgs)
        {
            CurrentPlayer = ScriptableObject.CreateInstance<PlayerData>();
            CurrentPlayer.Init();
        }
        
        private void OnMapStart(object sender, StartMapEventArgs eventArgs)
        {
            Debug.Log("Map Started");
            CurrentPlayer.StartDungeon(eventArgs.MapName, eventArgs.Map);
            _dungeonDataController.CurrentDungeon = CurrentPlayer.CurrentDungeon;
        }
        

        private void OnProfileSelected(object sender, ProfileSelectedEventArgs eventArgs)
        {
            CurrentPlayer.PlayerProfile = eventArgs.PlayerProfile;
        }

        private void OnExperimentProfileSelected(object sender, ProfileSelectedEventArgs eventArgs)
        {
            CurrentPlayer.GivenPlayerProfile = eventArgs.PlayerProfile;
        }
        
        private void ResetCombo(object sender, EventArgs eventArgs)
        {
            CurrentPlayer.ResetCombo();
        }
        
        private void IncrementCombo(object sender, EventArgs eventArgs)
        {
            CurrentPlayer.IncrementCombo();
        }
        
        private void OnPreTestFormAnswered(object sender, FormAnsweredEventArgs eventArgs)
        {
            CurrentPlayer.PreFormAnswers = eventArgs.AnswerValue;
        }
        
        private void OnKillEnemy(object sender, EventArgs eventArgs)
        {
            CurrentPlayer.IncrementKills();
        }

        private void OnInteractNPC(object sender, EventArgs eventArgs)
        {
            CurrentPlayer.IncrementInteractionsWithNpcs();
        }
        private void OnDeath(object sender, EventArgs eventArgs)
        {
            CurrentPlayer.IncrementDeaths();
        }
        
        private void OnMapComplete(object sender, EventArgs eventArgs)
        {
            CurrentPlayer.IncrementWins();
        }
        
        private void OnPlayerDamage(object sender, PlayerIsDamagedEventArgs eventArgs)
        {
            CurrentPlayer.AddLostHealth(eventArgs.DamageDone);
        }
        
        private void GetTreasure(object sender, TreasureCollectEventArgs eventArgs)
        {
            CurrentPlayer.AddCollectedTreasure(eventArgs.Amount);
        }
        
        private void OnGetKey(object sender, KeyCollectEventArgs eventArgs)
        {
            CurrentPlayer.IncrementCollectedKeys();
        }

        private void OnKeyUsed(object sender, KeyUsedEventArgs eventArgs)
        {
            CurrentPlayer.IncrementOpenedLocks();
        }

        private void OnFormNotAnswered(object sender, EventArgs eventArgs)
        {
#if UNITY_EDITOR
            CurrentPlayer.SaveAndRefreshAssets();
            CurrentPlayer.RefreshJson();
#endif
        }
        
        private void OnPostTestFormAnswered(object sender, FormAnsweredEventArgs eventArgs)
        {
            CurrentPlayer.AddPostTestDataToDungeon(eventArgs.AnswerValue);
            _gameplayData.SendProfileToServer(CurrentPlayer);
        }
        
#if UNITY_WEBGL
        public void SendJsonToServer()
        {
            StartCoroutine(PostData());
        }
        private IEnumerator PostData()
        {
            var data = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(CurrentPlayer));
            var form = new WWWForm();
            form.AddField("name", CurrentPlayer.PlayerId);
            form.AddBinaryData("data", data, CurrentPlayer.PlayerId + "-Player" + ".json", "application/json");
            using var www = UnityWebRequest.Post(PostDataURL, form);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
        }
#endif
    }
}