using EvoGraph.GenAlg;

namespace EvoGraphTest.MatrixRecoveryTest;

public class AgentRecovery : IAgent
{
    public int[] Chromosome { get; set; }
    
    public double Fitness { get; set; }
    
    public AgentRecovery(int[] chromosome)
    {
        Chromosome = chromosome;
        Fitness = double.MaxValue;
        CompactLabels();
    }
    
    private void CompactLabels()
    {
        var max = Chromosome.Max();
        
        var map = new int[max + 1];
        for (var i = 0; i <= max; i++) map[i] = -1;
        
        var nextLabel = 0;
        foreach (var gene in Chromosome)
            if (map[gene] == -1) map[gene] = nextLabel++;
        
        for (var i = 0; i < Chromosome.Length; i++)
            Chromosome[i] = map[Chromosome[i]];
    }

    private static Exception WrongArgumentType(IAgent agent)
    {
        return new Exception($"Expected agent type is AgentRecovery, received {agent.GetType().FullName}");
    }
    
    public double Distance(IAgent agent)
    {
        var other = agent as AgentRecovery ?? throw WrongArgumentType(agent);
        var clusters = Chromosome.Distinct().Count();
        var otherClusters = other.Chromosome.Distinct().Count();
        return Math.Abs(clusters - otherClusters);
    }

    public IAgent Crossover(IAgent agent)
    {
        const double fitterGeneProb = 0.67;
        const double ratio0 = 0.3;
        const double ratio1 = 0.9;
        
        var fitter = this;
        var other = agent as AgentRecovery ?? throw WrongArgumentType(agent);
        if (Fitness > other.Fitness) (fitter, other) = (other, fitter);

        var dna = new int[Chromosome.Length];
        return Random.Shared.NextDouble() switch
        {
            <= ratio0 => new AgentRecovery(RandomMix()),
            <= ratio1 => new AgentRecovery(OnePointCrossover()),
            _ => new AgentRecovery(TwoPointsCrossover())
        };

        int[] RandomMix()
        {
            for (var i = 0; i < dna.Length; ++i)
            {
                if (Random.Shared.NextDouble() < fitterGeneProb)
                    dna[i] = fitter.Chromosome[i];
                else dna[i] = other.Chromosome[i];
            }

            return dna;
        }

        int[] OnePointCrossover()
        {
            var index = Random.Shared.Next(1, dna.Length - 1);
            var (a, b) = (fitter.Chromosome, other.Chromosome);
            if (Random.Shared.NextDouble() < 0.5) (a, b) = (b, a);
            for (var i = 0; i < index; ++i) dna[i] = a[i];
            for (var i = index; i < dna.Length; ++i) dna[i] = b[i];
            return dna;
        }
        
        int[] TwoPointsCrossover()
        {
            var index0 = Random.Shared.Next(1, dna.Length - 2);
            var index1 = Random.Shared.Next(2, dna.Length - 1);
            if (index0 > index1) (index0, index1) = (index1, index0);
            var (a, b) = (fitter.Chromosome, other.Chromosome);
            if (Random.Shared.NextDouble() < 0.5) (a, b) = (b, a);
            for (var i = 0; i < index0; ++i) dna[i] = a[i];
            for (var i = index0; i < index1; ++i) dna[i] = b[i];
            for (var i = index1; i < dna.Length; ++i) dna[i] = a[i];
            return dna;
        }
    }

    public IAgent Mutation()
    {
        const double mutateGeneProb = 0.05;
        const double ratio = 0.3;
        
        var dna = new int[Chromosome.Length];
        return Random.Shared.NextDouble() switch
        {
            <= ratio => new AgentRecovery(ChangeCluster()),
            _ => new AgentRecovery(RandomChanges())
        };
        
        int[] RandomChanges()
        {
            Array.Copy(Chromosome, dna, Chromosome.Length);
            
            var strength = Random.Shared.Next(dna.Length);
            for (var i = 0; i < dna.Length; ++i) 
                strength = Math.Min(strength, Random.Shared.Next(dna.Length));

            for (var i = 0; i < strength; ++i)
            {
                var max = dna.Max();
                var index = Random.Shared.Next(dna.Length);
                dna[index] = Random.Shared.Next(max + 2);
                CompactLabels();
            }
            
            return dna;
        }

        int[] ChangeCluster()
        {
            var to = Random.Shared.Next(dna.Length);
            var from = dna[Random.Shared.Next(dna.Length)];
            var changeClusterProb = Random.Shared.NextDouble();
            for (var i = 0; i < dna.Length; ++i)
            {
                if (Chromosome[i] == from)
                {
                    if (Random.Shared.NextDouble() < changeClusterProb) dna[i] = to;
                }
                else dna[i] = Chromosome[i];
            }

            return dna;
        }
    }

    public IAgent Clone()
    {
        var dna = Chromosome.Clone() as int[];
        return new AgentRecovery(dna ?? throw new Exception("Cannot clone agent"))
        {
            Fitness = Fitness,
        };
    }
}