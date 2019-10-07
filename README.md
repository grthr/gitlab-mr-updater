# Integrate Sonarqube with Gitlab Merge Requests

## Building this docker image

```bash
docker build -t gitlab-sonarqube-webhook .
```

## Running this docker image

```bash
docker run -d -p 8088:80 -e GitlabBaseUrl="https://my.gitlab.url" -e SonarqubeBaseUrl="https://my.sonarqube.url" gitlab-sonarqube-webhook
```
