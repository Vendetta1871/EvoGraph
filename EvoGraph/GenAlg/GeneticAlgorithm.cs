using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace EvoGraph.GenAlg;

public class GeneticAlgorithm
{
    private ILogger _logger; 
     
    public SpeciesManager Manager { get; set; }
    
    public IFitnessFunction FitnessFunction { get; set; }

    public GeneticAlgorithm(SpeciesManager manager, IFitnessFunction fitnessFunction)
    {
        _logger = LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<GeneticAlgorithm>();
        Manager = manager;
        FitnessFunction = fitnessFunction;
        FitnessFunction.CountFitness(Manager); // initialization
        _logger.LogInformation
        (
            "Epoch: {ResultEpochNumber} finished. Best fitness: {ResultBestFitness}",
            0,
            FitnessFunction.BestAgent.Fitness
        );
    }

    private int _epoch;

    public EpochResult Step()
    {
        Manager.GenerateOffspring();
        FitnessFunction.CountFitness(Manager);
        var result = new EpochResult(_epoch += 1, FitnessFunction.BestAgent);
        
        _logger.LogInformation
        (
            "Epoch: {ResultEpochNumber} finished. Best fitness: {ResultBestFitness}.",
            result.EpochNumber,
            result.BestFitness
        );
        // _logger.LogDebug("Species count: {Species}", Manager.SpeciesList.Count);
        return result;
    }
}
