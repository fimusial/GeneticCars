using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;

namespace GeneticCars.Assets.Scripts.AI.Strategies
{
    public class RankBasedSelectionStrategy : ISelectionStrategy
    {
        public IList<NeuralNetwork> ApplySelection(IList<GeneticAlgorithmAgent> agents)
        {
            IList<GeneticAlgorithmAgent> agentsSorted = agents
                .OrderBy(agent => agent.Fitness)
                .ToList();
            
            int populationCount = agentsSorted.Count;

            double[] probabilities = Enumerable
                .Range(0, populationCount)
                .Select(i => (double)(2 * i) / (populationCount * (populationCount - 1)))
                .ToArray();

            var distribution = new Categorical(probabilities);
            var newPopulation = new List<NeuralNetwork>();
            for (int i = 0; i < populationCount; i++)
            {
                newPopulation.Add(agentsSorted[distribution.Sample()].Network.Clone());
            }

            return newPopulation;
        }
    }
}