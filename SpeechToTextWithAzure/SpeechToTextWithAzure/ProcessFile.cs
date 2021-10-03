using SpeechToTextWithAzure.Models;
using System;
using NAudio.Wave;
using Microsoft.CognitiveServices.Speech;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech.Audio;
using System.IO;

namespace SpeechToTextWithAzure
{
    internal class ProcessFile
    {
        /// <summary>
        ///     Main logic for processing file.
        /// </summary>
        /// <param name="file">
        ///     The file configuration object.
        /// </param>
        /// <param name="cognitiveServices">
        ///     The configuration data for the Cognitive Services.
        /// </param>
        /// <returns></returns>
        internal async Task ProcessFileLogic(
            ConfigurationFile file, 
            AzureCognitiveServices cognitiveServices)
        {
            Console.WriteLine($"Processing file '{file.FilePath}'...");

            // Convert MP3 to WAV
            string filePathOut = ConvertMP3ToWAV(file.FilePath);

            // Convert Speech to Text
            string text = await ConvertSpeechToText(
                    cognitiveServices.SpeechToTextService.Key,
                    cognitiveServices.SpeechToTextService.Region,
                    file.SourceLanguage,
                    filePathOut
                );

            // Write text to file
            string textFilePath = filePathOut.Replace(".wav", ".txt");
            await File.WriteAllTextAsync(textFilePath, text);
        }


        /// <summary>
        ///     Convert a MP3 file to a WAV file.
        ///     See also https://github.com/naudio/NAudio/blob/master/Docs/ConvertMp3ToWav.md.
        ///     Requires <TargetFramework>net5.0-windows</TargetFramework>.
        /// </summary>
        /// <param name="filePathIn">
        ///     The path to the WAV file.
        /// </param>
        private string ConvertMP3ToWAV(string filePathIn)
        {
            string filePathOut = filePathIn.Replace(".mp3", ".wav");

            using (Mp3FileReader reader = new Mp3FileReader(filePathIn))
            {
                WaveFileWriter.CreateWaveFile(filePathOut, reader);
            }

            return filePathOut;
        }

        /// <summary>
        ///     Convert the Speech to Text.
        /// </summary>
        /// <param name="speechToTextServiceKey">
        ///     The Cognitive Services Speech to Text instance key.
        /// </param>
        /// <param name="speechToTextServiceRegion">
        ///     The Cognitive Services Speech to Text deployment region.
        /// </param>
        /// <param name="fileSourceLanguage">
        ///     The source language of the speech in BCP-47 format.
        /// </param>
        /// <param name="wavFilePath">
        ///     The path to the WAV file.
        /// </param>
        /// <returns>
        ///     Returns the recognized text.
        /// </returns>
        private async Task<string> ConvertSpeechToText(
                string speechToTextServiceKey,
                string speechToTextServiceRegion,
                string fileSourceLanguage,
                string wavFilePath
            )
        {
            try
            {
                // Configure
                SpeechConfig speechConfig = SpeechConfig.FromSubscription(
                speechToTextServiceKey,
                speechToTextServiceRegion);
                // https://docs.microsoft.com/en-us/dotnet/api/microsoft.cognitiveservices.speech.speechconfig?view=azure-dotnet
                speechConfig.SpeechRecognitionLanguage = fileSourceLanguage;
                string resultingText = "";

                // Execute
                using (AudioConfig audioConfig = AudioConfig.FromWavFileInput(wavFilePath))
                using (SpeechRecognizer recognizer = new SpeechRecognizer(speechConfig, audioConfig))
                {
                    TaskCompletionSource<int> stopRecognition = new TaskCompletionSource<int>();

                    recognizer.Recognizing += (s, e) =>
                    {
                        //Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                            resultingText += $"{e.Result.Text}{Environment.NewLine}";
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the speech key and location/region info?");
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine($"{Environment.NewLine}    Session stopped event.");
                        stopRecognition.TrySetResult(0);
                    };

                    await recognizer.StartContinuousRecognitionAsync();

                    await Task.WhenAny(new[] { stopRecognition.Task });

                    return resultingText;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
