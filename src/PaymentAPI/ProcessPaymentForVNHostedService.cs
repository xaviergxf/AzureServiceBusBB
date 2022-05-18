using Azure.Messaging.ServiceBus;
using MessageContracts;
using System.Text.Json;

namespace PaymentAPI
{
    public class ProcessPaymentForVNHostedService : IHostedService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<ProcessPaymentForVNHostedService> _logger;
        private ServiceBusProcessor? _processor;

        public ProcessPaymentForVNHostedService(ServiceBusClient serviceBusClient, ILogger<ProcessPaymentForVNHostedService> logger)
        {
            _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _processor = _serviceBusClient.CreateProcessor("order-placed", "billing-api-vn");

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
            _logger.LogInformation("Received Order #{orderId} placed from Service Bus. Processing Vietnam Payment...", orderPlaced.OrderID);

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

            _logger.LogInformation("Payment processing for Vietnam is done. Sent Order #{orderId} paid to Service Bus", orderPlaced.OrderID);
        }

        public Task? StopAsync(CancellationToken cancellationToken)
        {
            return _processor?.CloseAsync(cancellationToken: cancellationToken);
        }
    }
}
