using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

namespace GeneticCars.Assets.Scripts.Game.FitnessAccumulators
{
    public class DistanceTraveledFitnessAccumulator : IFitnessAccumulator
    {
        private readonly Car _car;
        private Vector3 _lastCarPosition;
        private float _distanceTraveled;

        public DistanceTraveledFitnessAccumulator(Car car)
        {
            _car = car;
            _lastCarPosition = car.transform.position;
            _distanceTraveled = 0f;
        }

        public float Fitness => _distanceTraveled;

        public void Update()
        {
            if (_car.HasCollidedWithWall)
            {
                return;
            }
            
            Vector3 travelVector = _car.transform.position - _lastCarPosition;
            _distanceTraveled += travelVector.magnitude;
            _lastCarPosition = _car.transform.position;
        }
    }
}