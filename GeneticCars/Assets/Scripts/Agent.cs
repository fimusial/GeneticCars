using GeneticCars.Assets.Scripts.AI;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

namespace GeneticCars.Assets.Scripts
{
    public class Agent
    {
        public GameObject GameObject { get; set; }
        public Car Car { get; set; }
        public NeuralNetwork Network { get; set; }
        public ICarController CarController { get; set; }
        public IFitnessAccumulator FitnessAccumulator { get; set; }
    }
}