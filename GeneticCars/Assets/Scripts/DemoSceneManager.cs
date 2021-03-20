using System.Collections.Generic;
using System.Linq;
using GeneticCars.Assets.Scripts.AI;
using GeneticCars.Assets.Scripts.Game;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Vehicles.Car;

namespace GeneticCars.Assets.Scripts
{
    public class DemoSceneManager : MonoBehaviour
    {
        public GameObject CarPrefab;
        public Text TextFitness;

        private int _trackIndex;
        private IList<GameObject> _tracks;

        private GameObject _carGameObject;
        private ICarController _carController;
        private IFitnessAccumulator _fitnessAccumulator;

        private const float FirstStartTimeDelay = 0.5f;

        public void Start()
        {
            Time.timeScale = 1f;

            _trackIndex = 0;
            _tracks = GameObject.FindGameObjectsWithTag(Tags.Track).ToList();

            foreach (var track in _tracks)
            {
                track.SetActive(false);
            }

            NextTrack(false);

            DataCollector.AddDataPoint("nn", ((NeuralNetwork)Scenes.Data[DataTags.DemoAgentNetwork]).ToFullParamsString());
            DataCollector.SaveDataSets(@"D:\Data\other\genetic-cars-data");
        }

        public void FixedUpdate()
        {
            if (Time.timeSinceLevelLoad < FirstStartTimeDelay)
            {
                return;
            }

            _carController.MoveCar();
            _fitnessAccumulator.Update();

            TextFitness.text = _fitnessAccumulator.Fitness.ToString();
        }

        public void CycleTracks()
        {
            NextTrack(true);
        }

        private void NextTrack(bool shouldDestroyCarGameObject)
        {
            _tracks[_trackIndex].SetActive(false);
            _trackIndex = (_trackIndex + 1) % _tracks.Count;
            _tracks[_trackIndex].SetActive(true);

            if (shouldDestroyCarGameObject)
            {
                Destroy(_carGameObject);
            }

            Transform trackStartPoint = _tracks[_trackIndex].transform.Find(Tags.TrackStartPoint);
            GameObject gameObject = _carGameObject = Instantiate(CarPrefab, trackStartPoint.position, trackStartPoint.rotation);

            Car car = gameObject.GetComponent<Car>();
            var network = (NeuralNetwork)Scenes.Data[DataTags.DemoAgentNetwork];
            Probe probe = gameObject.GetComponentInChildren<Probe>();

            probe.Reset(network.NetworkParameters.InputCount - 1);
            _carController = new NeuralNetworkCarController(car, probe, network);

            CarFollowCamera camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CarFollowCamera>();
            camera.Target = gameObject;

            _fitnessAccumulator = ((IFitnessAccumulatorFactory)Scenes.Data[DataTags.FitnessAccumulatorFactory]).Create(gameObject);
        }
    }
}