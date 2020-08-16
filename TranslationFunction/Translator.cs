using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TranslationFunction
{
    public static class Translator
    {
        private const string key_var = "TRANSLATOR_TEXT_SUBSCRIPTION_KEY";
        private static readonly string subscriptionKey = Environment.GetEnvironmentVariable(key_var);
        //private static readonly string subscriptionKey = key_var;

        private const string endpoint_var = "TRANSLATOR_TEXT_ENDPOINT";
        private static readonly string endpoint = Environment.GetEnvironmentVariable(endpoint_var);
        //private static readonly string endpoint = endpoint_var;

        

        [FunctionName("Translator")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            
            string route = "translate?api-version=3.0&to=de&to=it&to=ja&to=th";
            // Prompts you for text to translate. If you'd prefer, you can
            // provide a string as textToTranslate.
                                  
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string textToTranslate = data?.TextInput;            
            string translationResponse=await TranslateTextRequest(subscriptionKey, endpoint, route, textToTranslate);
            // Deserialize the response using the classes created earlier.
            TranslationResult[] deserializedOutput = JsonConvert.DeserializeObject<TranslationResult[]>(translationResponse);         
            
            // Iterate over the deserialized results.
            var _detectedSrcLanguage = string.Empty;
            var _confidenceScore = string.Empty;
            var _srcText = string.Empty;
            var _translationLang = string.Empty;
            var _translatedText = string.Empty;
            var _srcSentLen = string.Empty;
            var _translatedSentLen = string.Empty;
            Dictionary<string, string> translatedContent = new Dictionary<string, string>();

            foreach (TranslationResult transResult in deserializedOutput)
            {
                _detectedSrcLanguage = transResult.DetectedLanguage.Language;
                _confidenceScore = transResult.DetectedLanguage.Score.ToString();
                //TextResult sourceResult= transResult.SourceText;
                //_srcText = sourceResult.Text;

                // Iterate over the results of translation.
                foreach (Translation t in transResult.Translations)
                {
                    Console.Out.WriteLine("Translated to {0}: {1}", t.To, t.Text);
                   _translationLang = t.To;
                   _translatedText = t.Text;
                    translatedContent.Add(_translationLang, _translatedText);
                }
            }            
            return new OkObjectResult(translatedContent);
        }

        // This sample requires C# 7.1 or later for async/await.
        // Async call to the Translator
        public static async Task<string> TranslateTextRequest(string subscriptionKey, string endpoint, string route, string inputText)
        {
            object[] body = new object[] { new { Text = inputText } };
            var requestBody = JsonConvert.SerializeObject(body);
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                // Set the method to Post.
                request.Method = HttpMethod.Post;
                // Construct the URI and add headers.
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                // Read response as a string.
                string result = await response.Content.ReadAsStringAsync();
                return result;
                
                
            }
        }
    }

    /// <summary>
    /// The C# classes that represents the JSON returned by the Translator.
    /// </summary>
    public class TranslationResult
    {
        public DetectedLanguage DetectedLanguage { get; set; }
        public TextResult SourceText { get; set; }
        public Translation[] Translations { get; set; }
    }

    public class DetectedLanguage
    {
        public string Language { get; set; }
        public float Score { get; set; }
    }

    public class TextResult
    {
        public string Text { get; set; }
        public string Script { get; set; }
    }

    public class Translation
    {
        public string Text { get; set; }
        public TextResult Transliteration { get; set; }
        public string To { get; set; }
        public Alignment Alignment { get; set; }
        public SentenceLength SentLen { get; set; }
    }

    public class Alignment
    {
        public string Proj { get; set; }
    }

    public class SentenceLength
    {
        public int[] SrcSentLen { get; set; }
        public int[] TransSentLen { get; set; }
    }
}
