using System.Collections.Generic;
using UnityEngine;
namespace SimpleEA
{
    public class EAManager : MonoBehaviour
    {
        private EvolutionaryAlgorithm evolutionaryAlgorithm;
        [SerializeField]
        protected int populationSize, maxGenerations;
        [SerializeField]
        protected double crossoverRate, mutationRate;
        List<double> valueList;
        private void Awake()
        {
            evolutionaryAlgorithm = EvolutionaryAlgorithm.InitializeEAWithDefaultStrategies(
                populationSize, maxGenerations, crossoverRate, mutationRate
            );
            evolutionaryAlgorithm.Evolve();
            valueList = evolutionaryAlgorithm.bestFitnessPerGeneration;
        }
        public void Start()
        {
            Window_Graph.instance.ShowGraph(valueList, 1, -1, (int _i) => "Gen." + (_i + 1), (float _f) => "Fit." + Mathf.RoundToInt(_f));
        }
    }
}