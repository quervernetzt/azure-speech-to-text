using Newtonsoft.Json;
using SpeechToTextWithAzure.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SpeechToTextWithAzure
{
    public class Program
    {
        /// <summary>
        ///     Main method.
        /// </summary>
        /// <param name="args">
        ///     Input arguments.
        /// </param>
        public async static Task Main(string[] args)
        {
            Console.WriteLine("Starting...");

            // Get configuration
            ConfigurationModel configuration = GetConfiguration();

            // Process files
            List<Task> tasks = new List<Task>();
            ProcessFile processFile = new ProcessFile();
            foreach (ConfigurationFile file in configuration.Files)
            {
                tasks.Add(Task.Run(() =>
                    processFile.ProcessFileLogic(
                        file,
                        configuration.CognitiveServices)
                ));
            }
            await Task.WhenAll(tasks.ToArray());

            Console.WriteLine("Done...");
        }


        /// <summary>
        ///     Get the configuration.
        /// </summary>
        /// <returns>
        ///     Returns the configuration object.
        /// </returns>
        private static ConfigurationModel GetConfiguration()
        {
            string filePath = "Configuration.json";

            string fileContent = File.ReadAllText(filePath);

            ConfigurationModel configuration = JsonConvert.DeserializeObject<ConfigurationModel>(fileContent);

            return configuration;
        }
    }
}
