namespace StatisticalProcessControl.Tests;

public class Rule1Tests
{
    private const double Mean = 10;
    private const double StdDev = 1;
    private const double NumStdDev = 3;

    private readonly Rules _rules = new();

    [Fact]
    public void FlagsPointsBeyondTheLimitOnBothSidesOfTheMean()
    {
        double[] data = { 10, 14, 10, 6, 10 };

        var failedPoints = _rules.Rule1(data, numPoints: 1, NumStdDev, Mean, stdDev: StdDev);

        Assert.Equal(new[] { (1, 14.0), (3, 6.0) }, failedPoints);
    }

    [Fact]
    public void IgnoresPointsWithinTheLimit()
    {
        double[] data = { 10, 12.9, 7.1, 13, 7 };

        var failedPoints = _rules.Rule1(data, numPoints: 1, NumStdDev, Mean, stdDev: StdDev);

        Assert.Empty(failedPoints);
    }

    [Fact]
    public void ReportsEveryNthPointBeyondTheLimit()
    {
        double[] data = { 14, 10, 6, 10, 15, 5 };

        var failedPoints = _rules.Rule1(data, numPoints: 2, NumStdDev, Mean, stdDev: StdDev);

        Assert.Equal(new[] { (2, 6.0), (5, 5.0) }, failedPoints);
    }
}
