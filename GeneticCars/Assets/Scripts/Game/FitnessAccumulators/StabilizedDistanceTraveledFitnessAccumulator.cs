using System;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

namespace GeneticCars.Assets.Scripts.Game.FitnessAccumulators
{
    public class StabilizedDistanceTraveledFitnessAccumulator : IFitnessAccumulator
    {
        private readonly Car _car;
        private readonly Probe _probe;
        private Vector3 _lastPosition;

        private float _distanceTraveled;
        private float _stabilizationPenalty;

        private const double CurveFactor = 0.71d;

        public StabilizedDistanceTraveledFitnessAccumulator(Car car, Probe probe)
        {
            _car = car;
            _probe = probe;
            _lastPosition = car.transform.position;
            _distanceTraveled = 0f;
        }

        public float Fitness => _distanceTraveled - _stabilizationPenalty;

        public void Update()
        {
            if (_car.HasCollidedWithWall)
            {
                return;
            }

            float displacement = (_car.transform.position - _lastPosition).magnitude;
            _distanceTraveled += displacement;
            _lastPosition = _car.transform.position;

            float leftSensorDistance = _probe.SensorDistances[0];
            float rightSensorDistance = _probe.SensorDistances[_probe.SensorCount - 1];

            float deviation = Math.Abs(leftSensorDistance - rightSensorDistance);
            _stabilizationPenalty += displacement * CurveFunction(deviation / (leftSensorDistance + rightSensorDistance));
        }

        private float CurveFunction(float value)
        {
            return (float)Math.Pow((double)value, CurveFactor);
        }
    }
}