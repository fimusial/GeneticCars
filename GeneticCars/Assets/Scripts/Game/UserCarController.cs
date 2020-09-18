using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

namespace GeneticCars.Assets.Scripts.Game
{
    public class UserCarController : ICarController
    {
        private readonly Car _car;

        public UserCarController(Car car)
        {
            _car = car;
        }

        public void MoveCar()
        {
            float horizontal = 0f;
            if (Input.GetKey(KeyCode.D))
            {
                horizontal = 1f;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                horizontal = -1f;
            }

            float vertical = 0f;
            if (Input.GetKey(KeyCode.W))
            {
                vertical = 1f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                vertical = -1f;
            }

            _car.Move(horizontal, vertical);
        }
    }
}