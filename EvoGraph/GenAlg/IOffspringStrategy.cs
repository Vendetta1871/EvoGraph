namespace EvoGraph.GenAlg;

public interface IOffspringStrategy
{
    public List<IAgent> GetOffspring(Species species, int count) 
    {
        if (count == 0) return [];
        if (count == 1) return [species.Members[0].Clone()];
        
        // Elite
        var offspring = new IAgent[count];
        offspring[0] = species.Members[0].Clone();
        // Crossover
        Parallel.For(1, count, i =>
        {
            var i0 = ParentIndex();
            var i1 = Random.Shared.Next(species.Members.Count);
            offspring[i] = species.Members[i0].Crossover(species.Members[i1]);
        });
        // Mutate
        Parallel.For(1, count, i =>
        {
            offspring[i] = offspring[i].Mutation();
        });
        return offspring.ToList();
        
        int ParentIndex()
        {
            var weights = species.Members.Select(m => 1 / m.Fitness).ToArray();
            var roll = Random.Shared.NextDouble() * weights.Sum();
            var cumulative = 0.0;
            for (var i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative) return i;
            }

            return 0;
        }
    }
}
