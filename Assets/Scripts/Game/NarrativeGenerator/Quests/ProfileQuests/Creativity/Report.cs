using ScriptableObjects;
using Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.NPCs;

namespace Game.NarrativeGenerator.Quests.QuestGrammarTerminals
{
    class Report : CreativityQuestSOs
    {
        public override string symbolType {
            get { return Constants.REPORT; }
        }
    }
}
