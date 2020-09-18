using UnityEngine;

namespace GeneticCars.Assets.Scripts
{
    public static class LayerMasks
    {
        public static int CarAgents => LayerMask.GetMask("CarAgents");
    }
}