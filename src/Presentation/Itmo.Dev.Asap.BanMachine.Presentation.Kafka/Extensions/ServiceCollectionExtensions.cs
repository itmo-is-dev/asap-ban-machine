using Itmo.Dev.Asap.Kafka;
using Itmo.Dev.Platform.Kafka.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.BanMachine.Presentation.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationKafka(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        const string producerKey = "Presentation:Kafka:Producers";

        collection.AddPlatformKafka(builder => builder
            .ConfigureOptions(configuration.GetSection("Presentation:Kafka"))
            .AddProducer(b => b
                .WithKey<BanMachineAnalysisKey>()
                .WithValue<BanMachineAnalysisValue>()
                .WithConfiguration(configuration.GetSection($"{producerKey}:BanMachineAnalysis"))
                .SerializeKeyWithProto()
                .SerializeValueWithProto()));

        return collection;
    }
}