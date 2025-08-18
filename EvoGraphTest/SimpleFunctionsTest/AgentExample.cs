using EvoGraph.GenAlg;

namespace EvoGraphTest.SimpleFunctionsTest;

public class AgentExample : IAgent
{
    public string Chromosome { get; set; }
    
    public double Fitness { get; set; }

    public AgentExample(string chromosome)
    {
        Chromosome = chromosome;
        Fitness = double.MaxValue;
    }

    private static Exception WrongArgumentType(IAgent agent)
    {
        return new Exception($"Expected agent type is AgentExample, received {agent.GetType().FullName}");
    }
    
    public double Distance(IAgent agent)
    {
        var other = agent as AgentExample ?? throw WrongArgumentType(agent);
                    
        var x0 = Chromosome[..64].BinaryToDouble();
        var y0 = Chromosome[64..].BinaryToDouble();
        var x1 = other.Chromosome[..64].BinaryToDouble();
        var y1 = other.Chromosome[64..].BinaryToDouble();

        var r0 = Math.Sqrt(x0 * x0 + y0 * y0);
        var r1 = Math.Sqrt(x1 * x1 + y1 * y1);
        var dr = Math.Sqrt((x0 - x1) * (x0 - x1) + (y0 - y1) * (y0 - y1));

        return dr;
        //return 2 * dr / (r0 + r1);
    }

    public IAgent Crossover(IAgent agent)
    {
        const double fitterGeneProb = 0.67;
        const double ratio0 = 0.15;
        const double ratio1 = 0.3;
        
        var fitter = this;
        var other = agent as AgentExample ?? throw WrongArgumentType(agent);
        if (Fitness > other.Fitness) (fitter, other) = (other, fitter);
        return Random.Shared.NextDouble() switch
        {
            <= ratio0 => new AgentExample(OnePointCrossover()),
             <= ratio1 => new AgentExample(RandomCrossover()),
            _ => new AgentExample(ArithmeticCrossover())
        };
            

        string OnePointCrossover()
        {
            var chromosome = "";
            if (Random.Shared.NextDouble() < fitterGeneProb) 
                chromosome += fitter.Chromosome[..64];
            else chromosome += other.Chromosome[..64];
            if (Random.Shared.NextDouble() < fitterGeneProb) 
                chromosome += fitter.Chromosome[64..];
            else chromosome += other.Chromosome[64..];
            return chromosome;
        }

        string RandomCrossover()
        {
            var chromosome = "";
            for (var i = 0; i < 128; i++)
            {
                if (Random.Shared.NextDouble() < fitterGeneProb)
                    chromosome += fitter.Chromosome[i];
                else chromosome += other.Chromosome[i];
            }

            return chromosome;
        }

        string ArithmeticCrossover()
        {
            var x0 = fitter.Chromosome[..64].BinaryToDouble();
            var y0 = fitter.Chromosome[64..].BinaryToDouble();
            var x1 = other.Chromosome[..64].BinaryToDouble();
            var y1 = other.Chromosome[64..].BinaryToDouble();
            var alpha = Random.Shared.NextDouble();
            var childX = x0 * alpha + x1 * (1 - alpha);
            var childY = y0 * alpha + y1 * (1 - alpha);
            return childX.ToBinaryString() + childY.ToBinaryString();
        }
    }

    public IAgent Mutation()
    {
        const double ratio = 0.2;
        
        var chars = Chromosome.ToCharArray();
        return Random.Shared.NextDouble() switch
        {
            <=ratio => new AgentExample(InvertBit()),
            _ => new AgentExample(Noise())
        };
        
        string InvertBit()
        {
            var index = Random.Shared.Next(chars.Length);
            if (chars[index] == '1') chars[index] = '0';
            else if (chars[index] == '0') chars[index] = '1';
            else throw new Exception($"Invalid character in chromosome at index {index}");
            return new string(chars);
        }

        double Gaussian()
        {
            var x = Random.Shared.NextDouble();
            for (var i = 0; i < 11; ++i) 
                x += Random.Shared.NextDouble();
            return (x - 6) / 6;
        }

        string Noise()
        {
            var x = Chromosome[..64].BinaryToDouble();
            var y = Chromosome[64..].BinaryToDouble();
            x *= 1 + Gaussian();
            y *= 1 + Gaussian();
            return x.ToBinaryString() + y.ToBinaryString();
        }
    }

    public IAgent Clone()
    {
        var str = Chromosome.Clone() as string;
        return new AgentExample(str ?? throw new Exception("Cannot clone agent"))
        {
            Fitness = Fitness,
        };
    }
}