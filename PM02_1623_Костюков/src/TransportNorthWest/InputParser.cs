using System.Globalization;
using TransportNorthWest.Models;

namespace TransportNorthWest;

public static class InputParser
{
    public static TransportationProblem FromManualInput(string suppliesText, string demandsText, string costsText)
    {
        var supplies = ParseVector(suppliesText, "запасы");
        var demands = ParseVector(demandsText, "потребности");
        var costs = ParseMatrix(costsText, supplies.Length, demands.Length);

        return new TransportationProblem(supplies, demands, costs);
    }

    public static TransportationProblem FromCsvFile(string path)
    {
        var lines = File.ReadAllLines(path)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        if (lines.Length < 2)
        {
            throw new ArgumentException("Файл должен содержать строку потребностей и хотя бы одну строку поставщика.");
        }

        var demands = ParseCsvNumbers(lines[0]);
        var supplies = new int[lines.Length - 1];
        var costs = new int[lines.Length - 1, demands.Length];

        for (var row = 1; row < lines.Length; row++)
        {
            var values = ParseCsvNumbers(lines[row]);
            if (values.Length != demands.Length + 1)
            {
                throw new ArgumentException($"Строка {row + 1}: ожидается запас поставщика и {demands.Length} тарифов.");
            }

            supplies[row - 1] = values[0];
            for (var col = 0; col < demands.Length; col++)
            {
                costs[row - 1, col] = values[col + 1];
            }
        }

        return new TransportationProblem(supplies, demands, costs);
    }

    private static int[] ParseVector(string text, string title)
    {
        var values = ParseCsvNumbers(text);
        if (values.Length == 0)
        {
            throw new ArgumentException($"Поле \"{title}\" не заполнено.");
        }

        return values;
    }

    private static int[,] ParseMatrix(string text, int expectedRows, int expectedColumns)
    {
        var rows = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (rows.Length != expectedRows)
        {
            throw new ArgumentException($"Матрица тарифов должна содержать {expectedRows} строк.");
        }

        var matrix = new int[expectedRows, expectedColumns];

        for (var row = 0; row < expectedRows; row++)
        {
            var values = ParseCsvNumbers(rows[row]);
            if (values.Length != expectedColumns)
            {
                throw new ArgumentException($"Строка тарифов {row + 1} должна содержать {expectedColumns} значений.");
            }

            for (var col = 0; col < expectedColumns; col++)
            {
                matrix[row, col] = values[col];
            }
        }

        return matrix;
    }

    private static int[] ParseCsvNumbers(string text)
    {
        return text.Split(new[] { ';', ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(ParseInt)
            .ToArray();
    }

    private static int ParseInt(string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
        {
            throw new ArgumentException($"Значение \"{value}\" не является целым числом.");
        }

        return number;
    }
}
