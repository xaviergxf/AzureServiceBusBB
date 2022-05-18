using Azure.Messaging.ServiceBus;
using MessageContracts;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrderingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ServiceBusClient serviceBusClient, ILogger<OrderController> logger)
        {
            _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Order order, CancellationToken cancellationToken = default)
        {
            //Does its internal logic about ordering, like storing in its database.
            var orderPlaced = new OrderPlaced(
                order.OrderID,
                order.CustomerID,
                order.OrderDate,
                order.Items.Select(s => new OrderPlacedItem(s.ProductID, s.ProductName, s.Price, s.Quantity)).ToList(),
                new OrderPlacedPaymentInfo(
                    order.CreditCardCountry,
                    order.CreditCardType, 
                    order.CreditCardNumber, 
                    order.CreditCardExpMonth, 
                    order.CreditCardExpYear, 
                    order.CreditCardCvc
                )
            );

            byte[] orderPlacedSerialized = JsonSerializer.SerializeToUtf8Bytes(orderPlaced);
            var orderSender = _serviceBusClient.CreateSender("order-placed");
            var serviceBusOrderPlacedMessage = new ServiceBusMessage(orderPlacedSerialized);
            serviceBusOrderPlacedMessage.ApplicationProperties.Add("country", order.CreditCardCountry);

            await orderSender.SendMessageAsync(serviceBusOrderPlacedMessage, cancellationToken);

            _logger.LogInformation("Sent Order #{orderId} placed to Service Bus", order.OrderID);

            return Accepted();
        }

    }
}
