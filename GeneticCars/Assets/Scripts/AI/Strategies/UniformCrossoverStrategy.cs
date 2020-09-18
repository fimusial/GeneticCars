using System;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;

namespace GeneticCars.Assets.Scripts.AI.Strategies
{
    public class UniformCrossoverStrategy : ICrossoverStrategy
    {
        private readonly IContinuousDistribution _crossoverRateDistribution = new ContinuousUniform(0, 1);
        private readonly float _crossoverRate;

        public UniformCrossoverStrategy(float crossoverRate)
        {
            _crossoverRate = crossoverRate;
        }

        public IList<NeuralNetwork> ApplyCrossover(IList<NeuralNetwork> population)
        {
            int[] permutation = Combinatorics.GeneratePermutation(population.Count);

            var newPopulation = new List<NeuralNetwork>();
            for (int i = 0; i < population.Count; i += 2)
            {
                (NeuralNetwork, NeuralNetwork) offspring = PairCrossover(population[permutation[i]], population[permutation[i + 1]]);
                newPopulation.Add(offspring.Item1);
                newPopulation.Add(offspring.Item2);
            }

            return newPopulation;
        }

        public (NeuralNetwork, NeuralNetwork) PairCrossover(NeuralNetwork agent1, NeuralNetwork agent2)
        {
            Chromosome agent1chromosome = agent1.GetChromosome();
            Chromosome agent2chromosome = agent2.GetChromosome();

            int minInputCount = Math.Min(agent1chromosome.InputCount, agent2chromosome.InputCount);
            int minHiddenLayerNeuronCount = Math.Min(agent1chromosome.HiddenLayerNeuronCount, agent2chromosome.HiddenLayerNeuronCount);
            int outputNeuronCount = 2;

            for (int i = 0; i < minHiddenLayerNeuronCount; i++)
            {
                for (int j = 0; j < minInputCount; j++)
                {
                    if (ShouldCrossover())
                    {
                        float tmp = agent1chromosome.HiddenLayerNeurons[i][j];
                        agent1chromosome.HiddenLayerNeurons[i][j] = agent2chromosome.HiddenLayerNeurons[i][j];
                        agent2chromosome.HiddenLayerNeurons[i][j] = tmp;
                    }
                }
            }

            for (int i = 0; i < outputNeuronCount; i++)
            {
                for (int j = 0; j < minHiddenLayerNeuronCount; j++)
                {
                    if (ShouldCrossover())
                    {
                        float tmp = agent1chromosome.OutputLayerNeurons[i][j];
                        agent1chromosome.OutputLayerNeurons[i][j] = agent2chromosome.OutputLayerNeurons[i][j];
                        agent2chromosome.OutputLayerNeurons[i][j] = tmp;
                    }
                }
            }
            return (new NeuralNetwork(agent1chromosome, agent1.NetworkParameters.HiddenLayerActivationFunction, agent1.NetworkParameters.OutputLayerActivationFunction),
                new NeuralNetwork(agent2chromosome, agent2.NetworkParameters.HiddenLayerActivationFunction, agent2.NetworkParameters.OutputLayerActivationFunction));
        }

        private bool ShouldCrossover()
        {
            return _crossoverRateDistribution.Sample() < _crossoverRate;
        }
    }
}