using EvoGraph.GenAlg;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Targets;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace EvoGraphTest.MatrixRecoveryTest;

public class GenAlgRecoveryTest
{
    private class OffspringStrategyMatrix : IOffspringStrategy;
    
    private static readonly ILogger Logger = 
        LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<GeneticAlgorithm>();
    
    private static IEnumerable<(int[,], int[,], int)> GetRecoveryData()
    {
        const int matrixN = 20;
        const int samples = 100;
        DataRecovery.Init(samples, matrixN, 0.10, 0.10, 0.05);
        for (var i = 0; i < samples; i++)
            yield return (DataRecovery.Data[i], DataRecovery.Label[i], DataRecovery.Clusters[i]);
    }
    
    private static IEnumerable<(int[,], int[,], int)> GetRecoveryDataFile()
    {
        var samples = 3;
        DataRecovery.AddFromFile("./data/Node1.txt", out var size1, 0.5);
        DataRecovery.AddFromFile("./data/Node2.txt", out var size2, 0.5);
        DataRecovery.AddFromFile("./data/Node3.txt", out var size3, 0.5);
        for (var i = 0; i < samples; i++)
            yield return (DataRecovery.Data[i], DataRecovery.Label[i], DataRecovery.Clusters[i]);
    }

    [Theory]
    public static void MatrixRecoveryTest(
        [ValueSource(nameof(GetRecoveryDataFile))] (int[,] Data, int[,] Label, int Clusters) data)
    {
        var agentsCount = 500;
        var maxIterations = 400;
        //var threshold = 1e-5;
        
        var matrixN = data.Data.GetLength(0);
        var agents = GetAgents(agentsCount, matrixN);
        var strategy = new OffspringStrategyMatrix();
        var manager = SpeciesManager.Get(strategy, agents);
        var fitness = new FitnessFunctionRecovery(data.Data);
        var genAlg = new GeneticAlgorithm(manager, fitness);

        var clusters = 1;
        var bestFitness = double.MaxValue;
        Logger.LogInformation("Clusters: {clusters}, F1 score: {f1}",
            clusters, F1Score(data.Label, new int[matrixN]));
        for (var _ = 0; _ < maxIterations; _++)
        {
            var epochResult = genAlg.Step();
            var dna = (epochResult.BestAgent as AgentRecovery)?.Chromosome;
            if (dna == null) throw new Exception("dna is null");
            var f1 = F1Score(data.Label, dna);
            //if (epochResult.EpochNumber % 50 == 0 || _ == 0)
                //Console.WriteLine($"Step: {epochResult.EpochNumber}, F1 score = {f1}");
            clusters = dna.Distinct().Count();
            Logger.LogInformation("Clusters: {clusters}, F1 score: {f1}", clusters, f1);
            bestFitness = Math.Min(bestFitness, epochResult.BestFitness);
        }
        
        var fit = 0.0;
        for (var a = 0; a < matrixN; a++)
        for (var b = 0; b < matrixN; b++)
        {
            if (data.Data[a, b] < 0) continue;
            var m = data.Label[a, b];
            fit += Math.Abs(data.Data[a, b] - m);
        }
        fit = fit / matrixN / matrixN;
        Logger.LogInformation("Best GA fitness: {bestFitness}, fitness of M matrix: {fit}",
            bestFitness, fit);
        Logger.LogInformation("Predicted clusters: {clusters}, M matrix clusters: {data.Clusters}",
            clusters, data.Clusters);
        return;

        double F1Score(int[,] label, int[] result)
        {
            double tp = 0, tn = 0, fp = 0, fn = 0;
            for (var i = 0; i < result.Length; i++)
            for (var j = 0; j < result.Length; j++)
            {
                var expected = label[i, j];
                var actual = result[i] == result[j] ? 1 : 0;
                if (actual == 0 && expected == 0) tn += 1;
                if (actual == 0 && expected == 1) fn += 1;
                if (actual == 1 && expected == 0) fp += 1;
                if (actual == 1 && expected == 1) tp += 1;
            }

            var precision = tp / (tp + fp);
            var recall = tp / (tp + fn);

            return 2.0 * precision * recall / (precision + recall);
        }
    }
    
    private static List<IAgent> GetAgents(int count, int size)
    {
        var agents = new List<IAgent>(count);
        for (var i = 0; i < count; i++) agents.Add(new AgentRecovery(new int[size]));
        return agents;
    }
}