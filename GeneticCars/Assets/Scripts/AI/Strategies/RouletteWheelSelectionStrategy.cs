using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;

namespace GeneticCars.Assets.Scripts.AI.Strategies
{
    public class RouletteWheelSelectionStrategy : ISelectionStrategy
    {
        public IList<NeuralNetwork> ApplySelection(IList<GeneticAlgorithmAgent> agents)
        {
            int populationCount = agents.Count;
            float fitnessSum = agents.Select(agent => agent.Fitness).Sum();
            var probabilities = new double[populationCount];

            for (int i = 0; i < populationCount; i++)
            {
                probabilities[i] = (double)agents[i].Fitness / (double)fitnessSum;
            }

            var distribution = new Categorical(probabilities);
            var newPopulation = new List<NeuralNetwork>();
            for (int i = 0; i < populationCount; i++)
            {
                newPopulation.Add(agents[distribution.Sample()].Network.Clone());
            }

            return newPopulation;
        }
    }
}