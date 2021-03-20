using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.Distributions;

namespace GeneticCars.Assets.Scripts.AI
{
    public class GeneticAlgorithmParameters
    {
        public int PopulationCount { get; set; }
        public int MaxGenerationCount { get; set; }
        public IContinuousDistribution NeuralNetworkWeightInitializationDistribution { get; set; }

        public int MinSensorCount { get; set; }
        public int MaxSensorCount { get; set; }
        public int MinHiddenLayerNeuronCount { get; set; }
        public int MaxHiddenLayerNeuronCount { get; set; }
        public bool EvolveNetworkTopology { get; set; }

        public Func<float, float> NeuralNetworkHiddenLayerActivationFunction { get; set; }
        public Func<float, float> NeuralNetworkOutputLayerActivationFunction { get; set; }
        public ISelectionStrategy SelectionStrategy { get; set; }
        public ICrossoverStrategy CrossoverStrategy { get; set; }
        public IMutationStrategy MutationStrategy { get; set; }
    }

    public class GeneticAlgorithm
    {
        public GeneticAlgorithmParameters AlgorithmParameters { get; private set; }
        public IList<NeuralNetwork> Population { get; private set; }
        public int Generation { get; private set; }

        public GeneticAlgorithm(GeneticAlgorithmParameters parameters)
        {
            AlgorithmParameters = parameters;
            Population = new List<NeuralNetwork>();
            Generation = 0;
        }

        public void Initialize()
        {
            var inputCountDistribution = new DiscreteUniform(1 + AlgorithmParameters.MinSensorCount, 1 + AlgorithmParameters.MaxSensorCount);
            var hiddenNeuronCountDistribution = new DiscreteUniform(AlgorithmParameters.MinHiddenLayerNeuronCount, AlgorithmParameters.MaxHiddenLayerNeuronCount);

            for (int i = 0; i < AlgorithmParameters.PopulationCount; i++)
            {
                var networkParameters = new NeuralNetworkParameters()
                {
                    InputCount = AlgorithmParameters.EvolveNetworkTopology ? inputCountDistribution.Sample() : AlgorithmParameters.MinSensorCount + 1,
                    HiddenLayerNeuronCount = AlgorithmParameters.EvolveNetworkTopology ? hiddenNeuronCountDistribution.Sample() : AlgorithmParameters.MinHiddenLayerNeuronCount,
                    OutputCount = 2,
                    HiddenLayerActivationFunction = AlgorithmParameters.NeuralNetworkHiddenLayerActivationFunction,
                    OutputLayerActivationFunction = AlgorithmParameters.NeuralNetworkOutputLayerActivationFunction
                };

                Population.Add(new NeuralNetwork(networkParameters, AlgorithmParameters.NeuralNetworkWeightInitializationDistribution));
            }
        }

        public void NextGeneration(IList<GeneticAlgorithmAgent> agents)
        {
            GatherData(agents);

            IList<NeuralNetwork> reproducedPopulation = AlgorithmParameters.SelectionStrategy.ApplySelection(agents);
            IList<NeuralNetwork> crossoverPopulation = AlgorithmParameters.CrossoverStrategy.ApplyCrossover(reproducedPopulation);
            IList<NeuralNetwork> mutatedPopulation = AlgorithmParameters.MutationStrategy.ApplyMutations(crossoverPopulation);

            Population = mutatedPopulation;
            Generation++;
        }

        public bool IsGenerationLimitReached()
        {
            return Generation >= AlgorithmParameters.MaxGenerationCount;
        }

        public void ResetGenerationCounter()
        {
            Generation = 0;
        }

        private void GatherData(IList<GeneticAlgorithmAgent> agents)
        {
            DataCollector.AddDataPoint(DataCollector.SetsNames.MinFitness, agents.Min(agent => agent.Fitness));
            DataCollector.AddDataPoint(DataCollector.SetsNames.MaxFitness, agents.Max(agent => agent.Fitness));
            DataCollector.AddDataPoint(DataCollector.SetsNames.AvgFitness, agents.Average(agent => agent.Fitness));

            IDictionary<string, int> topologyTrends = new Dictionary<string, int>();
            foreach (var network in agents.Select(agent => agent.Network))
            {
                string topologyIdentifier = network.GetTopologyIdentifier();
                if (!topologyTrends.ContainsKey(topologyIdentifier))
                {
                    topologyTrends.Add(topologyIdentifier, 1);
                }
                else
                {
                    topologyTrends[topologyIdentifier]++;
                }
            }

            var topologyTrendsStringBuilder = new StringBuilder();
            foreach (var kvp in topologyTrends)
            {
                topologyTrendsStringBuilder.Append($"{kvp.Key}:{kvp.Value},");
            }

            DataCollector.AddDataPoint(DataCollector.SetsNames.TopologyTrends, topologyTrendsStringBuilder.ToString());
        }
    }
}