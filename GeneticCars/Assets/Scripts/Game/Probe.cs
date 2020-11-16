using System;
using System.Linq;
using UnityEngine;

namespace GeneticCars.Assets.Scripts.Game
{
    public class Probe : MonoBehaviour
    {
        public float SpreadAngle = 120f;
        public float MaxSensorDistance = 75f;
        public bool DrawRays = true;

        public int SensorCount { get; private set; }
        public float[] SensorDistances { get; private set; }

        private Quaternion[] _sensorRotations;
        private Ray[] _sensorRays;

        public void Reset(int sensorCount)
        {
            if (sensorCount < 2)
            {
                throw new ArgumentOutOfRangeException("Sensor count cannot be lower than 2.");
            }
            
            SensorCount = sensorCount;
            SensorDistances = Enumerable.Repeat(MaxSensorDistance, SensorCount).ToArray();
            _sensorRotations = new Quaternion[SensorCount];
            _sensorRays = new Ray[SensorCount];

            float incrementAngle = SpreadAngle / (SensorCount - 1);
            for (int i = 0; i < SensorCount; i++)
            {
                float rotationAngle = (SpreadAngle / 2) - i * incrementAngle;
                _sensorRotations[i] = Quaternion.Euler(0f, rotationAngle, 0f);
            }
        }

        public void FixedUpdate()
        {
            var hit = new RaycastHit();
            for (int i = 0; i < SensorCount; i++)
            {
                _sensorRays[i].origin = transform.position;
                _sensorRays[i].direction = _sensorRotations[i] * transform.forward;

                if (Physics.Raycast(_sensorRays[i], out hit, MaxSensorDistance, ~LayerMasks.CarAgents))
                {
                    SensorDistances[i] = hit.distance;
                }
                else
                {
                    SensorDistances[i] = MaxSensorDistance;
                }
            }
        }

        public void Update()
        {
            if (DrawRays)
            {
                for (int i = 0; i < SensorCount; i++)
                {
                    Ray ray = _sensorRays[i];
                    Debug.DrawLine(ray.origin, ray.origin + ray.direction * SensorDistances[i], Color.green);
                }
            }
        }
    }
}