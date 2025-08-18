namespace EvoGraph.GenAlg;

public interface IFitnessFunction
{
    public IAgent BestAgent { get; }
    
    public void CountFitness(SpeciesManager manager);
}
