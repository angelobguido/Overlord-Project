﻿using System;
using MyBox;
using UnityEngine;

namespace Game.LevelGenerator.EvolutionaryAlgorithm
{
    [Serializable]
    public class FitnessParameters
    {
        [Foldout("Desired Fitness Values", true)]
        [SerializeField, Range(2, 200)] private int desiredRooms = 20;
        [SerializeField, Range(0, 50)] private int desiredKeys = 4;
        [SerializeField, Range(0, 50)] private int desiredLocks = 4;
        [SerializeField, Range(0, 200)] private int desiredEnemies = 40;
        [SerializeField, Range(0, 200)] private int desiredItems = 10;
        [SerializeField, Range(0, 200)] private int desiredNpcs = 3;
        [SerializeField, Range(1.0f, 3.0f)] private float desiredLinearity = 1.5f;
        
        public FitnessParameters(int rooms, int keys, int locks, int enemies, float linearCoefficient, int items, int npcs)
        {
            DesiredRooms = rooms;
            DesiredKeys = keys;
            DesiredLocks = locks;
            DesiredEnemies = enemies;
            DesiredItems = items;
            DesiredNpcs = npcs;
            DesiredLinearity = linearCoefficient;
        }
        
        public int DesiredRooms { get => desiredRooms; set => desiredRooms = value; }
        public int DesiredKeys { get => desiredKeys; set => desiredKeys = value; }
        public int DesiredLocks { get => desiredLocks; set => desiredLocks = value; }
        public int DesiredEnemies { get => desiredEnemies; set => desiredEnemies = value; }
        public int DesiredItems { get => desiredItems; set => desiredItems = value; }
        public int DesiredNpcs { get => desiredNpcs; set => desiredNpcs = value; }
        public float DesiredLinearity { get => desiredLinearity; set => desiredLinearity = value; }
    }
}