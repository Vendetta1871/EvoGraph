namespace EvoGraph.GenAlg;

public class EpochResult
{
    public int EpochNumber;

    public double BestFitness;
    
    public IAgent BestAgent;

    public EpochResult(int epochNumber, IAgent bestAgent)
    {
        EpochNumber = epochNumber;
        BestFitness = bestAgent.Fitness;
        BestAgent = bestAgent;
    }
}
