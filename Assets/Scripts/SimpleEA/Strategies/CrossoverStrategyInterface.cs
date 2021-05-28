namespace SimpleEA
{
    public enum CrossoverStrategies
    {
        BLXAlpha
    }
    public interface CrossoverStrategyInterface
    {
        public Individual[] Crossover(Individual[] parents, double crossoverRate);
    }
}