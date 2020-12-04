using System;
using GeneticCars.Assets.Scripts.AI;
using GeneticCars.Assets.Scripts.AI.Strategies;
using GeneticCars.Assets.Scripts.Game.FitnessAccumulators;
using MathNet.Numerics.Distributions;
using UnityEngine;
using UnityEngine.UI;

namespace GeneticCars.Assets.Scripts
{
    public class MenuSceneManager : MonoBehaviour
    {
        public InputField PopulationPairCount;
        public InputField MaxGenerationCount;
        public InputField WeightInitStdDev;

        public InputField MinSensorCount;
        public InputField MaxSensorCount;
        public InputField MinHiddenLayerNeuronCount;
        public InputField MaxHiddenLayerNeuronCount;
        public Toggle EvolveNetworkTopology;

        public Dropdown HiddenLayerActivationFunction;
        public Dropdown SelectionStrategy;
        public Toggle KeepBestAgent;
        public Dropdown CrossoverStrategy;
        public Dropdown MutationStrategy;
        public Dropdown FitnessFunctionType;
        public InputField CrossoverRate;
        public InputField MutationRate;
        public InputField MutationRange;
        public Text ValidationMessage;

        private float _timestamp;

        public void Start()
        {
            EvolveNetworkTopology.onValueChanged.AddListener((value) =>
            {
                if (!EvolveNetworkTopology.isOn)
                {
                    MaxSensorCount.interactable = false;
                    MaxHiddenLayerNeuronCount.interactable = false;
                }
                else
                {
                    MaxSensorCount.interactable = true;
                    MaxHiddenLayerNeuronCount.interactable = true;
                }
            });

            SetTimestampToNow();
        }

        public void Update()
        {
            if (GetTimeSinceLastTimestamp() > 2f)
            {
                ValidationMessage.enabled = false;
            }
        }

        public void ButtonStartOnClick()
        {
            if (!IsFormValid())
            {
                ValidationMessage.enabled = true;
                SetTimestampToNow();
                return;
            }

            var geneticAlgorithmParameters = new GeneticAlgorithmParameters()
            {
                PopulationCount = 2 * int.Parse(PopulationPairCount.text),
                MaxGenerationCount = int.Parse(MaxGenerationCount.text),
                NeuralNetworkWeightInitializationDistribution = new Normal(0f, double.Parse(WeightInitStdDev.text)),

                MinSensorCount = int.Parse(MinSensorCount.text),
                MaxSensorCount = int.Parse(MaxSensorCount.text),
                MinHiddenLayerNeuronCount = int.Parse(MinHiddenLayerNeuronCount.text),
                MaxHiddenLayerNeuronCount = int.Parse(MaxHiddenLayerNeuronCount.text),
                EvolveNetworkTopology = EvolveNetworkTopology.isOn,

                NeuralNetworkHiddenLayerActivationFunction = GetHiddenLayerActivationFunction(),
                NeuralNetworkOutputLayerActivationFunction = x => (float)Math.Tanh(x),

                SelectionStrategy = GetSelectionStrategy(),
                CrossoverStrategy = GetCrossoverStrategy()
            };

            geneticAlgorithmParameters.MutationStrategy = GetMutationStrategy(geneticAlgorithmParameters);
            IFitnessAccumulatorFactory fitnessAccumulatorFactory = GetFitnessAccumulatorFactory();

            Scenes.Data.Add(DataTags.GeneticAlgorithmParameters, geneticAlgorithmParameters);
            Scenes.Data.Add(DataTags.FitnessAccumulatorFactory, fitnessAccumulatorFactory);
            Scenes.Load(SceneId.MainScene);
        }

        private Func<float, float> GetHiddenLayerActivationFunction()
        {
            switch (HiddenLayerActivationFunction.value)
            {
                case 0: return x => (float)Math.Tanh(x);
                case 1: return x => (float)Math.Max(0, x);
                case 2: return x => (float)(1 / (1 + Math.Exp(-x)));
                default: throw new Exception("Invalid dropdown value.");
            }
        }

        private ISelectionStrategy GetSelectionStrategy()
        {
            ISelectionStrategy strategy = null;

            switch (SelectionStrategy.value)
            {
                case 0: strategy = new RouletteWheelSelectionStrategy(); break;
                case 1: strategy = new RankBasedSelectionStrategy(); break;
                default: throw new Exception("Invalid dropdown value.");
            }

            if (KeepBestAgent.isOn)
            {
                strategy = new BestAgentSelectionStrategyDecorator(strategy);
            }

            return strategy;
        }

        private ICrossoverStrategy GetCrossoverStrategy()
        {
            switch (CrossoverStrategy.value)
            {
                case 0: return new UniformCrossoverStrategy(float.Parse(CrossoverRate.text));
                default: throw new Exception("Invalid dropdown value.");
            }
        }

        private IMutationStrategy GetMutationStrategy(GeneticAlgorithmParameters geneticAlgorithmParameters)
        {
            switch (MutationStrategy.value)
            {
                case 0:
                    double mutationRangeD2 = double.Parse(MutationRange.text) / 2;
                    var parameters = new StandardMutationStrategyParameters()
                    {
                        MinSensorCount = geneticAlgorithmParameters.MinSensorCount,
                        MaxSensorCount = geneticAlgorithmParameters.MaxSensorCount,
                        MinHiddenLayerNeuronCount = geneticAlgorithmParameters.MinHiddenLayerNeuronCount,
                        MaxHiddenLayerNeuronCount = geneticAlgorithmParameters.MaxHiddenLayerNeuronCount,
                        EvolveNetworkTopology = EvolveNetworkTopology.isOn,

                        NeuralNetworkWeightInitializationDistribution = geneticAlgorithmParameters.NeuralNetworkWeightInitializationDistribution,
                        MutationRate = float.Parse(MutationRate.text),
                        NeuralNetworkWeightMutationDistribution = new ContinuousUniform(-mutationRangeD2, mutationRangeD2)
                    };
                    return new StandardMutationStrategy(parameters);

                default: throw new Exception("Invalid dropdown value.");
            }
        }

        private IFitnessAccumulatorFactory GetFitnessAccumulatorFactory()
        {
            switch (FitnessFunctionType.value)
            {
                case 0: return new DistanceTraveledFitnessAccumulatorFactory();
                case 1: return new StabilizedDistanceTraveledFitnessAccumulatorFactory();
                default: throw new Exception("Invalid dropdown value.");
            }
        }

        private bool IsFormValid()
        {
            if (int.Parse(PopulationPairCount.text) < 1) return false;
            if (int.Parse(MaxGenerationCount.text) < 1) return false;

            if (int.Parse(MinSensorCount.text) < 2) return false;
            if (int.Parse(MaxSensorCount.text) <= int.Parse(MinSensorCount.text)) return false;

            if (int.Parse(MinHiddenLayerNeuronCount.text) < 1) return false;
            if (int.Parse(MaxHiddenLayerNeuronCount.text) <= int.Parse(MinHiddenLayerNeuronCount.text)) return false;

            if (float.Parse(CrossoverRate.text) <= 0f || float.Parse(CrossoverRate.text) > 1f) return false;
            if (float.Parse(MutationRate.text) <= 0f || float.Parse(MutationRate.text) > 1f) return false;
            if (float.Parse(MutationRange.text) <= 0f) return false;

            return true;
        }

        private void SetTimestampToNow()
        {
            _timestamp = Time.time;
        }

        private float GetTimeSinceLastTimestamp()
        {
            return Time.time - _timestamp;
        }
    }
}