﻿using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.RabbitMQ;
using Serilog.Core;
using Common.Logging.RabbitMQ;
using Microsoft.Extensions.Configuration;

namespace Common.Logging;

public static class SeriLogger
{
    public static Logger CustomLoggerConfiguration(IConfiguration configuration)
    {
        RabbitMqConfigModel rabbitMqConfigModel = new();
        configuration.Bind("RabbitMqConfig", rabbitMqConfigModel);

        return new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Information)
            .MinimumLevel.Override("Swashbuckle.AspNetCore", LogEventLevel.Error)
            .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker", LogEventLevel.Error)
            .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Infrastructure.ObjectResultExecutor", LogEventLevel.Error)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing.EndpointMiddleware", LogEventLevel.Error)
            .MinimumLevel.Override("Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware", LogEventLevel.Error)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Error)
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.RabbitMQ((clientConfiguration, sinkConfiguration) =>
            {
                clientConfiguration.Username = rabbitMqConfigModel.Username;
                clientConfiguration.Password = rabbitMqConfigModel.Password;
                clientConfiguration.Exchange = rabbitMqConfigModel.Exchange;
                clientConfiguration.ExchangeType = rabbitMqConfigModel.ExchangeType;
                clientConfiguration.DeliveryMode = RabbitMQDeliveryMode.Durable;
                clientConfiguration.RoutingKey = "LoggerQueue";
                clientConfiguration.Port = rabbitMqConfigModel.Port;
                clientConfiguration.VHost = rabbitMqConfigModel.VHostname;
                clientConfiguration.Hostnames.Add(rabbitMqConfigModel.Hostname);
                sinkConfiguration.RestrictedToMinimumLevel = LogEventLevel.Information;
                sinkConfiguration.TextFormatter = new JsonFormatter();
            })
            .CreateLogger();
    }
}