namespace Deepin.EventBus.Events;
public record SendChatMessageIntegrationEvent(string ChatId, string MessageId) : IntegrationEvent;