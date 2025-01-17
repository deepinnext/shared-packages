using Deepin.EventBus.Events;
using MassTransit;

namespace Deepin.EventBus;
public interface IIntegrationEventHandler<in TIntegrationEvent> : IConsumer<TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
{ }
public class SendEmailIntegrationEventHandler : IIntegrationEventHandler<SendEmailIntegrationEvent>
{
    public Task Consume(ConsumeContext<SendEmailIntegrationEvent> context)
    {
        throw new NotImplementedException();
    }
}
