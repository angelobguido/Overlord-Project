﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

namespace LevelGenerator
{
    public class Program
    {
        private readonly object crossoverLock = new object();
        private readonly object addChildLock = new object();
        private readonly object fitnessLock = new object();
        private readonly object minFitnessLock = new object();
        double min;
        double actual;
        public bool hasFinished;
        System.Diagnostics.Stopwatch watch;
        private GA gaObj = new GA();
        //Creates the first population of dungeons and generate their rooms
        List<Dungeon> dungeons;

        public Dungeon aux;

        public Program()
        {
            hasFinished = false;
            min = Double.MaxValue;
            //Console.WriteLine("New round!");
            watch = System.Diagnostics.Stopwatch.StartNew();
            dungeons = new List<Dungeon>(Constants.POP_SIZE);
            for (int i = 0; i < dungeons.Capacity; ++i) // Generate the first population
            {
                Dungeon individual = new Dungeon();
                individual.GenerateRooms();
                dungeons.Add(individual);
            }
            aux = dungeons[0];
        }

        public void CreateDungeon(TextMeshProUGUI progressText = null)
        {
            int matrixOffset = Constants.MATRIXOFFSET;
            hasFinished = false;
            min = Double.MaxValue;
            //Console.WriteLine("New round!");
            watch = System.Diagnostics.Stopwatch.StartNew();
            dungeons = new List<Dungeon>(Constants.POP_SIZE);
            for (int i = 0; i < dungeons.Capacity; ++i) // Generate the first population
            {
                Dungeon individual = new Dungeon();
                individual.GenerateRooms();
                dungeons.Add(individual);
            }
            aux = dungeons[0];

            //Evolve all the generations from the GA
            for (int gen = 0; gen < Constants.GENERATIONS; ++gen)
            {
                if(progressText!= null)
                    progressText.text = ((gen + 1) / (float)Constants.GENERATIONS)*100 + "%";
                //Interface.PrintNumericalGridWithConnections(dungeons[0]);
                //Interface.PrintTree(dungeons[0].RoomList[0]);
                //Interface.PrintGrid(dungeons[0].roomGrid);
                //Console.WriteLine("NEW GENERATION: "+gen);
                /*Console.WriteLine("Will Print");
                for(int i = 0; i < dungeons.Count; ++i)
                {
                    Interface.PrintNumericalGridWithConnections(dungeons[i]);
                }
                Console.WriteLine("Printed");*/

                foreach (Dungeon dun in dungeons)
                {
                    //Interface.PrintNumericalGridWithConnections(dun);
                    dun.fitness = gaObj.Fitness(dun, Constants.nV, Constants.nK, Constants.nL, Constants.lCoef, matrixOffset);
                    //Console.ReadKey();
                    //Console.Clear();
                    //yield return null;
                }

                //Elitism
                aux = dungeons[0];
                foreach (Dungeon dun in dungeons)
                {
                    //Interface.PrintNumericalGridWithConnections(dun);
                    actual = dun.fitness;
                    if (min > actual)
                    {
                        min = actual;
                        aux = dun;
                    }
                }

                
                //Create the child population by doing the crossover and mutation
                List<Dungeon> childPop = new List<Dungeon>(dungeons.Count);
                for (int i = 0; i < (dungeons.Count / 2); ++i)
                {
                    int parentIdx1 = 0, parentIdx2 = 1;
                    GA.Tournament(dungeons, ref parentIdx1, ref parentIdx2);
                    //Console.WriteLine("Selected!");
                    Dungeon parent1 = dungeons[parentIdx1].Copy();
                    Dungeon parent2 = dungeons[parentIdx2].Copy();

                    //GA.Crossover(ref parent1, ref parent2, ref child1, ref child2);
                    //The children weren't used, so the method was changed, as the crossover happens in the parents' copies
                    try
                    {
                        //Console.WriteLine("Will Cross");
                        //Interface.PrintNumericalGridWithConnections(parent1);
                        //Interface.PrintNumericalGridWithConnections(parent2);

                        GA.Crossover(ref parent1, ref parent2);
                        //Console.WriteLine("Crossed!");
                        //Console.WriteLine("Crossed");
                        //Interface.PrintNumericalGridWithConnections(parent1);
                        //Interface.PrintNumericalGridWithConnections(parent2);

                        //Mutation is disabled for now as it must be fixed
                        aux = dungeons[0];
                        GA.Mutation(ref parent1);
                        GA.Mutation(ref parent2);
                        //Console.WriteLine("Mutated");
                        //aux.FixRoomList();
                        parent1.FixRoomList();
                        parent2.FixRoomList();
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log(e.Message);
                        Util.OpenUri("https://stackoverflow.com/search?q=" + e.Message);
                        //yield break;
                    }
                    //Calculate the average number of children from the rooms in each children
                    parent1.CalcAvgChildren();
                    parent2.CalcAvgChildren();
                    //Console.WriteLine("Averaged");
                    //Add the children to the new population
                    childPop.Add(parent1);
                    childPop.Add(parent2);
                    //Console.WriteLine("Added");
                    //yield return null;
                }

                //Elitism
                childPop[0] = aux;
                dungeons = childPop;
                //Console.WriteLine("Elit");
                //Console.WriteLine("GEN "+gen+" COMPLETED!");
                //yield return null;
            }
            /*
            for (int i = 0; i < dungeons.Count; ++i)
            {
                Interface.PrintNumericalGridWithConnections(dungeons[i]);
            }*/
            //Find the best individual in the final population and print it as the answer
            min = Double.MaxValue;
            aux = dungeons[0];
            foreach (Dungeon dun in dungeons)
            {
                dun.fitness = gaObj.Fitness(dun, Constants.nV, Constants.nK, Constants.nL, Constants.lCoef, matrixOffset);
                actual = dun.fitness;
                if (min > actual)
                {
                    min = actual;
                    aux = dun;
                }
            }
            //Console.WriteLine("Found best");
            watch.Stop();
            long time = watch.ElapsedMilliseconds;
            //Console.WriteLine("AVGChildren: " + aux.AvgChildren+ " desiredKeys: "+aux.DesiredKeys);
            //Interface.PrintGridWithConnections(aux.roomGrid);
            //Console.WriteLine("Fitness: "+min);

            hasFinished = true;

            //CSVManager.SaveCSVLevel(id, aux.nKeys, aux.nLocks, aux.RoomList.Count, aux.AvgChildren, aux.neededLocks, aux.neededRooms, min, time, Constants.RESULTSFILE+"-"+Constants.nV+"-" + Constants.nK + "-" + Constants.nL + "-" + Constants.lCoef + ".csv");
            Debug.Log("Finished - fitnes:" + aux.fitness);
            Debug.Log("R:"+ aux.RoomList.Count+"-K:" + aux.nKeys + "-L:"+ aux.nLocks + "-Lin:"+aux.AvgChildren +"-nL:"+aux.neededLocks+"-nR:"+aux.neededRooms);
            Debug.Log("nRdelta:"+System.Math.Abs(aux.RoomList.Count * 0.8f - aux.neededRooms)+"-80p:"+ aux.RoomList.Count * 0.8f);

            //Console.WriteLine("Saved!");
            //Console.WriteLine("AVGChildren: " + aux.AvgChildren + "Fitness: " + min);
            //Console.WriteLine("Locks: " + aux.nLocks + "Needed: " + aux.neededLocks);
            Interface.PrintNumericalGridWithConnections(aux);
            //Console.WriteLine("OVER!");
            //Console.ReadLine();
            //AStar.FindRoute(aux);

            //dungeons.Clear();
        }


        public IEnumerator CreateDungeonParallel(TextMeshProUGUI progressText = null)
        {
            int matrixOffset = Constants.MATRIXOFFSET;
            float startTime = Time.realtimeSinceStartup;
            hasFinished = false;
            min = Double.MaxValue;
            //Console.WriteLine("New round!");
            watch = System.Diagnostics.Stopwatch.StartNew();
            dungeons = new List<Dungeon>(Constants.POP_SIZE);
            for (int i = 0; i < dungeons.Capacity; ++i) // Generate the first population
            {
                Dungeon individual = new Dungeon();
                individual.GenerateRooms();
                dungeons.Add(individual);
                yield return null;
            }
            aux = dungeons[0];

            //Evolve all the generations from the GA
            for (int gen = 0; gen < Constants.GENERATIONS; ++gen)
            {
                if (progressText != null)
                    progressText.text = ((gen + 1) / (float)Constants.GENERATIONS) * 100 + "%";

                foreach (Dungeon dun in dungeons)
                {
                    dun.fitness = gaObj.Fitness(dun, Constants.nV, Constants.nK, Constants.nL, Constants.lCoef, matrixOffset);
                    yield return null;
                }

                //Something is wrong with the indexes here... TODO: fix
                /*Parallel.ForEach(dungeons, (dun) =>
                {
                    Dungeon tempDun;
                    int _nV, _nK, _nL;
                    float _lCoef;
                    lock (fitnessLock)
                    {
                        tempDun = dun.Copy();
                        _nV = Constants.nV;
                        _nK = Constants.nK;
                        _nL = Constants.nL;
                        _lCoef = Constants.lCoef;
                    }
                    tempDun.fitness = gaObj.Fitness(tempDun, _nV, _nK, _nL, _lCoef, matrixOffset);
                    lock (minFitnessLock)
                    {
                        dun.fitness = tempDun.fitness;
                    }
                });*/
                yield return null;

                //Elitism
                aux = dungeons[0];
                /*Parallel.ForEach(dungeons, (dun) =>
                {
                    Dungeon tempDun;
                    lock (fitnessLock)
                    {
                        tempDun = dun.Copy();
                    }
                    actual = tempDun.fitness;
                    if (min > actual)
                    {
                        min = actual;
                        lock (minFitnessLock)
                        {
                            aux = dun;
                        }
                    }
                });*/

                foreach(Dungeon dun in dungeons)
                {
                    Dungeon tempDun;
                    
                    tempDun = dun.Copy();
                    
                    actual = tempDun.fitness;
                    if (min > actual)
                    {
                        min = actual;
                        aux = dun;
                    }
                };

                yield return null;
                //Create the child population by doing the crossover and mutation
                List<Dungeon> childPop = new List<Dungeon>(dungeons.Count);
                Parallel.For(0, (dungeons.Count / 2), (i) =>
                {
                    int parentIdx1 = 0, parentIdx2 = 1;
                    Dungeon parent1;
                    Dungeon parent2;
                    lock (crossoverLock)
                    {
                        GA.Tournament(dungeons, ref parentIdx1, ref parentIdx2);

                        parent1 = dungeons[parentIdx1].Copy();
                        parent2 = dungeons[parentIdx2].Copy();
                        //Debug.Log("Copied Parents");
                    }
                    try
                    {
                        GA.Crossover(ref parent1, ref parent2);
                        GA.Mutation(ref parent1);
                        GA.Mutation(ref parent2);
                        parent1.FixRoomList();
                        parent2.FixRoomList();
                        //Debug.Log("Crossed and Fixed");
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log(e.Message);
                        System.Console.WriteLine(e.Message);
                        Util.OpenUri("https://stackoverflow.com/search?q=" + e.Message);
                    }
                    //Calculate the average number of children from the rooms in each children
                    parent1.CalcAvgChildren();
                    parent2.CalcAvgChildren();
                    //Add the children to the new population
                    lock (addChildLock)
                    {
                        childPop.Add(parent1);
                        childPop.Add(parent2);
                        //Debug.Log("Added");
                    }
                    
                });

                //Elitism
                childPop[0] = aux;
                dungeons = childPop;
                yield return null;
            }
            //Find the best individual in the final population and print it as the answer
            min = Double.MaxValue;
            aux = dungeons[0];
            foreach (Dungeon dun in dungeons)
            {
                gaObj.Fitness(dun, Constants.nV, Constants.nK, Constants.nL, Constants.lCoef, matrixOffset);
                actual = dun.fitness;
                if (min > actual)
                {
                    min = actual;
                    aux = dun;
                }
                //Debug.Log("Got Fitness");
            }
            watch.Stop();
            long time = watch.ElapsedMilliseconds;

            hasFinished = true;
            Debug.Log(((Time.realtimeSinceStartup - startTime)*1000f)+"ms");

            Debug.Log("Dungeon has been generated");
            Debug.Log("Fitness = " + actual);
        }
    }
}
