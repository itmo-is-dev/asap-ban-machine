using Itmo.Dev.Asap.BanMachine.Application.Extensions;
using Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Extensions;
using Itmo.Dev.Asap.BanMachine.Infrastructure.Persistence.Extensions;
using Itmo.Dev.Asap.BanMachine.Presentation.Grpc.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Logging.Extensions;
using Itmo.Dev.Platform.YandexCloud.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();
await builder.AddYandexCloudConfigurationAsync();

builder.Services
    .AddApplication()
    .AddInfrastructureMachineLearning()
    .AddInfrastructurePersistence()
    .AddPresentationGrpc();

builder.Services.AddPlatformBackgroundTasks(configurator => configurator
    .ConfigurePersistence(builder.Configuration.GetSection("Infrastructure:BackgroundTasks:Persistence"))
    .ConfigureScheduling(builder.Configuration.GetSection("Infrastructure:BackgroundTasks:Scheduling"))
    .ConfigureExecution(builder.Configuration.GetSection("Infrastructure:BackgroundTasks:Execution"))
    .AddApplicationBackgroundTasks());

builder.Host.AddPlatformSerilog(builder.Configuration);
builder.Services.AddUtcDateTimeProvider();

WebApplication app = builder.Build();

app.UseGrpcPresentation();

await app.RunAsync();