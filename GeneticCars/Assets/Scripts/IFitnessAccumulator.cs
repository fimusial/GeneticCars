namespace GeneticCars.Assets.Scripts
{
    public interface IFitnessAccumulator
    {
        void Update();
        float Fitness { get; }
    }
}