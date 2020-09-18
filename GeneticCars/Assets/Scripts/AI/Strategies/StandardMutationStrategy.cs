using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;

namespace GeneticCars.Assets.Scripts.AI.Strategies
{
    public class StandardMutationStrategyParameters
    {
        public int MinProbeCount { get; set; }
        public int MaxProbeCount { get; set; }
        public int MinHiddenLayerNeuronCount { get; set; }
        public int MaxHiddenLayerNeuronCount { get; set; }
        public float MutationRate { get; set; }
        public IContinuousDistribution NeuralNetworkWeightInitializationDistribution { get; set; }
        public IContinuousDistribution NeuralNetworkWeightMutationDistribution { get; set; }
    }

    public class StandardMutationStrategy : IMutationStrategy
    {
        private readonly StandardMutationStrategyParameters _parameters;
        private readonly IContinuousDistribution _mutationRateDistribution = new ContinuousUniform(0, 1);

        public StandardMutationStrategy(StandardMutationStrategyParameters parameters)
        {
            _parameters = parameters;
        }

        public IList<NeuralNetwork> ApplyMutations(IList<NeuralNetwork> population)
        {
            var mutatedPopulation = new List<NeuralNetwork>();

            var inputCountDistribution = new DiscreteUniform(1 + _parameters.MinProbeCount, 1 + _parameters.MaxProbeCount);
            var hiddenNeuronCountDistribution = new DiscreteUniform(_parameters.MinHiddenLayerNeuronCount, _parameters.MaxHiddenLayerNeuronCount);

            foreach (var agent in population)
            {
                Chromosome chromosome = agent.GetChromosome();

                if (ShouldMutate())
                {
                    int oldInputCount = chromosome.InputCount;
                    chromosome.InputCount = inputCountDistribution.Sample();
                    if (oldInputCount != chromosome.InputCount)
                    {
                        for (int i = 0; i < chromosome.HiddenLayerNeurons.Count; i++)
                        {
                            var newNeuronData = new float[chromosome.InputCount + 1];
                            CopyAndFill(chromosome.HiddenLayerNeurons[i], newNeuronData);
                            chromosome.HiddenLayerNeurons[i] = newNeuronData;
                        }
                    }
                }

                if (ShouldMutate())
                {
                    int oldHiddenLayerNeuronCount = chromosome.HiddenLayerNeuronCount;
                    chromosome.HiddenLayerNeuronCount = hiddenNeuronCountDistribution.Sample();

                    if (oldHiddenLayerNeuronCount != chromosome.HiddenLayerNeuronCount)
                    {
                        int diff = oldHiddenLayerNeuronCount - chromosome.HiddenLayerNeuronCount;
                        if (diff > 0)
                        {
                            for (int i = 0; i < diff; i++)
                            {
                                chromosome.HiddenLayerNeurons.RemoveAt(chromosome.HiddenLayerNeurons.Count - 1);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < Math.Abs(diff); i++)
                            {
                                chromosome.HiddenLayerNeurons.Add(NewNeuron(chromosome.InputCount));
                            }
                        }

                        for (int i = 0; i < chromosome.OutputLayerNeurons.Count; i++)
                        {
                            var newNeuronData = new float[chromosome.HiddenLayerNeuronCount + 1];
                            CopyAndFill(chromosome.OutputLayerNeurons[i], newNeuronData);
                            chromosome.OutputLayerNeurons[i] = newNeuronData;
                        }
                    }
                }

                foreach (var neuronData in chromosome.HiddenLayerNeurons)
                {
                    if (ShouldMutate())
                    {
                        MutateWeights(neuronData);
                    }
                }

                foreach (var neuronData in chromosome.OutputLayerNeurons)
                {
                    if (ShouldMutate())
                    {
                        MutateWeights(neuronData);
                    }
                }

                var network = new NeuralNetwork(chromosome,
                    agent.NetworkParameters.HiddenLayerActivationFunction, agent.NetworkParameters.OutputLayerActivationFunction);
                mutatedPopulation.Add(network);
            }
            return mutatedPopulation;
        }

        private bool ShouldMutate()
        {
            return _mutationRateDistribution.Sample() <= _parameters.MutationRate;
        }

        private void CopyAndFill(float[] source, float[] destination)
        {
            for (int i = 0; i < destination.Length; i++)
            {
                if (i < source.Length)
                {
                    destination[i] = source[i];
                }
                else
                {
                    destination[i] = (float)_parameters.NeuralNetworkWeightInitializationDistribution.Sample();
                }
            }
        }

        private void MutateWeights(float[] neuronData)
        {
            for (int i = 0; i < neuronData.Length; i++)
            {
                neuronData[i] += (float)_parameters.NeuralNetworkWeightMutationDistribution.Sample();
            }
        }

        private float[] NewNeuron(int inputCount)
        {
            var neuronData = new float[inputCount + 1];

            for (int i = 0; i < neuronData.Length; i++)
            {
                neuronData[i] = (float)_parameters.NeuralNetworkWeightInitializationDistribution.Sample();
            }

            return neuronData;
        }
    }
}