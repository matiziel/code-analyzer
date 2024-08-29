namespace MetricCalculator;

public interface IMetricCalculator<TMetric> {
    public Task<IEnumerable<TMetric>> Calculate(string solutionPath);
}