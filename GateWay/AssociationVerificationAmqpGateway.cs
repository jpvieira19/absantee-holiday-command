using System.Text;
using RabbitMQ.Client;

namespace Gateway
{
    public class AssociationVerificationAmqpGateway
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        public AssociationVerificationAmqpGateway()
        {
            _factory = new ConnectionFactory { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "associationPendentResponse", type: ExchangeType.Fanout);
        }

        public void Publish(string holiday)
        {
            var body = Encoding.UTF8.GetBytes(holiday);
            _channel.BasicPublish(exchange: "associationPendentResponse",
                                  routingKey: string.Empty,
                                  basicProperties: null,
                                  body: body);
        }
    }
}