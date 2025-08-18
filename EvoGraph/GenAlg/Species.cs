namespace EvoGraph.GenAlg;

public class Species
{
    public double MeanFitness { get; set; }
    
    public double AdjustedFitness { get; set; }
    
    public List<IAgent> Members { get; set; }

    public Species(IAgent agent)
    {
        Members = [agent];
    }

    public List<IAgent> GetOffspring(IOffspringStrategy strategy, int count)
    {
        return strategy.GetOffspring(this, count);
    }
}
