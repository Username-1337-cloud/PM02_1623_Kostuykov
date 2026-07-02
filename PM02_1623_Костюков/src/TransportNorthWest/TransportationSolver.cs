using System.Text;
using TransportNorthWest.Models;

namespace TransportNorthWest;

public static class TransportationSolver
{
    public static SolutionResult SolveByNorthWestCorner(TransportationProblem problem)
    {
        Validate(problem);

        var supplies = (int[])problem.Supplies.Clone();
        var demands = (int[])problem.Demands.Clone();
        var allocations = new int[problem.SupplierCount, problem.ConsumerCount];
        var steps = new List<string>();

        var i = 0;
        var j = 0;

        while (i < supplies.Length && j < demands.Length)
        {
            var value = Math.Min(supplies[i], demands[j]);
            allocations[i, j] = value;
            steps.Add($"A{i + 1} -> B{j + 1}: отправляем {value}; тариф {problem.Costs[i, j]}");

            supplies[i] -= value;
            demands[j] -= value;

            if (supplies[i] == 0 && demands[j] == 0)
            {
                i++;
                j++;
            }
            else if (supplies[i] == 0)
            {
                i++;
            }
            else
            {
                j++;
            }
        }

        var totalCost = 0;
        for (var row = 0; row < problem.SupplierCount; row++)
        {
            for (var col = 0; col < problem.ConsumerCount; col++)
            {
                totalCost += allocations[row, col] * problem.Costs[row, col];
            }
        }

        return new SolutionResult(allocations, totalCost, steps);
    }

    public static string FormatSolution(TransportationProblem problem, SolutionResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Решение транспортной задачи методом северо-западного угла");
        builder.AppendLine();
        builder.AppendLine("Матрица поставок:");
        builder.Append("       ");

        for (var col = 0; col < problem.ConsumerCount; col++)
        {
            builder.Append($"B{col + 1,6}");
        }

        builder.AppendLine();

        for (var row = 0; row < problem.SupplierCount; row++)
        {
            builder.Append($"A{row + 1,3}   ");
            for (var col = 0; col < problem.ConsumerCount; col++)
            {
                builder.Append($"{result.Allocations[row, col],6}");
            }

            builder.AppendLine();
        }

        builder.AppendLine();
        builder.AppendLine("Ход решения:");
        foreach (var step in result.Steps)
        {
            builder.AppendLine("- " + step);
        }

        builder.AppendLine();
        builder.AppendLine($"Итоговая стоимость перевозок: {result.TotalCost}");
        return builder.ToString();
    }

    private static void Validate(TransportationProblem problem)
    {
        if (problem.Supplies.Length == 0 || problem.Demands.Length == 0)
        {
            throw new ArgumentException("Количество поставщиков и потребителей должно быть больше нуля.");
        }

        if (problem.Costs.GetLength(0) != problem.SupplierCount ||
            problem.Costs.GetLength(1) != problem.ConsumerCount)
        {
            throw new ArgumentException("Размер матрицы тарифов не соответствует количеству поставщиков и потребителей.");
        }

        if (problem.Supplies.Any(value => value < 0) || problem.Demands.Any(value => value < 0))
        {
            throw new ArgumentException("Запасы и потребности не могут быть отрицательными.");
        }

        foreach (var cost in problem.Costs)
        {
            if (cost < 0)
            {
                throw new ArgumentException("Тарифы перевозки не могут быть отрицательными.");
            }
        }

        var totalSupply = problem.Supplies.Sum();
        var totalDemand = problem.Demands.Sum();

        if (totalSupply != totalDemand)
        {
            throw new ArgumentException($"Задача должна быть закрытой: сумма запасов {totalSupply}, сумма потребностей {totalDemand}.");
        }
    }
}
