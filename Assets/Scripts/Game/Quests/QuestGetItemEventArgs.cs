﻿using ScriptableObjects;

namespace Game.Quests
{
    public class QuestGetItemEventArgs: QuestElementEventArgs
    {
        public ItemSo ItemType {get; set; }

        public QuestGetItemEventArgs(ItemSo itemType)
        {
            ItemType = itemType;
        }
    }
}