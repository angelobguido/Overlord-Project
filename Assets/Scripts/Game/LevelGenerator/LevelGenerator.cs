using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Events;
using Game.LevelGenerator.EvolutionaryAlgorithm;
using UnityEngine;
using Util;

namespace Game.LevelGenerator
{
    /// This class holds the evolutionary level generation algorithm.
    public class LevelGenerator
    {
        /// The number of parents to be selected for crossover.
        private static readonly int CROSSOVER_PARENTS = 2;
        /// The size of the intermediate population.
        private static readonly int INTERMEDIATE_POPULATION = 100;

        /// The evolutionary parameters.
        private Parameters prs;
        /// The found MAP-Elites population.
        private Population solution;
        /// The evolutionary process' collected data.
        private Data data;

        /// Return the found MAP-Elites population.
        public Population Solution { get => solution; }

        /// Return the collected data from the evolutionary process.
        public Data Data { get => data; }


        /// The event to handle the progress bar update.
        public static event NewEAGenerationEvent NewEaGenerationEventHandler;

        private FitnessPlot fitnessPlot;

        /// Level Generator constructor.
        public LevelGenerator(Parameters _prs, FitnessPlot fitnessPlot = null) {
            prs = _prs;
            data = new Data();
            data.parameters = prs;
            this.fitnessPlot = fitnessPlot;
        }

        /// Generate and return a set of levels.
        public async Task Evolve()
        {
            DateTime start = DateTime.Now;
            await Evolution();
            DateTime end = DateTime.Now;
            data.duration = (end - start).TotalSeconds;
        }

        /// Perform the level evolution process.
        private async Task Evolution()
        {
            // Initialize the MAP-Elites population
            var pop = InitializePopulation();
            await EvolvePopulation(pop);
            // Get the final population (solution)
            solution = pop;
        }

        private Population InitializePopulation()
        {
            Population pop = new Population(
                SearchSpace.ExplorationRanges.Length,
                SearchSpace.LeniencyRanges.Length, fitnessPlot
            );
            var maxTries = INTERMEDIATE_POPULATION;
            var currentTry = 0;
            // Generate the initial population
            while (pop.EliteList.Count < prs.Population && currentTry < maxTries)
            {
                var individual = Individual.CreateRandom(prs.FitnessParameters);
                individual.Fix();
                individual.CalculateFitness();
                pop.PlaceIndividual(individual);
                currentTry++;
            }

            return pop;
        }

        private async Task EvolvePopulation(Population pop)
        {
            Debug.Log("Start Evolution Process");
            // Evolve the population
            int g = 0;
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;
            while (!HasReachedStopCriteria(end, start, pop.GetAmountOfBiomesWithElites(), pop.GetAmountOfBiomesWithElitesBetterThan(prs.AcceptableFitness)))
            {
                var intermediate = CreateIntermediatePopulation(pop, g);
                // Place the offspring in the MAP-Elites population
                foreach (Individual individual in intermediate)
                {
                    pop.PlaceIndividual(individual);
                }
                g++;
                end = DateTime.Now;

                // Update the progress bar
                double seconds = (end - start).TotalSeconds;
                var progress = (float) seconds / prs.Time;
                NewEaGenerationEventHandler?.Invoke(this, new NewEAGenerationEventArgs(progress));
                pop.UpdateBiomes(g);
                await Task.Yield();
            }
            Debug.Log("Finish Evolution Process");
            //pop.SaveJson();
            NewEaGenerationEventHandler?.Invoke(this, new NewEAGenerationEventArgs(1.0f));
        }

        private List<Individual> CreateIntermediatePopulation(in Population pop, int g)
        {
            List<Individual> intermediate = new List<Individual>();
            while (intermediate.Count < INTERMEDIATE_POPULATION)
            {
                // Apply the crossover operation
                var parents = Selection.SelectParents(CROSSOVER_PARENTS, prs.Competitors, pop);
                var offspring = CreateOffspring(parents);

                // Place the offspring in the MAP-Elites population
                foreach (var individual in offspring)
                {
                    individual.Fix();
                    individual.CalculateFitness();
                    individual.generation = g;
                    intermediate.Add(individual);
                }
            }
            return intermediate;
        }

        private Individual[] CreateOffspring(Individual[] parents)
        {
            var offspring = Crossover.Apply(parents[0], parents[1]);
            // Apply the mutation operation with a random chance or
            // always that the crossover was not successful
            if (offspring.Length != 0 && prs.Mutation <= RandomSingleton.GetInstance().RandomPercent())
                return offspring;
            if (offspring.Length == CROSSOVER_PARENTS)
            {
                parents[0] = offspring[0];
                parents[1] = offspring[1];
            }
            else
            {
                offspring = new Individual[2];
            }

            offspring[0] = Mutation.Apply(parents[0]);
            offspring[1] = Mutation.Apply(parents[1]);

            return offspring;
        }

        private bool HasReachedStopCriteria(DateTime end, DateTime start, int biomesWithElites, float elitesWithAcceptableFitness)
        {
            if (biomesWithElites < prs.MinimumElite) return false;
            if (elitesWithAcceptableFitness >= prs.MinimumElite) return true;
            var elapsedTime = (end - start).TotalSeconds;
            return elapsedTime > prs.Time;
        }
    }
}