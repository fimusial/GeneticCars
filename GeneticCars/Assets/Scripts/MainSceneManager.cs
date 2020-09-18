using System;
using System.Collections.Generic;
using System.Linq;
using GeneticCars.Assets.Scripts.AI;
using GeneticCars.Assets.Scripts.Game;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Vehicles.Car;

namespace GeneticCars.Assets.Scripts
{
    public class MainSceneManager : MonoBehaviour
    {
        public GameObject CarPrefab;
        public float GeneticAlgorithmSimulationTimeScale;
        public Text TimeSinceStartText;
        public Text GenerationTimeLeftText;
        public Text GenerationNumberText;
        public Text HighestGenerationFitnessText;
        public Text HighestFitnessText;
        public Text TimeScaleIndicatorText;

        private GeneticAlgorithm _geneticAlgorithm;
        private IFitnessAccumulatorFactory _fitnessAccumulatorFactory;
        private Vector3 _trackStartPoint = new Vector3(0f, 10f, 0f);

        private IList<Agent> _agents;
        private float _highestFitness = 0f;
        private float _timestamp;
        private float _agentsLifespan = 50f;
        private const float DefaultTimeScale = 1f;
        private const float FirstStartTimeDelay = 0.5f;

        public void Start()
        {
            var geneticAlgorithmParameters = (GeneticAlgorithmParameters)Scenes.Data[DataTags.GeneticAlgorithmParameters];
            var fitnessAccumulatorFactory = (IFitnessAccumulatorFactory)Scenes.Data[DataTags.FitnessAccumulatorFactory];

            _fitnessAccumulatorFactory = fitnessAccumulatorFactory;
            _geneticAlgorithm = new GeneticAlgorithm(geneticAlgorithmParameters);
            _trackStartPoint = GameObject.FindGameObjectWithTag(Tags.TrackStartPoint).transform.position;

            _geneticAlgorithm.Initialize();

            _agents = Enumerable
                .Range(0, _geneticAlgorithm.AlgorithmParameters.PopulationCount)
                .Select(i => new Agent())
                .ToList();

            ResetAgents();

            Time.timeScale = GeneticAlgorithmSimulationTimeScale;
            TimeScaleIndicatorText.text = $"Time scale: {Time.timeScale.ToString()}x";

            SetTimestampToNow();
        }

        public void FixedUpdate()
        {
            if (Time.timeSinceLevelLoad < FirstStartTimeDelay)
            {
                return;
            }

            if (GetGenerationTimeLeft() <= 0f || CanForceSkipGeneration())
            {
                if (!_geneticAlgorithm.IsGenerationLimitReached())
                {
                    _geneticAlgorithm.NextGeneration(_agents.Select(agent => new GeneticAlgorithmAgent()
                    {
                        Network = agent.Network,
                        Fitness = agent.FitnessAccumulator.Fitness
                    })
                    .ToList());

                    ResetAgents();
                    SetTimestampToNow();
                }
                else
                {
                    NeuralNetwork demoAgentNetwork = _agents
                        .OrderByDescending(agent => agent.FitnessAccumulator.Fitness)
                        .First()
                        .Network
                        .Clone();

                    try
                    {
                        Scenes.Data.Add(DataTags.DemoAgentNetwork, demoAgentNetwork);
                    }
                    catch (ArgumentException) {}

                    Scenes.Load(SceneId.DemoScene);
                }

                return;
            }

            foreach (var agent in _agents)
            {
                agent.CarController.MoveCar();
                agent.FitnessAccumulator.Update();
            }
        }

        public void Update()
        {
            TimeSinceStartText.text = Time.time.ToString();
            GenerationTimeLeftText.text = GetGenerationTimeLeft().ToString();
            GenerationNumberText.text = _geneticAlgorithm.Generation.ToString();
            HighestGenerationFitnessText.text = GetHighestGenerationFitness().ToString();
            HighestFitnessText.text = _highestFitness.ToString();
        }

        private void ResetAgents()
        {
            float highestGenerationFitness = GetHighestGenerationFitness();
            if (highestGenerationFitness > _highestFitness)
            {
                _highestFitness = highestGenerationFitness;
            }

            foreach (var (agent, index) in _agents.Select((agent, index) => (agent, index)))
            {
                if (agent.GameObject != null)
                {
                    Destroy(agent.GameObject);
                }

                GameObject gameObject = Instantiate(CarPrefab, _trackStartPoint, Quaternion.identity);
                Car car = gameObject.GetComponent<Car>();
                NeuralNetwork network = _geneticAlgorithm.Population[index];
                Probe probe = gameObject.GetComponentInChildren<Probe>();

                agent.GameObject = gameObject;
                agent.Car = car;
                agent.Network = network;

                probe.Reset(network.NetworkParameters.InputCount - 1);
                agent.CarController = new NeuralNetworkCarController(car, probe, network);
                agent.FitnessAccumulator = _fitnessAccumulatorFactory.Create(car);
            }
        }

        private void SetTimestampToNow()
        {
            _timestamp = Time.time;
        }

        private float GetTimeSinceLastTimestamp()
        {
            return Time.time - _timestamp;
        }

        private float GetGenerationTimeLeft()
        {
            return _agentsLifespan - GetTimeSinceLastTimestamp();
        }

        private float GetHighestGenerationFitness()
        {
            if (_agents.Any(agent => agent.GameObject == null))
            {
                return 0f;
            }
            else
            {
                return _agents.Max(agent => agent.FitnessAccumulator.Fitness);
            }
        }

        private bool CanForceSkipGeneration()
        {
            bool pastStart = GetGenerationTimeLeft() <= (_agentsLifespan * 0.9f);
            bool allStopped = _agents.All(agent => agent.Car.CurrentSpeed <= 0.005f);
            return pastStart && allStopped;
        }
    }
}