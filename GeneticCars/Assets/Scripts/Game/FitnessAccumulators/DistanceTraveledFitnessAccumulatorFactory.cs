using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

namespace GeneticCars.Assets.Scripts.Game.FitnessAccumulators
{
    public class DistanceTraveledFitnessAccumulatorFactory : IFitnessAccumulatorFactory
    {
        public IFitnessAccumulator Create(GameObject gameObject)
        {
            return new DistanceTraveledFitnessAccumulator(gameObject.GetComponent<Car>());
        }
    }
}