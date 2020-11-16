using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

namespace GeneticCars.Assets.Scripts.Game.FitnessAccumulators
{
    public class StabilizedDistanceTraveledFitnessAccumulatorFactory : IFitnessAccumulatorFactory
    {
        public IFitnessAccumulator Create(GameObject gameObject)
        {
            return new StabilizedDistanceTraveledFitnessAccumulator(gameObject.GetComponent<Car>(), gameObject.GetComponentInChildren<Probe>());
        }
    }
}