﻿using Deepin.Application.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Deepin.Application.Behaviors;
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling command {CommandName},request:{@command}", request.GetGenericTypeName(), request);
        var response = await next();
        _logger.LogInformation("Command {CommandName} handled, response:{@Response}", request.GetGenericTypeName(), response);

        return response;
    }
}
