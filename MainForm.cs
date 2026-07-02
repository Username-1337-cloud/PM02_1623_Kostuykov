using TransportNorthWest.Models;

namespace TransportNorthWest;

public sealed class MainForm : Form
{
    private readonly TextBox _suppliesTextBox = new();
    private readonly TextBox _demandsTextBox = new();
    private readonly TextBox _costsTextBox = new();
    private readonly TextBox _resultTextBox = new();

    private TransportationProblem? _currentProblem;
    private SolutionResult? _currentResult;

    public MainForm()
    {
        Text = "Транспортная задача: метод северо-западного угла";
        MinimumSize = new Size(980, 680);
        StartPosition = FormStartPosition.CenterScreen;

        BuildInterface();
        FillExample();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(16),
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));

        var inputPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 8,
            ColumnCount = 1,
        };

        inputPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inputPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        inputPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inputPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        inputPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inputPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        inputPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        inputPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

        ConfigureSingleLineInput(_suppliesTextBox);
        ConfigureSingleLineInput(_demandsTextBox);
        ConfigureMultilineInput(_costsTextBox);
        ConfigureMultilineInput(_resultTextBox);
        _resultTextBox.ReadOnly = true;

        inputPanel.Controls.Add(CreateLabel("Запасы поставщиков"), 0, 0);
        inputPanel.Controls.Add(_suppliesTextBox, 0, 1);
        inputPanel.Controls.Add(CreateLabel("Потребности потребителей"), 0, 2);
        inputPanel.Controls.Add(_demandsTextBox, 0, 3);
        inputPanel.Controls.Add(CreateLabel("Матрица тарифов"), 0, 4);
        inputPanel.Controls.Add(_costsTextBox, 0, 5);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
        };

        buttonPanel.Controls.Add(CreateButton("Загрузить CSV", LoadFromFile));
        buttonPanel.Controls.Add(CreateButton("Решить", Solve));
        buttonPanel.Controls.Add(CreateButton("Сохранить ответ", SaveResult));
        buttonPanel.Controls.Add(CreateButton("Очистить", ClearFields));

        inputPanel.Controls.Add(buttonPanel, 0, 6);

        var hint = CreateLabel("Формат ввода: числа через пробел, запятую или точку с запятой. Каждая строка тарифов соответствует одному поставщику.");
        hint.ForeColor = Color.DimGray;
        inputPanel.Controls.Add(hint, 0, 7);

        var resultPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
        };
        resultPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        resultPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        resultPanel.Controls.Add(CreateLabel("Результат"), 0, 0);
        resultPanel.Controls.Add(_resultTextBox, 0, 1);

        root.Controls.Add(inputPanel, 0, 0);
        root.Controls.Add(resultPanel, 1, 0);
        Controls.Add(root);
    }

    private static Label CreateLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Margin = new Padding(0, 8, 0, 6),
        };
    }

    private static Button CreateButton(string text, Action handler)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            Height = 36,
            Margin = new Padding(0, 10, 10, 0),
            Padding = new Padding(12, 4, 12, 4),
        };

        button.Click += (_, _) => handler();
        return button;
    }

    private static void ConfigureSingleLineInput(TextBox textBox)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.Font = new Font("Consolas", 11);
    }

    private static void ConfigureMultilineInput(TextBox textBox)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.Multiline = true;
        textBox.ScrollBars = ScrollBars.Both;
        textBox.Font = new Font("Consolas", 10);
        textBox.WordWrap = false;
    }

    private void FillExample()
    {
        _suppliesTextBox.Text = "20 30 30";
        _demandsTextBox.Text = "15 25 40";
        _costsTextBox.Text = "8 6 10" + Environment.NewLine +
                             "9 7 4" + Environment.NewLine +
                             "3 4 2";
    }

    private void LoadFromFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*",
            Title = "Выберите файл с исходными данными",
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            _currentProblem = InputParser.FromCsvFile(dialog.FileName);
            ShowProblemInFields(_currentProblem);
            _resultTextBox.Text = "Исходные данные успешно загружены.";
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private void Solve()
    {
        try
        {
            _currentProblem = InputParser.FromManualInput(
                _suppliesTextBox.Text,
                _demandsTextBox.Text,
                _costsTextBox.Text);
            _currentResult = TransportationSolver.SolveByNorthWestCorner(_currentProblem);
            _resultTextBox.Text = TransportationSolver.FormatSolution(_currentProblem, _currentResult);
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private void SaveResult()
    {
        if (_currentProblem is null || _currentResult is null)
        {
            ShowError("Сначала выполните решение задачи.");
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "Text files (*.txt)|*.txt",
            FileName = "transport_solution.txt",
            Title = "Сохранить результат",
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        File.WriteAllText(dialog.FileName, TransportationSolver.FormatSolution(_currentProblem, _currentResult));
        MessageBox.Show(this, "Результат сохранен.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ClearFields()
    {
        _suppliesTextBox.Clear();
        _demandsTextBox.Clear();
        _costsTextBox.Clear();
        _resultTextBox.Clear();
        _currentProblem = null;
        _currentResult = null;
    }

    private void ShowProblemInFields(TransportationProblem problem)
    {
        _suppliesTextBox.Text = string.Join(" ", problem.Supplies);
        _demandsTextBox.Text = string.Join(" ", problem.Demands);

        var rows = new List<string>();
        for (var row = 0; row < problem.SupplierCount; row++)
        {
            var values = new List<int>();
            for (var col = 0; col < problem.ConsumerCount; col++)
            {
                values.Add(problem.Costs[row, col]);
            }

            rows.Add(string.Join(" ", values));
        }

        _costsTextBox.Text = string.Join(Environment.NewLine, rows);
    }

    private void ShowError(string message)
    {
        MessageBox.Show(this, message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
