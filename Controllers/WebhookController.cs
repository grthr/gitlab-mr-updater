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

            using (var gitlabApiClient = CreateGitlabApiClient(gitlabApiToken))
            {
                var sonarqubeProjectId = await GetSonarqubeProjectIdAsync(gitlabApiClient, mergeRequestEvent.Project.Id);
                var comment = "Find results of [SonarQube Branch Analysis](" + SonarqubeBaseUrl + "/dashboard?id=" + sonarqubeProjectId + "&branch=" + mergeRequestEvent.ObjectAttributes.SourceBranch + ")";
                var result = await CreateMergeRequestComment(gitlabApiClient, comment, mergeRequestEvent);
                return new CreatedResult("", result);
            }
        }

        private HttpClient CreateGitlabApiClient(string gitlabApiToken)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Private-Token", gitlabApiToken);
            return client;
        }

        private Uri CreateMergeRequestCommentUri(string comment, int projectId, int mergeRequestIid)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["body"] = comment;

            var builder = new UriBuilder(GitlabBaseUrl)
            {
                Path = "/api/v4/projects/" + projectId + "/merge_requests/" + mergeRequestIid + "/discussions",
                Query = query.ToString()
            };

            return builder.Uri;
        }

        private async Task<string> CreateMergeRequestComment(HttpClient gitlabApiClient, string comment, MergeRequestEvent mergeRequestEvent)
        {
            var requestUri = CreateMergeRequestCommentUri(comment, mergeRequestEvent.Project.Id, mergeRequestEvent.ObjectAttributes.Iid);

            var response = await gitlabApiClient.PostAsync(requestUri, new StringContent(""));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> GetSonarqubeProjectIdAsync(HttpClient gitlabApiClient, int projectId)
        {
            var response = await gitlabApiClient.GetAsync(GitlabBaseUrl + "/api/v4/projects/" + projectId + "/variables/SONARQUBE_PROJECT_ID");
            response.EnsureSuccessStatusCode();
            var variableDetails = await response.Content.ReadAsAsync<VariableDetails>();
            return variableDetails.Value;
        }

        private Uri CreateRequestUri(MergeRequestEvent mergeRequestEvent, string sonarqubeProjectId)
        {
            var value = "Find results of [SonarQube Branch Analysis](" + SonarqubeBaseUrl + "/dashboard?id=" + sonarqubeProjectId + "&branch=" + mergeRequestEvent.ObjectAttributes.SourceBranch + ")";

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["body"] = value;

            var builder = new UriBuilder(GitlabBaseUrl)
            {
                Path = "/api/v4/projects/" + mergeRequestEvent.Project.Id + "/merge_requests/" + mergeRequestEvent.ObjectAttributes.Iid + "/discussions",
                Query = query.ToString()
            };

            return builder.Uri;
        }
    }
}
