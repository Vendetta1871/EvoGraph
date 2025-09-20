using EvoGraph.GenAlg;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace EvoGraphTest.GraphPaintingTest;

public class GenAlgPaintingTest
{
    private static readonly ILogger Logger = 
        LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<GeneticAlgorithm>();
    
    private class OffspringStrategyPainting : IOffspringStrategy;
    
    private static IEnumerable<(int[,], int, int)> GetData()
    {
        const int matrixN = 20;
        const int samples = 100;
        DataPainting.Init(samples, matrixN, true);
        for (var i = 0; i < samples; i++)
            yield return (DataPainting.Data[i], DataPainting.LabelGreedy[i], DataPainting.LabelFull[i]);
    }

    [Theory]
    public static void GraphPaintingTest([ValueSource(nameof(GetData))] (int[,] Data, int LabelGreedy, int LabelFull) data)
    {
        var agentsCount = 500;
        var maxIterations = 300;
        //var threshold = 1e-5;
        
        var matrixN = data.Data.GetLength(0);
        var agents = GetAgents(agentsCount, matrixN);
        var strategy = new OffspringStrategyPainting();
        var manager = SpeciesManager.Get(strategy, agents/*, 2, 6, 1.5, 1, 5*/);
        var fitness = new FitnessFunctionPainting(data.Data);
        var genAlg = new GeneticAlgorithm(manager, fitness, false);

        var paints = 0;
        for (var _ = 0; _ < maxIterations; _++)
        {
            var epochResult = genAlg.Step(false);
            var dna = (epochResult.BestAgent as AgentPainting)?.Chromosome;
            if (dna == null) throw new Exception("dna is null");
            paints = dna.Distinct().Count();
            Logger.LogInformation("Step: {epochNumber}, fitness = {fitness}, paints = {paints}",
                epochResult.EpochNumber, epochResult.BestFitness, paints);
        }

        Logger.LogInformation("Result. GenAlg: {paints}, greedy: {greedy}, full: {full}",
            paints, data.LabelGreedy, data.LabelFull);
    }
    
    private static List<IAgent> GetAgents(int count, int size)
    {
        var agents = new List<IAgent>(count);
        for (var i = 0; i < count; i++) agents.Add(new AgentPainting(new int[size]));
        return agents;
    }
}