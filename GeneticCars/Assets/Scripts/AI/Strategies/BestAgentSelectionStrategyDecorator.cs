using System.Collections.Generic;
using System.Linq;

namespace GeneticCars.Assets.Scripts.AI.Strategies
{
    public class BestAgentSelectionStrategyDecorator : ISelectionStrategy
    {
        private readonly ISelectionStrategy _baseStrategy;
        private GeneticAlgorithmAgent _bestAgent;

        public BestAgentSelectionStrategyDecorator(ISelectionStrategy baseStrategy)
        {
            _baseStrategy = baseStrategy;
        }

        public IList<NeuralNetwork> ApplySelection(IList<GeneticAlgorithmAgent> agents)
        {
            GeneticAlgorithmAgent currentBestAgent = agents.OrderByDescending(agent => agent.Fitness).First();
            GeneticAlgorithmAgent currentWorstAgent = agents.OrderBy(agent => agent.Fitness).First();

            if (_bestAgent == null)
            {
                _bestAgent = currentBestAgent;
            }
            else if (currentBestAgent.Fitness >= _bestAgent.Fitness)
            {
                _bestAgent = currentBestAgent;
            }
            else
            {
                agents.Remove(currentWorstAgent);
                agents.Add(currentBestAgent);
            }

            return _baseStrategy.ApplySelection(agents);
        }
    }
}