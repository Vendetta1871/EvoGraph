namespace EvoGraph.GenAlg;

public interface IOffspringStrategy
{
    public List<IAgent> GetOffspring(Species species, int count) 
    {
        if (count == 0) return [];
        if (count == 1) return [species.Members[0].Clone()];
        
        // Elite
        List<IAgent> offspring = [species.Members[0].Clone()];
        // Crossover
        for (var i = 0; i < count - 1; i++)
        {
            var i0 = ParentIndex();
            var i1 = Random.Shared.Next(species.Members.Count);
            offspring.Add(species.Members[i0].Crossover(species.Members[i1]));
        }
        // Mutate
        for (var i = 1; i < count; i++) offspring[i] = offspring[i].Mutation();
        return offspring;
        
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
