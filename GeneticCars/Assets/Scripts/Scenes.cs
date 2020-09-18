using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace GeneticCars.Assets.Scripts
{
    public enum SceneId
    {
        MenuScene,
        MainScene,
        DemoScene
    }

    public static class Scenes
    {
        public static IDictionary<string, object> Data { get; } = new Dictionary<string, object>();

        public static void Load(SceneId sceneId)
        {
            SceneManager.LoadScene(sceneId.ToString());
        }
    }
}