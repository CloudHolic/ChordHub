using Microsoft.Extensions.Hosting;

using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// DB & Cache
var postgres = builder.AddPostgres("postgres")
    .WithEnvironment("POSTGRES_DB", "chordhub");
var redis = builder.AddRedis("redis");

// Python Microservice
var analysisService = builder.AddUvApp("analysis-service", "../ChordHub.Service.Analysis", "main.py")
    .WithReference(redis)
    .WithHttpEndpoint(env: "HTTP_PORT")
    .WithHttpsEndpoint(env: "HTTPS_PORT");

// Internal API Server
var internalApi = builder.AddProject<ChordHub_Api_Internal>("internal-api")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(analysisService)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

// Public API Server
var publicApi = builder.AddProject<ChordHub_Api_Public>("public-api")
    .WithReference(postgres)
    .WithReference(redis);

// Discord Bot Service
var discordService = builder.AddProject<ChordHub_Service_Discord>("discord-service")
    .WithReference(redis)
    .WithReference(internalApi);

// Next.js Frontend
var web = builder.AddNpmApp("web", "../ChordHub.Web", "dev")
    .WithReference(internalApi)
    .WithEnvironment("NEXT_PUBLIC_API_URL", internalApi.GetEndpoint("http"))
    .WithHttpEndpoint(env: "HTTP_PORT")
    .WithHttpsEndpoint(env: "HTTPS_PORT");

if (builder.Environment.IsDevelopment())
    web.WithEnvironment("BROWSER", "chrome");

var app = builder.Build();
await app.RunAsync();
