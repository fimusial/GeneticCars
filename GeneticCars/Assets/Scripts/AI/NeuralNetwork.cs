using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;

namespace GeneticCars.Assets.Scripts.AI
{
    public class NeuralNetworkParameters
    {
        public int InputCount { get; set; }
        public int HiddenLayerNeuronCount { get; set; }
        public int OutputCount { get; set; }
        public Func<float, float> HiddenLayerActivationFunction { get; set; }
        public Func<float, float> OutputLayerActivationFunction { get; set; }
    }

    public class NeuralNetwork
    {
        public NeuralNetworkParameters NetworkParameters { get; private set; }
        public Matrix<float> HiddenWeights { get; private set; }
        public Vector<float> HiddenBiases { get; private set; }
        public Matrix<float> OutputWeights { get; private set; }
        public Vector<float> OutputBiases { get; private set; }

        public NeuralNetwork(NeuralNetworkParameters parameters, IContinuousDistribution rng)
        {
            NetworkParameters = parameters;
            HiddenWeights = CreateMatrix.Random<float>(NetworkParameters.HiddenLayerNeuronCount, NetworkParameters.InputCount, rng);
            HiddenBiases = CreateVector.Random<float>(NetworkParameters.HiddenLayerNeuronCount, rng);
            OutputWeights = CreateMatrix.Random<float>(NetworkParameters.OutputCount, NetworkParameters.HiddenLayerNeuronCount, rng);
            OutputBiases = CreateVector.Random<float>(NetworkParameters.OutputCount, rng);
        }

        public NeuralNetwork(Matrix<float> hiddenWeights, Vector<float> hiddenBiases,
            Matrix<float> outputWeights, Vector<float> outputBiases,
            Func<float, float> hiddenLayerActivationFunction, Func<float, float> outputLayerActivationFunction)
        {
            NetworkParameters = new NeuralNetworkParameters()
            {
                InputCount = hiddenWeights.ColumnCount,
                HiddenLayerNeuronCount = hiddenWeights.RowCount,
                OutputCount = outputWeights.RowCount,
                HiddenLayerActivationFunction = hiddenLayerActivationFunction,
                OutputLayerActivationFunction = outputLayerActivationFunction
            };

            HiddenWeights = hiddenWeights;
            HiddenBiases = hiddenBiases;
            OutputWeights = outputWeights;
            OutputBiases = outputBiases;
        }

        public NeuralNetwork(Chromosome chromosome,
            Func<float, float> hiddenLayerActivationFunction, Func<float, float> outputLayerActivationFunction)
        {
            NetworkParameters = new NeuralNetworkParameters()
            {
                InputCount = chromosome.InputCount,
                HiddenLayerNeuronCount = chromosome.HiddenLayerNeuronCount,
                OutputCount = chromosome.OutputLayerNeurons.Count,
                HiddenLayerActivationFunction = hiddenLayerActivationFunction,
                OutputLayerActivationFunction = outputLayerActivationFunction
            };

            HiddenWeights = GetWeightsFromChromosomeNeuronData(chromosome.HiddenLayerNeurons);
            HiddenBiases = GetBiasesFromChromosomeNeuronData(chromosome.HiddenLayerNeurons);
            OutputWeights = GetWeightsFromChromosomeNeuronData(chromosome.OutputLayerNeurons);
            OutputBiases = GetBiasesFromChromosomeNeuronData(chromosome.OutputLayerNeurons);
        }

        private Matrix<float> GetWeightsFromChromosomeNeuronData(IList<float[]> neurons)
        {
            return CreateMatrix.DenseOfRowArrays(neurons.Select(neuron => neuron.Skip(1).ToArray()));
        }

        private Vector<float> GetBiasesFromChromosomeNeuronData(IList<float[]> neurons)
        {
            return CreateVector.DenseOfArray(neurons.Select(neuron => neuron[0]).ToArray());
        }

        public Vector<float> FeedForward(Vector<float> inputs)
        {
            Vector<float> hiddenActivations = HiddenWeights
                .Multiply(inputs)
                .Add(HiddenBiases)
                .Map(NetworkParameters.HiddenLayerActivationFunction);

            return OutputWeights
                .Multiply(hiddenActivations)
                .Add(OutputBiases)
                .Map(NetworkParameters.OutputLayerActivationFunction);
        }

        public Chromosome GetChromosome()
        {
            return new Chromosome()
            {
                InputCount = NetworkParameters.InputCount,
                HiddenLayerNeuronCount = NetworkParameters.HiddenLayerNeuronCount,
                HiddenLayerNeurons = GetNeuronsDataForChromosome(HiddenWeights, HiddenBiases),
                OutputLayerNeurons = GetNeuronsDataForChromosome(OutputWeights, OutputBiases)
            };
        }

        private IList<float[]> GetNeuronsDataForChromosome(Matrix<float> weights, Vector<float> biases)
        {
            var neurons = new List<float[]>();
            for (int i = 0; i < weights.RowCount; i++)
            {
                var neuronData = new float[weights.ColumnCount + 1];
                neuronData[0] = biases[i];
                weights.Row(i).AsArray().CopyTo(neuronData, 1);
                neurons.Add(neuronData);
            }
            return neurons;
        }

        public NeuralNetwork Clone()
        {
            return new NeuralNetwork(HiddenWeights.Clone(), HiddenBiases.Clone(), OutputWeights.Clone(),
                OutputBiases.Clone(), NetworkParameters.HiddenLayerActivationFunction, NetworkParameters.OutputLayerActivationFunction);
        }

        public override string ToString()
        {
            return $"{NetworkParameters.InputCount}-{NetworkParameters.HiddenLayerNeuronCount} " +
            $"{HiddenWeights.GetHashCode()},{HiddenBiases.GetHashCode()},{OutputWeights.GetHashCode()},{OutputBiases.GetHashCode()}";
        }
    }
}
