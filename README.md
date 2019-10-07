# Integrate Sonarqube with Gitlab Merge Requests

This project is responsible for integrating Gitlab Merge Requests with SonarQube Branch Anaalysis.

The single API endpoint is compatible with [Merge request event](https://docs.gitlab.com/ee/user/project/integrations/webhooks.html#merge-request-events) from Gitlab Webhook.
If a new merge request is created (not updated), the service adds a new thread to the new merge request which contains the user interface integration to SonarQube Branch Analysis.

The Gitlab project needs to have a project environment variable named `SONARQUBE_PROJECT_ID` containing a valid Sonarqube Project ID.

This service uses the provided Gitlab Webhook Token for authentication with the Gitlab API (API Token pass-through). Make sure to set a valid Token with API scope in the Webhook configuration.

Make sure your Gitlab CI pipeline runs Sonarqube Scanner with the branch name (this is a feature of the Developer Edition of Sonarqube)

## Building this docker image

```bash
docker build -t gitlab-sonarqube-webhook .
```

## Running this docker image

```bash
docker run -d -p 8088:80 -e GitlabBaseUrl="https://my.gitlab.url" -e SonarqubeBaseUrl="https://my.sonarqube.url" gitlab-sonarqube-webhook
```
