using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeneticCars.Assets.Scripts
{
    public static class DataCollector
    {
        private static IDictionary<string, IList<object>> DataSets;
        private const string FileExtension = ".gcdata";

        public static class SetsNames
        {
            public const string MaxFitness = "max-fitness";
            public const string MinFitness = "min-fitness";
            public const string AvgFitness = "avg-fitness";
            public const string TopologyTrends = "topology-trends";
        }

        static DataCollector()
        {
            DataSets = new Dictionary<string, IList<object>>();
        }

        public static void AddDataPoint(string setName, object dataPoint)
        {
            if (!DataSets.ContainsKey(setName))
            {
                var newSet = new List<object>();
                newSet.Add(dataPoint);
                DataSets.Add(setName, newSet);
            }
            else
            {
                DataSets.TryGetValue(setName, out IList<object> dataSet);
                dataSet.Add(dataPoint);
            }
        }

        public static void SaveDataSets(string rootDirectoryPath)
        {
            string directoryPath = Path.Combine(rootDirectoryPath, GenerateDirectoryName());
            Directory.CreateDirectory(directoryPath);

            foreach (var kvp in DataSets)
            {
                string filePath = Path.Combine(directoryPath, kvp.Key + FileExtension);
                File.WriteAllLines(filePath, kvp.Value.Select(point => point.ToString()));
            }
        }

        private static string GenerateDirectoryName()
        {
            string dateString = DateTime.Now.ToString("yyyy-MM-dd");
            string timeString = DateTime.Now.ToString("hh-mm");
            return $"{dateString}t{timeString}";
        }
    }
}