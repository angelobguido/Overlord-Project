using System.Collections.Generic;
using Game.NarrativeGenerator.Quests.QuestGrammarTerminals;
using ScriptableObjects;
using UnityEngine;
using System;
using Util;

namespace Game.NarrativeGenerator.Quests.QuestGrammarNonterminals
{
    public class Explore : NonTerminalQuest
    {        
        public override Dictionary<string, Func<int,int>> nextSymbolChances
        {
            get
            {
                Dictionary<string, Func<int, int>> getSymbolWeights = new Dictionary<string, Func<int, int>>();
                getSymbolWeights.Add( Constants.ITEM_TERMINAL, Constants.ThreeOptionQuestLineWeight );
                getSymbolWeights.Add( Constants.DROP_TERMINAL, Constants.ThreeOptionQuestLineWeight );
                getSymbolWeights.Add( Constants.GET_TERMINAL, Constants.ThreeOptionQuestLineWeight );
                getSymbolWeights.Add( Constants.EMPTY_TERMINAL, Constants.ThreeOptionQuestEmptyWeight );
                return getSymbolWeights;
            } 
        }
        public override string symbolType {
            get { return Constants.EXPLORE_QUEST; }
        }
        public void DefineQuestSO ( List<QuestSO> questSos )
        {
            Explore.CreateAndSaveSecretRoomQuestSo( questSos );
        }

        public static void CreateAndSaveSecretRoomQuestSo( List<QuestSO> questSos)
        {
            var secretRoomQuest = ScriptableObject.CreateInstance<SecretRoomQuestSO>();
            secretRoomQuest.Init("Explore Room", false, questSos.Count > 0 ? questSos[questSos.Count-1] : null);
            //TODO initiate data for secretRoomQuest
            secretRoomQuest.SaveAsAsset();
            questSos.Add(secretRoomQuest);
        }
    }
}