# WeatherPix

WeatherPix is a serverless cloud application that retrieves current weather data from Dutch weather stations, generates weather-based images asynchronously, stores the generated results in Azure, and exposes job status and result endpoints through a secured HTTP API.

The project was built as part of a university cloud computing assignment and focuses on asynchronous processing, serverless architecture, secure cloud storage, infrastructure as code, CI/CD, and OAuth2-based API security.

---

## Features

- Creates asynchronous weather image generation jobs
- Retrieves live weather station data from Buienradar
- Uses Pexels to retrieve location/weather-related background images
- Generates weather cards using SixLabors ImageSharp
- Processes stations asynchronously through Azure Service Bus
- Stores generated images in private Azure Blob Storage
- Tracks job and station progress in Azure Table Storage
- Returns temporary read-only SAS URLs for generated images
- Protects HTTP endpoints using OAuth2 access tokens
- Supports scoped authorization
- Deploys Azure infrastructure using Bicep
- Uses GitHub Actions for validation, security scanning, and deployment
- Supports separate development and production environments

---

## Architecture

```mermaid
flowchart TD
    Client[Client]

    CreateJob[Create Job Function]
    Status[Get Job Status Function]
    Results[Get Job Results Function]

    StartQueue[Service Bus: start-jobs]
    ImageQueue[Service Bus: image-jobs]

    StartJob[Start Job Function]
    GenerateImage[Generate Weather Image Function]

    Buienradar[Buienradar API]
    Pexels[Pexels API]

    Table[(Azure Table Storage)]
    Blob[(Azure Blob Storage)]
    KeyVault[Azure Key Vault]

    Auth0[Auth0 OAuth2]

    Client -->|OAuth2 access token| CreateJob
    Client -->|OAuth2 access token| Status
    Client -->|OAuth2 access token| Results

    Auth0 -->|JWT validation| CreateJob
    Auth0 -->|JWT validation| Status
    Auth0 -->|JWT validation| Results

    CreateJob --> Table
    CreateJob --> StartQueue

    StartQueue --> StartJob
    StartJob --> Buienradar
    StartJob --> Table
    StartJob --> ImageQueue

    ImageQueue --> GenerateImage
    GenerateImage --> Pexels
    GenerateImage --> Blob
    GenerateImage --> Table

    Results --> Blob
    Status --> Table

    KeyVault --> GenerateImage