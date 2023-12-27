using Itmo.Dev.Asap.BanMachine.Application.Extensions;
using Itmo.Dev.Asap.BanMachine.Infrastructure.Persistence.Extensions;
using Itmo.Dev.Asap.BanMachine.Presentation.Grpc.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructurePersistence()
    .AddInfrastructurePersistence()
    .AddPresentationGrpc();

WebApplication app = builder.Build();

await app.RunAsync();