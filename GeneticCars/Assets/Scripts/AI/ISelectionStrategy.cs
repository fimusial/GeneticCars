using System.Collections.Generic;

namespace GeneticCars.Assets.Scripts.AI
{
    public interface ISelectionStrategy
    {
        IList<NeuralNetwork> ApplySelection(IList<GeneticAlgorithmAgent> agents);
    }
}