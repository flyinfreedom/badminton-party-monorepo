var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var db = postgres.AddDatabase("badminton-party");

var api = builder.AddProject<Projects.BadmintonParty_Liff_Web_Api>("api")
    .WithReference(db);

builder.AddNpmApp("frontend", "../../WebApps/badminton-party", "start")
    .WithReference(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
