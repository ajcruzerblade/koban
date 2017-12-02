using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json.Linq;

namespace ServiceHelpers
{
    public class TextAnalyticsHelper
    {
        private static HttpClient httpClient { get; set; }

        private static string apiKey;

        public static string ApiKey
        {
            get { return apiKey; }
            set
            {
                var changed = apiKey != value;
                apiKey = value;
                if (changed)
                {
                    InitializeTextAnalyticsClient();
                }
            }
        }

        private static string apiKeyRegion;
        public static string ApiKeyRegion
        {
            get { return apiKeyRegion; }
            set
            {
                var changed = apiKeyRegion != value;
                apiKeyRegion = value;
                if (changed)
                {
                    InitializeTextAnalyticsClient();
                }
            }
        }

        private static void InitializeTextAnalyticsClient()
        {
            if (!string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiKeyRegion))
            {
                httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ApiKey);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.BaseAddress = new Uri(string.Format("https://{0}.api.cognitive.microsoft.com/", ApiKeyRegion));
            }
        }

        public static async Task<SentimentResult> GetTextSentimentAsync(string[] input, string language = "en")
        {
            SentimentResult sentimentResult = new SentimentResult() { Scores = new double[] { 0.5 } };

            if (input != null)
            {
                string requestString = "{\"documents\":[";
                for (int i = 0; i < input.Length; i++)
                {
                    requestString += string.Format("{{\"id\":\"{0}\",\"text\":\"{1}\", \"language\":\"{2}\"}}", i, input[i].Replace("\"", "'"), language);
                    if (i != input.Length - 1)
                    {
                        requestString += ",";
                    }
                }

                requestString += "]}";

                byte[] byteData = Encoding.UTF8.GetBytes(requestString);

                string uri = "text/analytics/v2.0/sentiment";
                var response = await CallEndpoint(httpClient, uri, byteData);
                string content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Text Analytics failed. " + content);
                }
                dynamic data = JObject.Parse(content);
                Dictionary<int, double> scores = new Dictionary<int, double>();
                if (data.documents != null)
                {
                    for (int i = 0; i < data.documents.Count; i++)
                    {
                        scores[(int)data.documents[i].id] = data.documents[i].score;
                    }
                }

                if (data.errors != null)
                { 
                    for (int i = 0; i < data.errors.Count; i++)
                    {
                        scores[(int)data.errors[i].id] = 0.5;
                    }
                }

                sentimentResult = new SentimentResult { Scores = scores.OrderBy(s => s.Key).Select(s => s.Value) };
            }

            return sentimentResult;
        }

        public static async Task<KeyPhrasesResult> GetKeyPhrasesAsync(string[] input, string language = "en")
        {
            KeyPhrasesResult result = new KeyPhrasesResult() { KeyPhrases = Enumerable.Empty<IEnumerable<string>>() };

            if (input != null)
            {
                string requestString = "{\"documents\":[";
                for (int i = 0; i < input.Length; i++)
                {
                    requestString += string.Format("{{\"id\":\"{0}\",\"text\":\"{1}\", \"language\":\"{2}\"}}", i, input[i].Replace("\"", "'"), language);
                    if (i != input.Length - 1)
                    {
                        requestString += ",";
                    }
                }

                requestString += "]}";

                byte[] byteData = Encoding.UTF8.GetBytes(requestString);

                string uri = "text/analytics/v2.0/keyPhrases";
                var response = await CallEndpoint(httpClient, uri, byteData);
                string content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Text Analytics failed. " + content);
                }
                dynamic data = JObject.Parse(content);
                Dictionary<int, IEnumerable<string>> phrasesDictionary = new Dictionary<int, IEnumerable<string>>();
                if (data.documents != null)
                {
                    for (int i = 0; i < data.documents.Count; i++)
                    {
                        List<string> phrases = new List<string>();

                        for (int j = 0; j < data.documents[i].keyPhrases.Count; j++)
                        {
                            phrases.Add((string)data.documents[i].keyPhrases[j]);
                        }
                        phrasesDictionary[i] = phrases;
                    }
                }

                if (data.errors != null)
                {
                    for (int i = 0; i < data.errors.Count; i++)
                    {
                        phrasesDictionary[i] = Enumerable.Empty<string>();
                    }
                }

                result.KeyPhrases = phrasesDictionary.OrderBy(e => e.Key).Select(e => e.Value);
            }

            return result;
        }

        static async Task<HttpResponseMessage> CallEndpoint(HttpClient client, string uri, byte[] byteData)
        {
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return await client.PostAsync(uri, content);
            }
        }
    }

    public class SentimentResult
    {
        public IEnumerable<double> Scores { get; set; }
    }

    public class KeyPhrasesResult
    {
        public IEnumerable<IEnumerable<string>> KeyPhrases { get; set; }
    }

}
