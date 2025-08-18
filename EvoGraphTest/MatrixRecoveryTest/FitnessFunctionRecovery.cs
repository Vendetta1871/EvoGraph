using EvoGraph.GenAlg;

namespace EvoGraphTest.MatrixRecoveryTest;

public class FitnessFunctionRecovery: IFitnessFunction
{
    public IAgent BestAgent { get; private set; }
    
    public int[,] MatrixO { get; set; }

    public FitnessFunctionRecovery(int[,] matrix)
    {
        MatrixO = matrix;
        BestAgent = new AgentRecovery(new int[matrix.GetLength(0)]);
    }
    
    public void CountFitness(SpeciesManager manager)
    {
        BestAgent = manager.SpeciesList[0].Members[0];
        foreach (var species in manager.SpeciesList)
        {
            species.MeanFitness = 0;
            foreach (var member in species.Members)
            {
                var agent = member as AgentRecovery ?? throw new Exception("Not a agent");
                
                var fit = 0.0;
                for (var i = 0; i < agent.Chromosome.Length; i++)
                for (var j = 0; j < agent.Chromosome.Length; j++)
                {
                    if (MatrixO[i, j] < 0) continue;
                    var m = agent.Chromosome[i] == agent.Chromosome[j] ? 1 : 0;
                    fit += Math.Abs(MatrixO[i, j] - m);
                }
                agent.Fitness = fit / agent.Chromosome.Length / agent.Chromosome.Length;
                species.MeanFitness += agent.Fitness / species.Members.Count;
            }
            species.AdjustedFitness = 1.0 / species.MeanFitness;
            species.Members.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));
            if (species.Members[0].Fitness < BestAgent.Fitness) BestAgent = species.Members[0];
        }
        manager.SpeciesList.Sort((x, y) => x.MeanFitness.CompareTo(y.MeanFitness));
    }
}