using Newtonsoft.Json;

namespace GitlabSonarqubeWebhook.Models
{
    public class VariableDetails
    {
        [JsonProperty("value")]
        [JsonRequired]
        public string Value { get; set; }
    }
}
