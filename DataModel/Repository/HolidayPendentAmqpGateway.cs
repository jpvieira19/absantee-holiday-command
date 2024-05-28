namespace DataModel.Repository;
using System.Text;
using RabbitMQ.Client;
public class HolidayPendentAmqpGateway
{
    private readonly IConnectionFactory _factory;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    public HolidayPendentAmqpGateway(IConnectionFactory factory)
    {
        _factory = factory;
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(exchange: "holidayToValidate", type: ExchangeType.Fanout);
    }
    
    public void PublishNewHolidayPending(string holiday)
    {
        var body = Encoding.UTF8.GetBytes(holiday);
        _channel.BasicPublish(exchange: "holidayToValidate",
                              routingKey: string.Empty,
                              basicProperties: null,
                              body: body);
    }
 
 
}