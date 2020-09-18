using UnityEngine;

namespace GeneticCars.Assets.Scripts
{
    public class CarFollowCamera : MonoBehaviour
    {
        public GameObject Target;
        public float CameraDistance = 6f;
        public float CameraLift = 2.5f;
        public float CameraTilt = 0.25f;

        public void LateUpdate()
        {
            Vector3 carPos = Target.transform.position;
            Vector3 carForward = Target.transform.forward;
            Vector3 carUp = Target.transform.up;

            transform.position = carPos - (carForward * CameraDistance) + (carUp * CameraLift);
            transform.forward = carForward - new Vector3(0f, CameraTilt, 0f);
        }
    }
}