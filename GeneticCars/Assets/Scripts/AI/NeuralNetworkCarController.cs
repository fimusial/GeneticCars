using System;
using GeneticCars.Assets.Scripts.Game;
using MathNet.Numerics.LinearAlgebra;
using UnityStandardAssets.Vehicles.Car;

namespace GeneticCars.Assets.Scripts.AI
{
    public class NeuralNetworkCarController : ICarController
    {
        private readonly Car _car;
        private readonly Probe _probe;
        private NeuralNetwork _network;

        public NeuralNetworkCarController(Car car, Probe probe, NeuralNetwork network)
        {
            _car = car;
            _probe = probe;
            _network = network;
        }

        public void MoveCar()
        {
            if (_car.HasCollidedWithWall)
            {
                _car.Move(0f, -1f);
                return;
            }

            float speedNormalized = _car.CurrentSpeed / _car.MaxSpeed;
            float[] sensorDistancesNormalized = new float[_probe.SensorCount];
            _probe.SensorDistances.CopyTo(sensorDistancesNormalized, 0);
            Array.ForEach(sensorDistancesNormalized, distance => distance = distance / _probe.MaxSensorDistance);

            float[] networkInputs = new float[_probe.SensorCount + 1];
            networkInputs[0] = speedNormalized;
            sensorDistancesNormalized.CopyTo(networkInputs, 1);

            Vector<float> networkOutputs = _network.FeedForward(CreateVector.DenseOfArray(networkInputs));
            _car.Move(networkOutputs[0], networkOutputs[1]);
        }
    }
}