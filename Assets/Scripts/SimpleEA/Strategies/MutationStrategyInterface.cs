namespace SimpleEA
{
    public enum MutationStrategies
    {
        Random
    }
    public interface MutationStrategyInterface
    {
        public void Mutate(Individual individual, double mutationRate);
    }
}