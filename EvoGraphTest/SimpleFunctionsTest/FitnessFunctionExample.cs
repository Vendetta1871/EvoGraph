using EvoGraph.GenAlg;

namespace EvoGraphTest.SimpleFunctionsTest;

public delegate double Function2D(double x, double y);

public class FitnessFunctionExample : IFitnessFunction
{
    public IAgent BestAgent { get; private set; }

    public Function2D Function { get; }

    public FitnessFunctionExample(Function2D function)
    {
        Function = function;
        var str = double.NaN.ToBinaryString() + double.NaN.ToBinaryString();
        BestAgent = new AgentExample(str);
    }
    
    public void CountFitness(SpeciesManager manager)
    {
        BestAgent = manager.SpeciesList[0].Members[0];
        foreach (var species in manager.SpeciesList)
        {
            species.MeanFitness = 0;
            foreach (var member in species.Members)
            {
                var agent = member as AgentExample ?? throw new Exception("Not a agent");
                var x = agent.Chromosome[..64].BinaryToDouble();
                var y = agent.Chromosome[64..].BinaryToDouble();
                var fit = Function(x, y);
                agent.Fitness = double.IsNaN(fit) || double.IsInfinity(fit) ? double.MaxValue : fit;
                species.MeanFitness += agent.Fitness / species.Members.Count;
            }

            var diversityFactor = 1; // - Math.Log(1 + 1.0 / species.Members.Count);
            species.AdjustedFitness = 1.0 / species.MeanFitness * diversityFactor;
            species.Members.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));
            if (species.Members[0].Fitness < BestAgent.Fitness) BestAgent = species.Members[0];
        }
        manager.SpeciesList.Sort((x, y) => x.MeanFitness.CompareTo(y.MeanFitness));
    }
}