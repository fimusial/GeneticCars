using GeneticCars.Assets.Scripts.AI;
using GeneticCars.Assets.Scripts.Game;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

namespace GeneticCars.Assets.Scripts
{
    public class DemoSceneManager : MonoBehaviour
    {
        public GameObject CarPrefab;

        private Vector3 _trackStartPoint = new Vector3(0f, 10f, 0f);
        private ICarController _carController;
        private const float FirstStartTimeDelay = 0.5f;

        public void Start()
        {
            Time.timeScale = 1f;

            _trackStartPoint = GameObject.FindGameObjectWithTag(Tags.TrackStartPoint).transform.position;
            GameObject gameObject = Instantiate(CarPrefab, _trackStartPoint, Quaternion.identity);

            Car car = gameObject.GetComponent<Car>();
            var network = (NeuralNetwork)Scenes.Data[DataTags.DemoAgentNetwork];
            Probe probe = gameObject.GetComponentInChildren<Probe>();

            probe.Reset(network.NetworkParameters.InputCount - 1);
            _carController = new NeuralNetworkCarController(car, probe, network);

            CarFollowCamera camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CarFollowCamera>();
            camera.Target = gameObject;
        }

        public void FixedUpdate()
        {
            if (Time.timeSinceLevelLoad < FirstStartTimeDelay)
            {
                return;
            }
            
            _carController.MoveCar();
        }
    }
}