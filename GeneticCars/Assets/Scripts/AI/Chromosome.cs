using System.Collections.Generic;

namespace GeneticCars.Assets.Scripts.AI
{
    public class Chromosome
    {
        public int InputCount { get; set; }
        public int HiddenLayerNeuronCount { get; set; }
        public IList<float[]> HiddenLayerNeurons { get; set; }
        public IList<float[]> OutputLayerNeurons { get; set; }
    }
}