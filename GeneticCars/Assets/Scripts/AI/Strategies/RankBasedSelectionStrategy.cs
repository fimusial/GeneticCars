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
            float maxRank = populationCount - 1f;

            float[] partial = Enumerable
                .Range(0, populationCount)
                .Select(i => 1f - (i / maxRank))
                .ToArray();

            float normalizeFactor = partial.Sum();
            double[] probabilities = partial.Select(x => (double)normalizeFactor * x).ToArray();

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