namespace TransportNorthWest.Models;

public sealed class TransportationProblem
{
    public TransportationProblem(int[] supplies, int[] demands, int[,] costs)
    {
        Supplies = supplies;
        Demands = demands;
        Costs = costs;
    }

    public int[] Supplies { get; }

    public int[] Demands { get; }

    public int[,] Costs { get; }

    public int SupplierCount => Supplies.Length;

    public int ConsumerCount => Demands.Length;
}
