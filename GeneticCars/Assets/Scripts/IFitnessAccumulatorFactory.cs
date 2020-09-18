using UnityStandardAssets.Vehicles.Car;

namespace GeneticCars.Assets.Scripts
{
    public interface IFitnessAccumulatorFactory
    {
        IFitnessAccumulator Create(Car car);
    }
}