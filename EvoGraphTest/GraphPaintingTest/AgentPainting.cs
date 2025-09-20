using EvoGraph.GenAlg;

namespace EvoGraphTest.GraphPaintingTest;

public class AgentPainting : IAgent
{
    public int[] Chromosome { get; set; }
    
    public double Fitness { get; set; }

    public List<int>[] Neightbors { get; set; }
    
    public AgentPainting(int[] chromosome)
    {
        Chromosome = chromosome;
        Neightbors = new List<int>[chromosome.Length];
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
        return new Exception($"Expected agent type is AgentPainting, received {agent.GetType().FullName}");
    }
    
    public double Distance(IAgent agent)
    {
        var other = agent as AgentPainting ?? throw WrongArgumentType(agent);
        var clusters = Chromosome.Distinct().Count();
        var otherClusters = other.Chromosome.Distinct().Count();
        return Math.Abs(clusters - otherClusters);
    }

    public IAgent Crossover(IAgent agent)
    {
        const double fitterGeneProb = 0.67;
        const double ratio = 0.3;
        
        var fitter = this;
        var other = agent as AgentPainting ?? throw WrongArgumentType(agent);
        if (Fitness > other.Fitness) (fitter, other) = (other, fitter);

        var dna = new int[Chromosome.Length];
        return Random.Shared.NextDouble() switch
        {
            <= ratio => new AgentPainting(RandomMix()),
            _ => new AgentPainting(OnePointCrossover())
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
            var index = Random.Shared.Next(dna.Length);
            var (a, b) = (fitter.Chromosome, other.Chromosome);
            if (Random.Shared.NextDouble() < 0.5) (a, b) = (b, a);
            for (var i = 0; i < index; ++i) dna[i] = a[i];
            for (var i = index; i < dna.Length; ++i) dna[i] = b[i];
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
            <= ratio => new AgentPainting(ChangeCluster()),
            _ => new AgentPainting(RandomChanges())
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
            var to = Random.Shared.Next(dna.Max() + 2);
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
        return new AgentPainting(dna ?? throw new Exception("Cannot clone agent"))
        {
            Fitness = Fitness,
        };
    }
}