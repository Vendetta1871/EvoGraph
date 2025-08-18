using EvoGraph.GenAlg;

namespace EvoGraphTest.SimpleFunctionsTest;

public class GenAlgExampleTest
{
    private class OffspringStrategyExample : IOffspringStrategy;
    
    static IEnumerable<Function2D> GetFunctions()
    {
        yield return (x, y) => (x - 4) * (x - 4) + (y + 3) * (y + 3);
        yield return (x, y) => (1 - x) * (1 - x) + 100 * (y - x * x) * (y - x * x);
        yield return (x, y) => (x * x + y - 11) * (x * x + y - 11) + (x + y * y - 7) * (x + y * y - 7);
    }
    
    [Theory]
    public void FunctionMinimizingTest([ValueSource(nameof(GetFunctions))] Function2D func)
    {
        var agentsCount = 500;
        var maxIterations = 100;
        var threshold = 1e-5;
        var maxAttempts = 3;

        for (var i = 0; i < maxAttempts; ++i)
        {
            var agents = GetAgents(agentsCount, 1000);
            //var settings = new SpeciesSettings(agentsCount);
            var strategy = new OffspringStrategyExample();
            var manager = SpeciesManager.Get(strategy, agents);
            var fitness = new FitnessFunctionExample(func);
            var genAlg = new GeneticAlgorithm(manager, fitness);
            
            var best = double.MaxValue;
            for (var _ = 0; _ < maxIterations; _++)
            {
                var res = genAlg.Step().BestFitness;
                //if (res > best) Assert.Fail("The result was spoiled");
                if (Math.Abs(best = res) < threshold) break;
            }

            if (best >= threshold) continue;
            Assert.Pass($"Success from {i + 1} attempt");
            return;
        }

        Assert.Fail($"Failed all of {maxAttempts} attempts");
    }

    private List<IAgent> GetAgents(int count, double abs)
    {
        var agents = new List<IAgent>(count);
        for (var i = 0; i < count; i++)
        {
            var x = Random.Shared.NextDouble();
            var y = Random.Shared.NextDouble();
            x = abs * (2 * x - 1);
            y = abs * (2 * y - 1);
            var str = x.ToBinaryString() + y.ToBinaryString();
            agents.Add(new AgentExample(str));
        }
        return agents;
    }
}