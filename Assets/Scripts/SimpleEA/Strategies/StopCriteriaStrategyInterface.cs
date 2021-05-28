namespace SimpleEA
{
    public enum StopCriteriaStrategies
    {
        GenerationLimit
    }

    public interface StopCriteriaStrategyInterface
    {
        public void UpdateStopCriteria();
        public bool HasReachedStopCriteria();
    }
}