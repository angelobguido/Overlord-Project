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
    }
}