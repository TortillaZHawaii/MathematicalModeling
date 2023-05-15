using Google.OrTools.LinearSolver;
using static System.Double;

namespace MathematicalModeling;

public static class P2
{
    public static void Solve()
    {
        var solver = Solver.CreateSolver("SCIP");
        
        if (solver is null)
        {
            Console.WriteLine("Could not create solver");
            return;
        }

        var s1 = solver.MakeNumVar(0, 12_000, "s1");
        var s2 = solver.MakeNumVar(0, 9_000, "s2");
        
        // s1 ranges costs
        var s1RangesUpperLimits = new[] { 3620, 8055, 12_000 };
        var s1RangesCosts = new[] { 8, 9, 12 };
        var s1r1 = solver.MakeNumVar(0, s1RangesUpperLimits[0], "s1r1");
        var s1r2 = solver.MakeNumVar(0, s1RangesUpperLimits[1] - s1RangesUpperLimits[0], "s1r2");
        var s1r3 = solver.MakeNumVar(0, s1RangesUpperLimits[2] - s1RangesUpperLimits[1], "s1r3");
        
        var s1c1 = solver.MakeIntVar(0, PositiveInfinity, "s1c1");
        var s1c2 = solver.MakeIntVar(0, PositiveInfinity, "s1c2");
        var s1c3 = solver.MakeIntVar(0, PositiveInfinity, "s1c3");
        var s1c = solver.MakeIntVar(0, PositiveInfinity, "s1c");

        solver.Add(s1c == s1c1 + s1c2 + s1c3);
        solver.Add(s1c1 == s1RangesCosts[0] * s1r1);
        solver.Add(s1c2 == s1RangesCosts[1] * s1r2);
        solver.Add(s1c3 == s1RangesCosts[2] * s1r3);

        solver.Add(s1 == s1r1 + s1r2 + s1r3);
        
        // s2 ranges costs
        var s2RangesUpperLimits = new[] { 2_396, 5_924, 9_000 };
        var s2RangesCosts = new[] { 14, 10, 7 };
        // make sure to fill previous range before filling next one
        var s2r1y = solver.MakeBoolVar("s2r1y");
        var s2r2y = solver.MakeBoolVar("s2r2y");
        var s2r3y = solver.MakeBoolVar("s2r3y");
        
        // pick a range
        solver.Add(s2 <= s2RangesUpperLimits[0] * s2r1y + s2RangesUpperLimits[1] * s2r2y + s2RangesUpperLimits[2] * s2r3y);
        solver.Add(s2 >= 0 * s2r1y + s2RangesUpperLimits[0] * s2r2y + s2RangesUpperLimits[1] * s2r3y);
        solver.Add(s2r1y + s2r2y + s2r3y == 1);
        
        // calculate a cost based on a range
        var s2c1 = solver.MakeIntVar(NegativeInfinity, PositiveInfinity, "s2c1");
        var s2c2 = solver.MakeIntVar(NegativeInfinity, PositiveInfinity, "s2c2");
        var s2c3 = solver.MakeIntVar(NegativeInfinity, PositiveInfinity, "s2c3");
        var s2c = solver.MakeIntVar(0, PositiveInfinity, "s2c");

        int bigM = 100_000;
        var s2c1max = s2RangesCosts[0] * s2RangesUpperLimits[0];
        var s2c2max = s2c1max + s2RangesCosts[1] * (s2RangesUpperLimits[1] - s2RangesUpperLimits[0]);

        solver.Add(s2c1 ==
                    s2RangesCosts[0] * s2
                    - bigM * (1 - s2r1y));
        solver.Add(s2c2 ==
                   s2c1max +
                   s2RangesCosts[1] * (s2 - s2RangesUpperLimits[0])
                   - bigM * (1 - s2r2y));
        solver.Add(s2c3 == 
                    s2c2max +
                    s2RangesCosts[2] * (s2 - s2RangesUpperLimits[1])
                    - bigM * (1 - s2r3y));

        // pick min cost, which is the cost of the range we picked
        solver.Add(s2c >= s2c1);
        solver.Add(s2c >= s2c2);
        solver.Add(s2c >= s2c3);

        var s2d = solver.MakeNumVar(0, PositiveInfinity, "s2d");
        var s2w2 = solver.MakeNumVar(0, PositiveInfinity, "s2w2");
        
        // costs
        // train
        var trainCartCount = solver.MakeIntVar(0, PositiveInfinity, "trainCartCount");
        var trainCartCost = solver.MakeIntVar(0, PositiveInfinity, "trainCartCost");
        var locomotiveCount = solver.MakeIntVar(0, PositiveInfinity, "locomotiveCount");
        var locomotiveCost = solver.MakeIntVar(0, PositiveInfinity, "locomotiveCost");
        
        int trainCartCapacity = 15;
        int locomotiveCapacity = 10;
        // ceil(s1 / 15)
        solver.Add(trainCartCount >= s1 / trainCartCapacity);
        // ceil
        solver.Add(locomotiveCount >= trainCartCount / locomotiveCapacity);
        
        int perCartCost = 1_290;
        int perLocomotiveCost = 4_470;
        solver.Add(trainCartCost == trainCartCount * perCartCost);
        solver.Add(locomotiveCost == locomotiveCount * perLocomotiveCost);
        
        // trucks
        var truckCount = solver.MakeIntVar(0, PositiveInfinity, "truckCount");
        var truckCost = solver.MakeIntVar(0, PositiveInfinity, "truckCost");
        
        int truckCapacity = 25;
        int perTruckCost = 1_500;
        solver.Add(truckCount >= s2 / truckCapacity);
        solver.Add(truckCost == truckCount * perTruckCost);

        var d1 = solver.MakeNumVar(0, PositiveInfinity, "d1");
        var d2 = solver.MakeNumVar(0, PositiveInfinity, "d2");
        
        var s1d1 = solver.MakeNumVar(0, PositiveInfinity, "s1d1");
        var s1d2 = solver.MakeNumVar(0, PositiveInfinity, "s1d2");
        var s2d1 = solver.MakeNumVar(0, PositiveInfinity, "s2d1");
        var s2d2 = solver.MakeNumVar(0, PositiveInfinity, "s2d2");

        solver.Add(d2 == s1d2 + s2d2);
        solver.Add(d1 == s1d1 + s2d1);
        
        solver.Add(s2w2 + s2d == s2);
        
        solver.Add(s1d1 == 0.6 * s1 );
        solver.Add( s1d2 == 0.4 * s1);
        solver.Add( s2d1 == 0.9 * s2d);
        solver.Add( s2d2 == 0.1 * s2d);
        
        // Surowce są poddawane obróbce w przygotowalni o całkowitej dziennej przepustowości 16800 ton. 
        solver.Add(d1 + d2 <= 16_800);
        
        //Koszt pracy przygotowalni zależy od liczby zatrudnionych pracowników,
        //przy czym wymogi bezpieczeństwa wymagają by było zatrudnionych
        //przy- najmniej 4 pracowników na każde 250 ton całkowitej ilości przerabianych surowców.
        //Dzienny koszt pracy jednego pracow- nika to 200 zł.
        var quadWorkers = solver.MakeIntVar(0, PositiveInfinity, "quadWorkers");
        var workersCount = solver.MakeIntVar(0, PositiveInfinity, "workersCount");
        int perWorkerCost = 200;
        // ceil
        solver.Add(quadWorkers >= (d1 + d2) / 250);
        solver.Add(workersCount == 4 * quadWorkers);
        var workersCost = solver.MakeIntVar(0, PositiveInfinity, "workersCost");
        solver.Add(workersCost == workersCount * perWorkerCost);

        //Zakład może pracować tylko w zakresie 2250-6750 ton przerobu surowca dziennie.
        // W tym zakresie koszt przetworzenia jednej tony wynosi 43 zł.
        // Zakład może też nie pracować w ogóle, nie generując żadnych kosztów.
        // "Czy zaklad cieplny pracuje"
        var s2y = solver.MakeBoolVar("s2y");
        
        // make sure in range or zero
        solver.Add(2250 * s2y <= s2w2);
        solver.Add(s2w2 <= 6750 * s2y);
        // cost per tonne
        var s2cw = solver.MakeIntVar(0, PositiveInfinity, "s2cw");
        solver.Add(s2cw == 43 * s2w2);
        
        // // Cena sprzedaży wyrobu W1 wynosi: 581 zł/tonę, wyrobu W2: 491 zł/tonę.
        var w1 = solver.MakeNumVar(0, PositiveInfinity, "w1");
        var w2 = solver.MakeNumVar(0, PositiveInfinity, "w2");
        
        solver.Add(w1 == d1);
        
        solver.Add(w2 == d2 + s2w2);
        
        var w1c = solver.MakeNumVar(0, PositiveInfinity, "w1c");
        var w2c = solver.MakeNumVar(0, PositiveInfinity, "w2c");
        
        solver.Add(w1c == 581 * w1);
        solver.Add(w2c == 491 * w2);
        
        // Zawarte umowy wymagają dostarczenia co naj- mniej 5250 ton każdego produktu.
        solver.Add(w1 >= 5250);
        solver.Add(w2 >= 5250);

        solver.Maximize(w1c + w2c - s1c - s2cw - s2c - truckCost - trainCartCost - locomotiveCost - workersCost);
        
        var resultStatus = solver.Solve();
        
        if (resultStatus != Solver.ResultStatus.OPTIMAL)
        {
            Console.WriteLine("The problem does not have an optimal solution!");
            return;
        }
        
        Console.WriteLine("Status:");
        Console.WriteLine(resultStatus);
        
        Console.WriteLine("Solution:");
        Console.WriteLine($"Objective value = {solver.Objective().Value()}");
        Console.WriteLine("Variables:");
        foreach (var variable in solver.variables())
        {
            Console.WriteLine($"{variable.Name(),15} = {variable.SolutionValue()}");
        }
    }
}