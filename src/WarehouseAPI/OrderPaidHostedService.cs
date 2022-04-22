using Azure.Messaging.ServiceBus;
using MessageContracts;
using System.Text.Json;

namespace WarehouseAPI
{
    public class OrderPaidHostedService : IHostedService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<OrderPaidHostedService> _logger;
        private ServiceBusProcessor? _processor;

        public OrderPaidHostedService(ServiceBusClient serviceBusClient, ILogger<OrderPaidHostedService> logger)
        {
            _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _processor = _serviceBusClient.CreateProcessor("order-paid", "warehouse-api");

            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            await _processor.StartProcessingAsync(cancellationToken);

            _logger.LogInformation("Started listening for Order Paid messages");
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, "Failed to consume order placed event");
            return Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            var orderPaid = arg.Message.Body.ToObjectFromJson<OrderPaid>();
            _logger.LogInformation("Received Order #{orderId} paid from Service Bus", orderPaid.OrderID);


            /*
                Does its internal logic about warehousing, like reserving the product.
            */
            var cancellationToken = arg.CancellationToken;
            var orderShipped = new OrderShiped(
                orderPaid.OrderID
            );
            byte[] billOrderSerialized = JsonSerializer.SerializeToUtf8Bytes(orderShipped);
            var orderShippedSender = _serviceBusClient.CreateSender("order-shipped");
            await orderShippedSender.SendMessageAsync(new ServiceBusMessage(billOrderSerialized), cancellationToken);

            _logger.LogInformation("Sent Order #{orderId} shipped to Service Bus", orderPaid.OrderID);
        }

        public Task? StopAsync(CancellationToken cancellationToken)
        {
            return _processor?.CloseAsync(cancellationToken: cancellationToken);
        }
    }
}
