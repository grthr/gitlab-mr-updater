using Newtonsoft.Json;

namespace GitlabSonarqubeWebhook.Models
{
    public class MergeRequestEvent
    {
        [JsonProperty("project")]
        [JsonRequired]
        public Project Project { get; set; }

        [JsonProperty("object_attributes")]
        [JsonRequired]
        public ObjectAttributes ObjectAttributes { get; set; }

        public bool IsNew => ObjectAttributes.CreatedAt == ObjectAttributes.UpdatedAt;
    }
    public class Project
    {
        [JsonProperty("id")]
        [JsonRequired]
        public int Id { get; set; }
    }

    public class ObjectAttributes
    {
        [JsonProperty("iid")]
        [JsonRequired]
        public int Iid { get; internal set; }

        [JsonProperty("source_branch")]
        [JsonRequired]
        public string SourceBranch { get; set; }

        [JsonProperty("created_at")]
        [JsonRequired]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        [JsonRequired]
        public string UpdatedAt { get; set; }
    }
}
