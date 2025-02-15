using System;
using Game.LevelGenerator.EvolutionaryAlgorithm;

namespace Game.LevelGenerator
{
    /// This struct holds the most relevant data of the evolutionary process.
    [Serializable]
    public struct Data
    {
        public Parameters parameters { get; set; }
        public double duration { get; set; }
    }
}