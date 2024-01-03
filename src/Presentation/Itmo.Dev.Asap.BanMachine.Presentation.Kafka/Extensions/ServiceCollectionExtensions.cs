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

        collection.AddKafka(builder => builder
            .ConfigureOptions(b => b.BindConfiguration("Presentation:Kafka"))
            .AddProducer<BanMachineAnalysisKey, BanMachineAnalysisValue>(selector => selector
                .SerializeKeyWithProto()
                .SerializeValueWithProto()
                .UseNamedOptionsConfiguration(
                    "BanMachineAnalysis",
                    configuration.GetSection($"{producerKey}:BanMachineAnalysis"))));

        return collection;
    }
}