using Azure.Messaging.ServiceBus;
using MessageContracts;
using System.Text.Json;

namespace PaymentAPI
{
    public class ProcessCreditCardPaymentHostedService : IHostedService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<ProcessCreditCardPaymentHostedService> _logger;
        private ServiceBusProcessor? _processor;

        public ProcessCreditCardPaymentHostedService(ServiceBusClient serviceBusClient, ILogger<ProcessCreditCardPaymentHostedService> logger)
        {
            _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _processor = _serviceBusClient.CreateProcessor("order-placed", "billing-api-cc");

            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            await _processor.StartProcessingAsync(cancellationToken);

            _logger.LogInformation("Started listening for Order Placed messages to process credit card payments");
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, "Failed to consume order placed event while processing credit card payment");
            return Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            var orderPlaced = arg.Message.Body.ToObjectFromJson<OrderPlaced>();
            _logger.LogInformation("Received Order #{orderId} placed from Service Bus. Processing Credit Card Payment...", orderPlaced.OrderID);

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

            _logger.LogInformation("Credit Card Payment processing is done. Sent Order #{orderId} paid to Service Bus", orderPlaced.OrderID);
        }

        public Task? StopAsync(CancellationToken cancellationToken)
        {
            return _processor?.CloseAsync(cancellationToken: cancellationToken);
        }
    }
}
