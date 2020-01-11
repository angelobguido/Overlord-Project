using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyGenerator
{
    public static class EnemyUtil
    {
        //The population size of the EA
        public const int popSize = 1000;
        public const int crossChance = 99;
        public const int mutChance = 5;
        public const int maxGenerations = 100;
        public const float desiredFitness = 20.0f;
        public const int nBestEnemies = 20;
        public const int minDamage = 1;
        public const int maxDamage = 5;
        public const int minHealth = 1;
        public const int maxHealth = 11;
        public const float minAtkSpeed = 0.5f;
        public const float maxAtkSpeed = 5;
        public const float minMoveSpeed = 0.5f;
        public const float maxMoveSpeed = 1.5f;
        public const float minActivetime = 1.5f;
        public const float maxActiveTime = 10;
        public const float minResttime = 0.3f;
        public const float maxRestTime = 1.5f;
        public const float minProjectileSpeed = 3;
        public const float maxProjectileSpeed = 8;
    }
}