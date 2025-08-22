using EvoGraph.GenAlg;

namespace EvoGraphTest.GraphPaintingTest;

public class FitnessFunctionPainting : IFitnessFunction
{
    public IAgent BestAgent { get; private set; }
    
    public int[,] Graph { get; set; }

    public FitnessFunctionPainting(int[,] matrix)
    {
        Graph = matrix;
        BestAgent = new AgentPainting(new int[matrix.GetLength(0)]);
        _neightbors = new List<int>[matrix.GetLength(0)];
        for (var i = 0; i < _neightbors.Length; ++i)
            _neightbors[i] = new List<int>();
        for (var i = 0; i < _neightbors.Length; ++i)
        for (var j = i + 1; j < _neightbors.Length; ++j)
        {
            if (Graph[i, j] != 1) continue;
            _neightbors[i].Add(j);
            _neightbors[j].Add(i);
        }
    }

    private List<int>[] _neightbors;
    
    public void CountFitness(SpeciesManager manager)
    {
        BestAgent = manager.SpeciesList[0].Members[0];
        foreach (var species in manager.SpeciesList)
        {
            species.MeanFitness = 0;
            foreach (var member in species.Members)
            {
                var agent = member as AgentPainting ?? throw new Exception("Not a agent");

                agent.Neightbors = _neightbors;
                var paints = agent.Chromosome.Distinct().Count();
                var conflicts = 0;
                for (var i = 0; i < agent.Chromosome.Length; i++)
                for (var j = i + 1; j < agent.Chromosome.Length; j++)
                {
                    if (Graph[i, j] != 1) continue;
                    conflicts += agent.Chromosome[i] == agent.Chromosome[j] ? 1 : 0;
                }
                agent.Fitness = paints + (agent.Chromosome.Length + 1) * conflicts;
                species.MeanFitness += agent.Fitness / species.Members.Count;
            }
            species.AdjustedFitness = 1.0 / species.MeanFitness;
            species.Members.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));
            if (species.Members[0].Fitness < BestAgent.Fitness) BestAgent = species.Members[0];
        }
        manager.SpeciesList.Sort((x, y) => x.MeanFitness.CompareTo(y.MeanFitness));
    }
}