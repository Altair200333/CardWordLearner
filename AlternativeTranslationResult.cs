using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WordLearner
{
    public class Translations
    {
        [JsonProperty("confidence")]
        public float confidence;

        [JsonProperty("displayTarget")]
        public string displayTarget;
    }
    public class AlternativeTranslationResult
    {
        [JsonProperty("displaySource")]
        public string displaySource;

        [JsonProperty("normalizedSource")]
        public string normalizedSource;

        [JsonProperty("translations")]
        public List<Translations> translations;

    }
}
