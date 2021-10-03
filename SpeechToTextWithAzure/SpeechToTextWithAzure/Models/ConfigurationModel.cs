using Newtonsoft.Json;
using System.Collections.Generic;

namespace SpeechToTextWithAzure.Models
{
    public class ConfigurationModel
    {
        [JsonProperty("cognitiveServices")]
        public AzureCognitiveServices CognitiveServices { get; set; }

        [JsonProperty("files")]
        public List<ConfigurationFile> Files { get; set; }
    }

    public class AzureCognitiveServices
    {
        [JsonProperty("speechToTextService")]
        public AzureCognitiveServicesSpeechToText SpeechToTextService { get; set; }
    }

    public class AzureCognitiveServicesSpeechToText
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }
    }

    public class ConfigurationFile
    {
        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        [JsonProperty("sourceLanguage")]
        public string SourceLanguage { get; set; }
    }
}