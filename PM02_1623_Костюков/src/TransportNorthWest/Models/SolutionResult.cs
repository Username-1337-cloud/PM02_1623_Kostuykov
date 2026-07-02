namespace TransportNorthWest.Models;

public sealed class SolutionResult
{
    public SolutionResult(int[,] allocations, int totalCost, IReadOnlyList<string> steps)
    {
        Allocations = allocations;
        TotalCost = totalCost;
        Steps = steps;
    }

    public int[,] Allocations { get; }

    public int TotalCost { get; }

    public IReadOnlyList<string> Steps { get; }
}
