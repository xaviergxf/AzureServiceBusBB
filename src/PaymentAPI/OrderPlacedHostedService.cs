﻿using Azure.Messaging.ServiceBus;
using MessageContracts;
using System.Text.Json;

namespace PaymentAPI
{
    public class OrderPlacedHostedService : IHostedService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<OrderPlacedHostedService> _logger;
        private ServiceBusProcessor? _processor;

        public OrderPlacedHostedService(ServiceBusClient serviceBusClient, ILogger<OrderPlacedHostedService> logger)
        {
            _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _processor = _serviceBusClient.CreateProcessor("order-placed", "billing-api");

            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            await _processor.StartProcessingAsync(cancellationToken);

            _logger.LogInformation("Started listening for Order Placed messages");
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, "Failed to consume order placed event");
            return Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            var orderPlaced = arg.Message.Body.ToObjectFromJson<OrderPlaced>();
            _logger.LogInformation("Received Order #{orderId} placed from Service Bus", orderPlaced.OrderID);

            /*
                var paymentInfo = orderPlaced.PaymentInfo;

                Does its internal logic about billing, like contacting an payment api.
            */
            var cancellationToken = arg.CancellationToken;
            var orderPayed = new OrderPaid(
                orderPlaced.OrderID,
                DateTimeOffset.Now
            );
            byte[] billOrderSerialized = JsonSerializer.SerializeToUtf8Bytes(orderPayed);
            var orderPayedSender = _serviceBusClient.CreateSender("order-paid");
            await orderPayedSender.SendMessageAsync(new ServiceBusMessage(billOrderSerialized), cancellationToken);

            _logger.LogInformation("Sent Order #{orderId} paid to Service Bus", orderPlaced.OrderID);
        }

        public Task? StopAsync(CancellationToken cancellationToken)
        {
            return _processor?.CloseAsync(cancellationToken: cancellationToken);
        }
    }
}
