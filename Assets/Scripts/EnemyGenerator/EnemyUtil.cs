using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyGenerator
{
    public static class EnemyUtil
    {
        //The population size of the EA
        public const int popSize = 10000;
        public const int crossChance = 99;
        public const int mutChance = 5;
        public const int maxGenerations = 1000;
        public const float desiredFitness = 30.0f;
        public const int nBestEnemies = 20;
        public const int minDamage = 1;
        public const int maxDamage = 11;
        public const int minHealth = 1;
        public const int maxHealth = 11;
        public const float minAtkSpeed = 1;
        public const float maxAtkSpeed = 11;
        public const float minMoveSpeed = 1;
        public const float maxMoveSpeed = 11;
        public const float minActivetime = 1;
        public const float maxActiveTime = 11;
        public const float minResttime = 1;
        public const float maxRestTime = 11;
        public const float minProjectileSpeed = 1;
        public const float maxProjectileSpeed = 11;
    }
}