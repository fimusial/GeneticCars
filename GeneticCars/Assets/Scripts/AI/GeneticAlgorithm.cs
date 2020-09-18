using System;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;

namespace GeneticCars.Assets.Scripts.AI
{
    public class GeneticAlgorithmParameters
    {
        public int PopulationCount { get; set; }
        public int MaxGenerationCount { get; set; }
        public IContinuousDistribution NeuralNetworkWeightInitializationDistribution { get; set; }
        public int MinProbeCount { get; set; }
        public int MaxProbeCount { get; set; }
        public int MinHiddenLayerNeuronCount { get; set; }
        public int MaxHiddenLayerNeuronCount { get; set; }
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
        public int Generation { get; set; }

        public GeneticAlgorithm(GeneticAlgorithmParameters parameters)
        {
            AlgorithmParameters = parameters;
        }

        public void Initialize()
        {
            Population = new List<NeuralNetwork>();

            for (int i = 0; i < AlgorithmParameters.PopulationCount; i++)
            {
                var inputCountDistribution = new DiscreteUniform(1 + AlgorithmParameters.MinProbeCount, 1 + AlgorithmParameters.MaxProbeCount);
                var hiddenNeuronCountDistribution = new DiscreteUniform(AlgorithmParameters.MinHiddenLayerNeuronCount, AlgorithmParameters.MaxHiddenLayerNeuronCount);

                var networkParameters = new NeuralNetworkParameters()
                {
                    InputCount = inputCountDistribution.Sample(),
                    HiddenLayerNeuronCount = hiddenNeuronCountDistribution.Sample(),
                    OutputCount = 2,
                    HiddenLayerActivationFunction = AlgorithmParameters.NeuralNetworkHiddenLayerActivationFunction,
                    OutputLayerActivationFunction = AlgorithmParameters.NeuralNetworkOutputLayerActivationFunction
                };

                Population.Add(new NeuralNetwork(networkParameters, AlgorithmParameters.NeuralNetworkWeightInitializationDistribution));
            }

            Generation = 0;
        }

        public void NextGeneration(IList<GeneticAlgorithmAgent> agents)
        {
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
    }
}