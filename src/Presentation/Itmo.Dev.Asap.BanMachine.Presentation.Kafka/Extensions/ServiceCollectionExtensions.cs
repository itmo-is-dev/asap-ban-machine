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

        string host = configuration.GetSection("Presentation:Kafka:Host").Get<string>() ?? string.Empty;

        collection.AddKafkaProducer<BanMachineAnalysisKey, BanMachineAnalysisValue>(builder => builder
            .SerializeKeyWithProto()
            .SerializeValueWithProto()
            .UseNamedOptionsConfiguration(
                "BanMachineAnalysis",
                configuration.GetSection($"{producerKey}:BanMachineAnalysis"),
                c => c.WithHost(host)));

        return collection;
    }
}