using System.Collections.Generic;

namespace GeneticCars.Assets.Scripts.AI
{
    public interface IMutationStrategy
    {
        IList<NeuralNetwork> ApplyMutations(IList<NeuralNetwork> population);
    }
}