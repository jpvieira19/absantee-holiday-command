using RabbitMQ.Client;

using Application.DTO;
using RabbitMQ.Client.Events;
using System.Text;
using Application.Services;

namespace WebApi.Controllers
{
    public class RabbitMQHolidayPendingConsumerController : IRabbitMQConsumerController
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        private List<string> _errorMessages = new List<string>();
        private string _queueName;
 
        public RabbitMQHolidayPendingConsumerController(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _factory = new ConnectionFactory { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
 
            _channel.ExchangeDeclare(exchange: "holidayToValidate", type: ExchangeType.Fanout);
 
            Console.WriteLine(" [*] Waiting for messages from pending holidays.");


        }

        
 
       
 
 
        public void StartConsuming()
        {
            Console.WriteLine(" h");
            
            var consumer = new EventingBasicConsumer(_channel);
            
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                HolidayDTO holidayAmpqDTO = HolidayGatewayDTO.Deserialize(message);
                Console.WriteLine($" [x] Received {message}");
                //_channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                using (var scope = _scopeFactory.CreateScope()){
                var holidayPendingService = scope.ServiceProvider.GetRequiredService<HolidayPendingService>();
                await holidayPendingService.Add(holidayAmpqDTO, _errorMessages);
                };
            };
            _channel.BasicConsume(queue: _queueName,
                                autoAck: true,
                                consumer: consumer);
            
       
        }

        public void ConfigQueue(string queueName)
        {
            _queueName = queueName;

            _channel.QueueDeclare(queue: _queueName,
                                            durable: true,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

            _channel.QueueBind(queue: _queueName, 
                  exchange: "holidayToValidate",
                  routingKey: string.Empty);
        }
 
        public void StopConsuming()
        {
            throw new NotImplementedException();
        }
    }
}