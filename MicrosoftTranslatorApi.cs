using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WordLearner
{
    class MicrosoftTranslatorApi
    {
        private static string key = "c1207a4441224386aaced999f5aeb646";
        private static string location = "eastasia";
        private static readonly string endpoint = "https://api.cognitive.microsofttranslator.com/";

        private HttpClient client;
        private static MicrosoftTranslatorApi instance;

        public static void init()
        {
            getInstance();
        }
        private static MicrosoftTranslatorApi getInstance()
        {
            return instance ?? (instance = new MicrosoftTranslatorApi());
        }

        private MicrosoftTranslatorApi()
        {
            client = new HttpClient();
        }

        public static void Dispose()
        {
            var inst = getInstance();
            if (inst.client != null)
            {
                inst.client.Dispose();
            }
        }

        public static async Task<TranslatedWord> translate(string word, string from = "en", string to = "ru",
            float confidence = 0.35f)
        {
            TranslatedWord translation = new TranslatedWord();

            string route = $"/dictionary/lookup?api-version=3.0&from={from}&to={to}";
            object[] body = new object[] {new {Text = word}};

            var requestBody = JsonConvert.SerializeObject(body);

            var response = await sendRequest(route, requestBody).ConfigureAwait(false);

            var myobjList = JsonConvert.DeserializeObject<List<AlternativeTranslationResult>>(response);
            if (myobjList == null || myobjList.Count == 0)
                return null;

            var result = myobjList[0];

            translation.word = result.displaySource;
            translation.translations = result.translations.Where(x => x.confidence > confidence)
                .Select(x => x.displayTarget).ToList();

            if (translation.translations.Count == 0 && result.translations.Count > 0)
            {
                translation.translations.Add(result.translations[0].displayTarget);
            }
            return translation;
        }

        public static async Task<List<TranslatedWord>> translate(List<string> words, string from = "en",
            string to = "ru", float confidence = 0.15f)
        {
            List<TranslatedWord> translations = new List<TranslatedWord>();

            string route = $"/dictionary/lookup?api-version=3.0&from={from}&to={to}";

            object[] body = words.Select(x => new {Text = x}).ToArray();

            var requestBody = JsonConvert.SerializeObject(body);

            var response = await sendRequest(route, requestBody).ConfigureAwait(false);

            var myobjList = JsonConvert.DeserializeObject<List<AlternativeTranslationResult>>(response);
            if (myobjList == null || !myobjList.Any())
                return null;

            foreach (var translationResult in myobjList)
            {
                var translation = new TranslatedWord();
                translation.word = translationResult.displaySource;
                translation.translations = translationResult.translations.Where(x => x.confidence > confidence)
                    .Select(x => x.displayTarget).ToList();

                translations.Add(translation);
            }

            return translations;
        }

        private static async Task<string> sendRequest(string route, string requestBody)
        {
            var client = getInstance().client;
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);
                request.Headers.Add("Ocp-Apim-Subscription-Region", location);

                try
                {
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                catch (TaskCanceledException e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        //DEBUG METHOD
        public static async Task<int> def()
        {
            string route = "/translate?api-version=3.0&from=en&to=ru";
            string textToTranslate = "Dog";
            object[] body = new object[] {new {Text = textToTranslate}};
            var requestBody = JsonConvert.SerializeObject(body);

            var client = getInstance().client;
            //using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    // Build the request.
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(endpoint + route);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", key);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", location);

                    // Send the request and get response.
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    // Read response as a string.
                    string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Console.WriteLine(result);
                }

                route = "/dictionary/lookup?api-version=3.0&from=en&to=ru";
                body = new object[] {new {Text = "Dog"}};
                requestBody = JsonConvert.SerializeObject(body);
                return 0;
                using (var request = new HttpRequestMessage())
                {
                    // Build the request.
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(endpoint + route);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", key);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", location);

                    // Send the request and get response.
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    // Read response as a string.
                    string result = await response.Content.ReadAsStringAsync();

                    JArray jsonArray = JArray.Parse(result);
                    JObject data = JObject.Parse(jsonArray[0].ToString());

                    var myobjList = JsonConvert.DeserializeObject<List<AlternativeTranslationResult>>(result);

                    Console.WriteLine(result);
                }
            }

            return 0;
        }
    }
}