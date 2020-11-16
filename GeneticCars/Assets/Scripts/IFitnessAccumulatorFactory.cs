using UnityEngine;

namespace GeneticCars.Assets.Scripts
{
    public interface IFitnessAccumulatorFactory
    {
        IFitnessAccumulator Create(GameObject gameObject);
    }
}