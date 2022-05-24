using Azure.Messaging.ServiceBus;
using MessageContracts;
using System.Text.Json;

namespace PaymentAPI
{
    public class ProcessBitcoinPaymentHostedService : IHostedService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<ProcessBitcoinPaymentHostedService> _logger;
        private ServiceBusProcessor? _processor;

        public ProcessBitcoinPaymentHostedService(ServiceBusClient serviceBusClient, ILogger<ProcessBitcoinPaymentHostedService> logger)
        {
            _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _processor = _serviceBusClient.CreateProcessor("order-placed", "billing-api-btc");

            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            await _processor.StartProcessingAsync(cancellationToken);

            _logger.LogInformation("Started listening for Order Placed messages to process bitcoin payments");
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, "Failed to consume order placed event while processing bitcoin payments");
            return Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            var orderPlaced = arg.Message.Body.ToObjectFromJson<OrderPlaced>();
            _logger.LogInformation("Received Order #{orderId} placed from Service Bus. Processing BTC Payment...", orderPlaced.OrderID);

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

            _logger.LogInformation("BTC Payment processing is done. Sent Order #{orderId} paid to Service Bus", orderPlaced.OrderID);
        }

        public Task? StopAsync(CancellationToken cancellationToken)
        {
            return _processor?.CloseAsync(cancellationToken: cancellationToken);
        }
    }
}
