using Itmo.Dev.Asap.BanMachine.Application.Contracts.Analysis.Events;
using Itmo.Dev.Asap.Kafka;
using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.Producer;

namespace Itmo.Dev.Asap.BanMachine.Presentation.Kafka.EventHandlers;

internal class AnalysisCompletedEventHandler : IEventHandler<AnalysisCompletedEvent>
{
    private readonly IKafkaMessageProducer<BanMachineAnalysisKey, BanMachineAnalysisValue> _producer;

    public AnalysisCompletedEventHandler(IKafkaMessageProducer<BanMachineAnalysisKey, BanMachineAnalysisValue> producer)
    {
        _producer = producer;
    }

    public async ValueTask HandleAsync(AnalysisCompletedEvent evt, CancellationToken cancellationToken)
    {
        var key = new BanMachineAnalysisKey { AnalysisKey = evt.AnalysisId.ToString() };
        var value = new BanMachineAnalysisValue { AnalysisCompleted = new() };

        var message = new KafkaProducerMessage<BanMachineAnalysisKey, BanMachineAnalysisValue>(key, value);

        await _producer.ProduceAsync(message, cancellationToken);
    }
}