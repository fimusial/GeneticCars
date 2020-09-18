using UnityStandardAssets.Vehicles.Car;

namespace GeneticCars.Assets.Scripts.Game.FitnessAccumulators
{
    public class DistanceTraveledFitnessAccumulatorFactory : IFitnessAccumulatorFactory
    {
        public IFitnessAccumulator Create(Car car)
        {
            return new DistanceTraveledFitnessAccumulator(car);
        }
    }
}