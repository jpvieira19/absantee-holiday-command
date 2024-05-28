namespace DataModel.Repository;
using System.Text;
using RabbitMQ.Client;
public class HolidayAmpqGateway
{
    private readonly IConnectionFactory _factory;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    public HolidayAmpqGateway(IConnectionFactory factory)
    {
        _factory = factory;
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(exchange: "holiday_logs", type: ExchangeType.Fanout);
    }
 
    public void Publish(string holiday)
    {
        var body = Encoding.UTF8.GetBytes(holiday);
        _channel.BasicPublish(exchange: "holiday_logs",
                              routingKey: string.Empty,
                              basicProperties: null,
                              body: body);
    }
 
}