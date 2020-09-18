using System.Collections.Generic;

namespace GeneticCars.Assets.Scripts.AI
{
    public interface ICrossoverStrategy
    {
        IList<NeuralNetwork> ApplyCrossover(IList<NeuralNetwork> population);
    }
}