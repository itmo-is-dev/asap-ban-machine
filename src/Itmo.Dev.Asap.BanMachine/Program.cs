#pragma warning disable CA1506

using Itmo.Dev.Asap.BanMachine.Application.Extensions;
using Itmo.Dev.Asap.BanMachine.Infrastructure.ContentLoader;
using Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Extensions;
using Itmo.Dev.Asap.BanMachine.Infrastructure.Persistence.Extensions;
using Itmo.Dev.Asap.BanMachine.Presentation.Grpc.Extensions;
using Itmo.Dev.Asap.BanMachine.Presentation.Kafka.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.Logging.Extensions;
using Itmo.Dev.Platform.YandexCloud.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();
await builder.AddYandexCloudConfigurationAsync();

builder.Services
    .AddApplication()
    .AddInfrastructureContentLoader()
    .AddInfrastructureMachineLearning()
    .AddInfrastructurePersistence()
    .AddPresentationGrpc()
    .AddPresentationKafka(builder.Configuration);

builder.Services.AddPlatformBackgroundTasks(configurator => configurator
    .ConfigurePersistence(builder.Configuration.GetSection("Infrastructure:BackgroundTasks:Persistence"))
    .ConfigureScheduling(builder.Configuration.GetSection("Infrastructure:BackgroundTasks:Scheduling"))
    .ConfigureExecution(builder.Configuration.GetSection("Infrastructure:BackgroundTasks:Execution"))
    .AddApplicationBackgroundTasks());

builder.Services.AddPlatformEvents(b => b
    .AddPresentationKafkaHandlers());

builder.Host.AddPlatformSerilog(builder.Configuration);
builder.Services.AddUtcDateTimeProvider();

builder.Services.AddControllers();

WebApplication app = builder.Build();

await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
{
    await scope.UsePlatformBackgroundTasksAsync(default);
}

app.UseRouting();
app.UsePresentationGrpc();

await app.RunAsync();
#pragma warning restore CA1506