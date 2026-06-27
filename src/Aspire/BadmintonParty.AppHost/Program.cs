using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", secret: true);
var postgres = builder.AddPostgres("postgres", password: postgresPassword)
    .WithHostPort(5432)
    .WithPgAdmin()
    .WithDataVolume();

var db = postgres.AddDatabase("badminton-party");

var api = builder.AddProject<Projects.BadmintonParty_Liff_Web_Api>("api")
    .WithReference(db);

var webhook = builder.AddProject<Projects.BadmintonParty_Webhook>("webhook")
    .WithReference(db);

builder.AddNpmApp("frontend", "../../WebApps/badminton-party", "start")
    .WithReference(api)
    .WithEndpoint(port: 4200, targetPort: 4201, scheme: "https", name: "https")
    .WithExternalHttpEndpoints();

builder.Build().Run();
