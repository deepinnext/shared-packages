namespace Deepin.EventBus.Events;
public record SendEmailIntegrationEvent(string[] To, string Subject, string Body, bool IsBodyHtml = false, string[]? CC = null) : IntegrationEvent;