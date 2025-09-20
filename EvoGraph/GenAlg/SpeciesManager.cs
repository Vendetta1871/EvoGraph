namespace EvoGraph.GenAlg;

public class SpeciesManager
{
    public int PopulationSize  { get; set; }
    
    public List<Species> SpeciesList { get; set; }
    
    public IOffspringStrategy OffspringStrategy { get; set; }

    protected SpeciesManager(IOffspringStrategy strategy, int population)
    {
        SpeciesList = [];
        OffspringStrategy = strategy;
        PopulationSize = population;
    }
    
    private int _minSpecies;
    private int _maxSpecies;
    private double _eps;
    private double _factor;
    private int _minSamples;
    
    public static SpeciesManager Get(IOffspringStrategy strategy, List<IAgent> agents,
        int minSpecies = 0, int maxSpecies = 1, double eps = 0.0, double factor = 0.0, int minSamples = 2)
    {
        var manager = new SpeciesManager(strategy, agents.Count)
        {
            _minSpecies = minSpecies,
            _maxSpecies = maxSpecies,
            _eps = eps,
            _factor = factor,
            _minSamples = minSamples
        };
        manager.AddAgents(agents);
        return manager;
    }
    
    private Dictionary<(IAgent, IAgent), double> _distanceCache = new();

    private double GetDistance(IAgent a, IAgent b)
    {
        var key = (a, b);
        if (_distanceCache.TryGetValue(key, out var d)) return d;

        d = a.Distance(b);
        _distanceCache[key] = d;
        _distanceCache[(b, a)] = d;
        return d;
    }

    protected virtual void AddAgents(List<IAgent> agents)
    {
        if (_maxSpecies <= 1)
        {
            var species = new Species(agents[0]);
            for (var i = 1; i < agents.Count; i++) species.Members.Add(agents[i]);
            SpeciesList = [species];
            return;
        }
        
        // DBSCAN
        
        var visited = new HashSet<IAgent>();
        var clustered = new HashSet<IAgent>();
        var noise = new List<IAgent>();

        foreach (var agent in agents)
        {
            if (visited.Contains(agent)) continue;

            visited.Add(agent);
            var neighbors = GetNeighbors(agent, agents);

            if (neighbors.Count < _minSamples)
            {
                noise.Add(agent);
                continue;
            }

            // A new cluster -> new species
            var species = new Species(agent);
            SpeciesList.Add(species);
            clustered.Add(agent);

            ExpandCluster(species, neighbors, agents, visited, clustered);
        }
        
        // A cluster for all the noise
        if (noise.Count <= 0) return;
        var noiseSpecies = new Species(noise[0]);
        noiseSpecies.Members.AddRange(noise.Skip(1));
        SpeciesList.Add(noiseSpecies);
    }

    private void ExpandCluster(
        Species species,
        List<IAgent> neighbors,
        List<IAgent> allAgents,
        HashSet<IAgent> visited,
        HashSet<IAgent> clustered)
    {
        var queue = new Queue<IAgent>(neighbors);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (!visited.Contains(current))
            {
                visited.Add(current);
                var currentNeighbors = GetNeighbors(current, allAgents);

                if (currentNeighbors.Count >= _minSamples)
                    foreach (var n in currentNeighbors)
                        if (!queue.Contains(n)) queue.Enqueue(n);
            }

            if (clustered.Contains(current)) continue;
            species.Members.Add(current);
            clustered.Add(current);
        }
    }

    private List<IAgent> GetNeighbors(IAgent agent, List<IAgent> agents)
    {
        var neighbors = new List<IAgent>();
        foreach (var other in agents)
            if (!ReferenceEquals(agent, other) && agent.Distance(other) <= _eps) 
                neighbors.Add(other);
        return neighbors;
    }

    protected virtual void SpeciesCooling()
    {
        if (SpeciesList.Count < _minSpecies) _eps += _factor;
        if (SpeciesList.Count > _maxSpecies) _eps -= _factor;
        foreach (var species in SpeciesList) species.Members.Clear();
    }

    public virtual void GenerateOffspring()
    {
        List<IAgent> offspring = [];
        var sum = SpeciesList.Sum(s => s.AdjustedFitness);
        foreach (var species in SpeciesList)
        {
            var count = species.AdjustedFitness / sum * PopulationSize;
            var agents = species.GetOffspring(OffspringStrategy, (int)count);
            offspring.AddRange(agents);
        }
        SpeciesCooling();
        AddAgents(offspring);
        SpeciesList.RemoveAll(s => s.Members.Count == 0);
    }
}
