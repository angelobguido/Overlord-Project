﻿using System.Collections.ObjectModel;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu]
public class LevelConfigSO : ScriptableObject
{
    public DungeonSize dungeonSize;
    public DungeonLinearity dungeonLinearity;
    public string levelName;
    public EnemyDifficultyInDungeon enemyDifficulty;
    public string enemyDifficultyFile;
    public string fileName;
}

public enum EnemyDifficultyInDungeon
{ 
    VeryEasy = 11,
    Easy = 13,
    Medium = 15,
    Hard = 17,
    VeryHard = 19
}

public enum DungeonSize
{
    VerySmall = 16,
    Small = 20,
    Medium = 24,
    Large = 28,
    VeryLarge = 32
}

public enum DungeonLinearity
{
    VeryLinear,
    Linear,
    Medium,
    Branched,
    VeryBranched
}

public enum DungeonKeys
{
    AFewKeys = 1,
    SomeKeys = 3,
    SeveralKeys = 5,
    ManyKeys = 7,
    LotsOfKeys = 9
}


public static class DungeonLinearityConverter
{
    public static readonly ReadOnlyDictionary<DungeonLinearity, float> DungeonLinearityEnumToFloat
            = new ReadOnlyDictionary<DungeonLinearity, float>(new Dictionary<DungeonLinearity, float>
    {
                { DungeonLinearity.VeryLinear, 1.0f},
                { DungeonLinearity.Linear, 1.2f},
                { DungeonLinearity.Medium, 1.4f},
                { DungeonLinearity.Branched, 1.6f},
                { DungeonLinearity.VeryBranched, 1.8f},
    });

    public static float ToFloat(this DungeonLinearity dungeonLinearity)
    {
        return DungeonLinearityEnumToFloat[dungeonLinearity];
    }

}