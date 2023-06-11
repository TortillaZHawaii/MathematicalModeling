using Google.OrTools.LinearSolver;

namespace MathematicalModeling;

public static class P3
{
    private static string[] _decisions =
    {
        "Utrzymać poziom produkcji",
        "Nieco zwiększyć poziom produkcji",
        "Znacznie zwiększyć produkcję",
        "Zmienić profil produkcji"
    };

    private static string[] _economy =
    {
        "Silny wzrost",
        "Umiarkowany wzrost",
        "Umiarkowana recesja",
        "Silna recesja"
    };

    private static int[,] _game =
    {
        { 33, 22, 21, 4 },
        { 42, 25,  5, 2 },
        { 63, 16,  3,-22},
        {  6,  8, 22, 22}
    };
    
    public static void Solve()
    {
        Console.WriteLine("Solving P3");
        Console.WriteLine("Game:");
        for (int i = 0; i < 4; ++i)
        {
            Console.WriteLine(string.Join(", ", _game[i, 0], _game[i, 1], _game[i, 2], _game[i, 3]));
        }
        
        Laplace();
        Console.WriteLine();
        Waldergrave();
        Console.WriteLine();
        Savage();
        Console.WriteLine();
        Hurwicz();
        Console.WriteLine();
        RandomizeEconomyStates();
        Console.WriteLine();
        SmallestLoss();
        Console.WriteLine();
        MixedStrategy();
    }

    private static int Laplace()
    {
        int n = 4;
        // 25/25/25/25 split
        var expected = new double[n];

        for (int y = 0; y < n; ++y)
        {
            for (int x = 0; x < n; ++x)
            {
                expected[y] += _game[y, x];
            }
            expected[y] /= n;
        }
        
        Console.WriteLine("Laplace criterion, 25/25/25/25 split");
        Console.WriteLine("Expected values:");
        Console.WriteLine(string.Join(", ", expected));
        
        var max = expected.Max();
        var maxIndex = expected.ToList().IndexOf(max);
        
        Console.WriteLine($"Max: {max}, index: {maxIndex}");
        Console.WriteLine($"Decision: {_decisions[maxIndex]}");
        
        return maxIndex;
    }

    private static int Waldergrave()
    {
        int n = 4;
        var mins = new double[n];
        
        for (int y = 0; y < n; ++y)
        {
            var min = double.MaxValue;
            for (int x = 0; x < n; ++x)
            {
                if (_game[y, x] < min)
                {
                    min = _game[y, x];
                }
            }
            mins[y] = min;
        }
        
        Console.WriteLine("Waldergrave criterion");
        Console.WriteLine("Minimum values:");
        Console.WriteLine(string.Join(", ", mins));
        
        var max = mins.Max();
        var maxIndex = mins.ToList().IndexOf(max);
        
        Console.WriteLine($"Max: {max}, index: {maxIndex}");
        Console.WriteLine($"Decision: {_decisions[maxIndex]}");
        
        return maxIndex;
    }
    
    private static int Savage()
    {
        int n = 4;
        var regretMatrix = new int[n, n];

        var columnMaxes = new int[n];
        
        for (int x = 0; x < n; ++x)
        {
            var max = int.MinValue;
            for (int y = 0; y < n; ++y)
            {
                if (_game[y, x] > max)
                {
                    max = _game[y, x];
                }
            }
            columnMaxes[x] = max;
        }
        
        for (int y = 0; y < n; ++y)
        {
            for (int x = 0; x < n; ++x)
            {
                regretMatrix[y, x] = columnMaxes[x] - _game[y, x];
            }
        }
        
        // Find max in each row
        var rowMaxes = new int[n];
        for (int y = 0; y < n; ++y)
        {
            rowMaxes[y] = int.MinValue;
            for (int x = 0; x < n; ++x)
            {
                rowMaxes[y] = Math.Max(rowMaxes[y], regretMatrix[y, x]);
            }
        }
        
        Console.WriteLine("Savage criterion");
        Console.WriteLine("Regret matrix:");
        for (int y = 0; y < n; ++y)
        {
            Console.WriteLine(string.Join(", ", regretMatrix.GetRow(y)));
        }
        
        Console.WriteLine("Row maxes:");
        Console.WriteLine(string.Join(", ", rowMaxes));
        
        var min = rowMaxes.Min();
        var minIndex = rowMaxes.ToList().IndexOf(min);
        
        Console.WriteLine($"Min: {min}, index: {minIndex}");
        Console.WriteLine($"Decision: {_decisions[minIndex]}");
        
        return minIndex;
    }
    
    private static int Hurwicz()
    {
        int n = 4;

        var rowMaxes = new double[n];
        var rowMins = new double[n];
        
        
        for (int y = 0; y < n; ++y)
        {
            var max = double.MinValue;
            var min = double.MaxValue;
            for (int x = 0; x < n; ++x)
            {
                if (_game[y, x] > max)
                {
                    max = _game[y, x];
                }
                if (_game[y, x] < min)
                {
                    min = _game[y, x];
                }
            }
            rowMaxes[y] = max;
            rowMins[y] = min;
        }
        
        double optimismCoefficient = 0.96;
        var hurwiczRows = new double[n];
        for (int i = 0; i < n; ++i)
        {
            hurwiczRows[i] = (1 - optimismCoefficient) * rowMaxes[i] + optimismCoefficient * rowMins[i];
        }
        
        Console.WriteLine("Hurwicz criterion");
        Console.WriteLine($"Optimism coefficient: {optimismCoefficient}");
        Console.WriteLine("Hurwicz rows:");
        Console.WriteLine(string.Join(", ", hurwiczRows));
        
        var maxi = hurwiczRows.Max();
        var maxIndex = hurwiczRows.ToList().IndexOf(maxi);
        
        Console.WriteLine($"Max: {maxi}, index: {maxIndex}");
        Console.WriteLine($"Decision: {_decisions[maxIndex]}");
        
        return maxIndex;
    }

    private static void RandomizeEconomyStates()
    {
        int seed = 12345;
        
        var random = new Random(seed);
        
        int n = 4;
        var profit = new int[n];
        int simulationCount = 100;
        
        for (int i = 0; i < simulationCount; ++i)
        {
            var economyState = random.GetRandomEconomyState();
            for (int j = 0; j < n; ++j)
            {
                profit[j] += _game[j, economyState];
            }
        }
        
        Console.WriteLine("Profit for each strategy after 100 randomizations");
        Console.WriteLine("Split: 15 35 40 10");
        for(int i = 0; i < n; ++i)
        {
            Console.WriteLine($"{_decisions[i]}: {profit[i]}");
        }
        Console.WriteLine("Average");
        for(int i = 0; i < n; ++i)
        {
            Console.WriteLine($"{_decisions[i]}: {profit[i] / (double)simulationCount}");
        }
    }

    private static int GetRandomEconomyState(this Random random)
    {
        // 15 35 40 10
        var r = random.Next(0, 100);
        return r switch
        {
            < 15 => 0,
            < 50 => 1,
            < 90 => 2,
            _ => 3
        };
    }

    private static void SmallestLoss()
    {
        // to determine saddle point
        int n = 4;
        
        var rowMins = new int[n];
        var columnMaxes = new int[n];
        
        for (int y = 0; y < n; ++y)
        {
            var min = int.MaxValue;
            for (int x = 0; x < n; ++x)
            {
                if (_game[y, x] < min)
                {
                    min = _game[y, x];
                }
            }
            rowMins[y] = min;
        }
        
        for (int x = 0; x < n; ++x)
        {
            var max = int.MinValue;
            for (int y = 0; y < n; ++y)
            {
                if (_game[y, x] > max)
                {
                    max = _game[y, x];
                }
            }
            columnMaxes[x] = max;
        }
        
        Console.WriteLine("Column maxes:");
        Console.WriteLine(string.Join(", ", columnMaxes));
        
        Console.WriteLine("Row mins:");
        Console.WriteLine(string.Join(", ", rowMins));

        var minimax = columnMaxes.Min();
        var minimaxIndex = columnMaxes.ToList().IndexOf(minimax);
        
        var maximin = rowMins.Max();
        var maximinIndex = rowMins.ToList().IndexOf(maximin);
        
        Console.WriteLine($"Minimax: {minimax}, index: {minimaxIndex}");
        Console.WriteLine($"Maximin: {maximin}, index: {maximinIndex}");

        Console.WriteLine(minimax == maximin ? "Saddle point found" : "Saddle point not found");
    }

    public static void MixedStrategy()
    {
        var solver = Solver.CreateSolver("SCIP");
        
        if (solver == null)
        {
            Console.WriteLine("SCIP solver not found.");
            return;
        }
        
        int n = 4;
        
        var p1 = solver.MakeNumVar(0, 1, "p1");
        var p2 = solver.MakeNumVar(0, 1, "p2");
        var p3 = solver.MakeNumVar(0, 1, "p3");
        var p4 = solver.MakeNumVar(0, 1, "p4");
        
        solver.Add(p1 + p2 + p3 + p4 == 1);
        
        var constraint = solver.MakeNumVar(Double.NegativeInfinity, Double.PositiveInfinity, $"q");
        for (int i = 0; i < n; ++i)
        {
            solver.Add(constraint == p1 * _game[i, 0] + p2 * _game[i, 1] + p3 * _game[i, 2] + p4 * _game[i, 3]);
        }
        
        var resultStatus = solver.Solve();
        
        if (resultStatus != Solver.ResultStatus.OPTIMAL)
        {
            Console.WriteLine("The problem does not have an optimal solution!");
            return;
        }
        
        Console.WriteLine("Solution:");
        Console.WriteLine($"p1 = {p1.SolutionValue()}");
        Console.WriteLine($"p2 = {p2.SolutionValue()}");
        Console.WriteLine($"p3 = {p3.SolutionValue()}");
        Console.WriteLine($"p4 = {p4.SolutionValue()}");
        
        Console.WriteLine($"Value of the game: {1 / (p1.SolutionValue() + p2.SolutionValue() + p3.SolutionValue() + p4.SolutionValue())}");
    }
    
    public static T[] GetColumn<T>(this T[,] matrix, int columnNumber)
    {
        return Enumerable.Range(0, matrix.GetLength(0))
            .Select(x => matrix[x, columnNumber])
            .ToArray();
    }

    private static T[] GetRow<T>(this T[,] matrix, int rowNumber)
    {
        return Enumerable.Range(0, matrix.GetLength(1))
            .Select(x => matrix[rowNumber, x])
            .ToArray();
    }
}