﻿using System.Collections.Generic;
using ScriptableObjects;
using System;
using System.Linq;
using Game.NarrativeGenerator.ItemRelatedNarrative;
using UnityEngine;
using Util;

namespace Game.NarrativeGenerator.Quests.QuestGrammarTerminals
{
    [Serializable]
    public class ItemQuestSo : QuestSO
    {
        [field: SerializeField] public ItemAmountDictionary ItemsToCollectByType { get; set; }
        public override Dictionary<string, Func<int,int>> nextSymbolChances
        {
            get {
                Dictionary<string, Func<int, int>> getSymbolWeights = new Dictionary<string, Func<int, int>>();
                getSymbolWeights.Add( Constants.GET_TERMINAL,  Constants.OneOptionQuestLineWeight );
                getSymbolWeights.Add( Constants.EMPTY_TERMINAL, Constants.OneOptionQuestEmptyWeight);
                return getSymbolWeights;
            } 
        }
        public override string symbolType {
            get { return Constants.ITEM_TERMINAL; }
        }

        public override void Init()
        {
            base.Init();
            ItemsToCollectByType = new ItemAmountDictionary();
        }
        
        public override void Init(QuestSO copiedQuest)
        {
            base.Init(copiedQuest);
            ItemsToCollectByType = new ItemAmountDictionary();
            foreach (var itemByAmount in (copiedQuest as ItemQuestSo).ItemsToCollectByType)
            {
                ItemsToCollectByType.Add(itemByAmount.Key, itemByAmount.Value);
            }
        }

        public void Init(string name, bool endsStoryLine, QuestSO previous, ItemAmountDictionary itemsByType)
        {
            base.Init(name, endsStoryLine, previous);
            ItemsToCollectByType = itemsByType;
        }
        
        public override QuestSO Clone()
        {
            var cloneQuest = CreateInstance<ItemQuestSo>();
            cloneQuest.Init(this);
            return cloneQuest;
        }

        public void AddItem(ItemSo item, int amount)
        {
            if (ItemsToCollectByType.TryGetValue(item, out var currentAmount))
            {
                ItemsToCollectByType[item] = currentAmount + amount;
            }
            else
            {
                ItemsToCollectByType.Add(item, amount);
            }
        }
        
        public void SubtractItem(ItemSo itemSo)
        {
            ItemsToCollectByType[itemSo]--;
        }

        public bool CheckIfCompleted()
        {
            return ItemsToCollectByType.All(itemToCollect => itemToCollect.Value == 0);
        }
        
        public bool HasItemToCollect(ItemSo itemSo)
        {
            if (!ItemsToCollectByType.TryGetValue(itemSo, out var itemsLeft)) return false;
            return itemsLeft > 0;
        }
    }

}
