using EvoGraph.GenAlg;

namespace EvoGraphTest.GraphPaintingTest;

public class GenAlgPaintingTest
{
    private class OffspringStrategyPainting : IOffspringStrategy;
    
    private static IEnumerable<(int[,], int)> GetData()
    {
        const int matrixN = 20;
        const int samples = 10;
        const bool greedy = true;
        DataPainting.Init(samples, matrixN, greedy);
        for (var i = 0; i < samples; i++)
            yield return (DataPainting.Data[i], DataPainting.Label[i]);
    }

    [Theory]
    public static void GraphPaintingTest([ValueSource(nameof(GetData))] (int[,] Data, int Label) data)
    {
        var agentsCount = 500;
        var maxIterations = 300;
        //var threshold = 1e-5;
        
        var matrixN = data.Data.GetLength(0);
        var agents = GetAgents(agentsCount, matrixN);
        var strategy = new OffspringStrategyPainting();
        var manager = SpeciesManager.Get(strategy, agents);
        var fitness = new FitnessFunctionPainting(data.Data);
        var genAlg = new GeneticAlgorithm(manager, fitness);

        var paints = 0;
        for (var _ = 0; _ < maxIterations; _++)
        {
            var epochResult = genAlg.Step();
            var dna = (epochResult.BestAgent as AgentPainting)?.Chromosome;
            if (dna == null) throw new Exception("dna is null");
            paints = dna.Distinct().Count();
            if (epochResult.EpochNumber % 50 == 0 || _ == 0)
                Console.WriteLine(
                    $"Step: {epochResult.EpochNumber}, fitness = {epochResult.BestFitness}, paints = {paints}");
        }

        Console.WriteLine($"GenAlg: {paints}, greedy: {data.Label}");
    }
    
    private static List<IAgent> GetAgents(int count, int size)
    {
        var agents = new List<IAgent>(count);
        for (var i = 0; i < count; i++) agents.Add(new AgentPainting(new int[size]));
        return agents;
    }
}