using Application.DTO;
using Application.Services;
using DataModel.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
namespace WebApi.Controllers
{
    public class RabbitMQHolidayPendingResponseConsumerController : IRabbitMQConsumerController{
        private List<string> _errorMessages = new List<string>();
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private string _queueName;

        public RabbitMQHolidayPendingResponseConsumerController(IServiceScopeFactory serviceScopeFactory){
            _serviceScopeFactory = serviceScopeFactory;
            _factory = new ConnectionFactory { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();


            _channel.ExchangeDeclare(exchange: "holidayPendentResponse", type: ExchangeType.Fanout);

            Console.WriteLine(" [*] Waiting for response from association with Holiday valid or not.");
        }

        public void ConfigQueue(string queueName)
        {
            _queueName = "pending" + queueName;

            _channel.QueueDeclare(queue: _queueName,
                                            durable: true,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

            _channel.QueueBind(queue: _queueName,
                  exchange: "holidayPendentResponse",
                  routingKey: string.Empty);
        }

        public void StartConsuming(){
            
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine($"Holiday.");
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                HolidayAmqpDTO holidayDTO = HolidayAmqpDTO.Deserialize(message);

                if (holidayDTO._status=="Not Okay"){

                    using (var scope = _serviceScopeFactory.CreateScope()){
                        var holidayPendingService = scope.ServiceProvider.GetRequiredService<HolidayPendingService>();
                        await holidayPendingService.Remove(holidayDTO.Id);
                    }
                    Console.WriteLine("Received 'Not Ok' message. No action required.");
                }
                
                else if (holidayDTO._status == "Ok")
                {
                    HolidayDTO holidayDTO1 = new HolidayDTO(holidayDTO._colabId, holidayDTO.Id, holidayDTO._holidayPeriod);

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AbsanteeContext>(); // Obtém o contexto do banco de dados

                        using (var transaction = await dbContext.Database.BeginTransactionAsync()) // Certifique-se de usar async/await aqui
                        {
                            try
                            {
                                var holidayPendingService = scope.ServiceProvider.GetRequiredService<HolidayPendingService>();
                                await holidayPendingService.Remove(holidayDTO.Id); // Operação assíncrona

                                var holidayService = scope.ServiceProvider.GetRequiredService<HolidayService>();
                                await holidayService.Add(holidayDTO1, _errorMessages); // Operação assíncrona

                                await dbContext.SaveChangesAsync(); // Salva as mudanças de forma assíncrona
                                await transaction.CommitAsync(); // Completa a transação de forma assíncrona

                                Console.WriteLine($"Received 'Ok' message and processed it: {message}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error processing message: {ex.Message}");
                                await transaction.RollbackAsync(); // Reverte a transação de forma assíncrona em caso de erro
                            }
                        }
                    }

                    Console.WriteLine($"Received 'Ok' message and processed it: {message}");
                }

            };
            _channel.BasicConsume(queue: _queueName,
                                autoAck: true,
                                consumer: consumer);
        }
    }
}