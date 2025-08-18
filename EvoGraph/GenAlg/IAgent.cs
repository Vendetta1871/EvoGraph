namespace EvoGraph.GenAlg;

public interface IAgent
{
    public double Fitness { get; set; }
    
    public double Distance(IAgent other);
    
    public IAgent Crossover(IAgent other);
    
    public IAgent Mutation();

    public IAgent Clone();
}
