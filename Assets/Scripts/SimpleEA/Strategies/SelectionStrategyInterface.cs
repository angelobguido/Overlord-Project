namespace SimpleEA
{
    public enum SelectionStrategies
    {
        Tournament
    }
    public interface SelectionStrategyInterface
    {
        public Individual Selection(Population population);
    }
}