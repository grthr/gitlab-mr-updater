using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using GitlabSonarqubeWebhook.Models;
using Microsoft.AspNetCore.Mvc;

namespace GitlabSonarqubeWebhook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private static readonly string GitlabBaseUrl = Environment.GetEnvironmentVariable("GitlabBaseUrl");
        private static readonly string SonarqubeBaseUrl = Environment.GetEnvironmentVariable("SonarqubeBaseUrl");

        [HttpPost]
        public async Task<IActionResult> Post([FromHeader(Name = "X-Gitlab-Token")] string gitlabApiToken, [FromBody] MergeRequestEvent mergeRequestEvent)
        {
            if (mergeRequestEvent == null)
            {
                throw new ArgumentException("merge request event may not be null.");
            }

            if (!mergeRequestEvent.IsNew)
            {
                return new NoContentResult();
            }

            var sonarqubeProjectId = await GetSonarqubeProjectIdAsync(gitlabApiToken, mergeRequestEvent.Project.Id);
            var request = CreateRequest(gitlabApiToken, mergeRequestEvent, sonarqubeProjectId);

            using (var client = new HttpClient())
            {
                var result = await client.SendAsync(request);
                if (result.StatusCode == HttpStatusCode.Created)
                {
                    return new CreatedResult("", await result.Content.ReadAsStringAsync());
                }
                return new BadRequestResult();
            }
        }

        private async Task<string> GetSonarqubeProjectIdAsync(string gitlabApiToken, int projectId)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Private-Token", gitlabApiToken);
                var response = await client.GetAsync(GitlabBaseUrl + "/api/v4/projects/" + projectId + "/variables/SONARQUBE_PROJECT_ID");
                response.EnsureSuccessStatusCode();
                var variableDetails = await response.Content.ReadAsAsync<VariableDetails>();
                return variableDetails.Value;
            }
        }

        private HttpRequestMessage CreateRequest(string gitlabApiToken, MergeRequestEvent mergeRequestEvent, string sonarqubeProjectId)
        {
            var value = "Find results of [SonarQube Branch Analysis](" + SonarqubeBaseUrl + "/dashboard?id=" + sonarqubeProjectId + "&branch=" + mergeRequestEvent.ObjectAttributes.SourceBranch + ")";

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["body"] = value;

            var builder = new UriBuilder(GitlabBaseUrl)
            {
                Path = "/api/v4/projects/" + mergeRequestEvent.Project.Id + "/merge_requests/" + mergeRequestEvent.ObjectAttributes.Iid + "/discussions",
                Query = query.ToString()
            };

            return new HttpRequestMessage
            {
                RequestUri = builder.Uri,
                Method = HttpMethod.Post,
                Headers = { { "Private-Token", gitlabApiToken } },
            };
        }
    }
}
