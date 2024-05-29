using Application.Services;
using DataModel.Mapper;
using DataModel.Repository;
using Domain.Factory;
using Domain.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using WebApi.Controllers;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

string replicaNameArg = Array.Find(args, arg => arg.Contains("--replicaName"));
string replicaName;
if (replicaNameArg != null)
    replicaName = replicaNameArg.Split('=')[1];
else
    replicaName = config.GetConnectionString("replicaName");

config.GetConnectionString("replicaName");

replicaName = "Repl1";

var queueName = config["Commands:" + replicaName];
 
var port = config["Ports:" + replicaName];

Console.WriteLine("Environment Development: " + config["ASPNETCORE_ENVIRONMENT"]);
Console.WriteLine("DB_CONNECTION: " + config["DB_CONNECTION"]);

string dbConnectionString = config.DefineDbConnection();
Console.WriteLine("DBConnectionString: " + dbConnectionString);

RabbitMqConfiguration rabbitMqConfig = config.DefineRabbitMqConfiguration();
Console.WriteLine("RabbitMqConfig: " + rabbitMqConfig.Hostname);


builder.Services.AddControllers();

builder.Services.AddDbContext<AbsanteeContext>(option =>
{
    option.UseNpgsql(dbConnectionString);
}, ServiceLifetime.Scoped);

builder.Services.AddEndpointsApiExplorer();
 
builder.Services.AddSwaggerGen(opt =>
    opt.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date",
        Example = new OpenApiString(DateTime.Today.ToString("yyyy-MM-dd"))
    })
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    return new ConnectionFactory()
    {
        HostName = rabbitMqConfig.Hostname,
        UserName = rabbitMqConfig.Username,
        Password = rabbitMqConfig.Password,
        Port = int.Parse(rabbitMqConfig.port.ToString())
    };
});

builder.Services.AddTransient<IHolidayRepository, HolidayRepository>();
builder.Services.AddTransient<IHolidayFactory, HolidayFactory>();
builder.Services.AddTransient<HolidayMapper>();
builder.Services.AddTransient<HolidayService>();
builder.Services.AddTransient<HolidayPendingService>();
builder.Services.AddTransient<HolidayAmpqGateway>();
builder.Services.AddTransient<HolidayPendentAmqpGateway>();
builder.Services.AddTransient<AssociationVerificationAmqpGateway>();

builder.Services.AddSingleton<IHolidayPeriodFactory, HolidayPeriodFactory>();

builder.Services.AddSingleton<IRabbitMQConsumerController, RabbitMQAssociationConsumerController>();
builder.Services.AddTransient<AssociationService>();

builder.Services.AddTransient<IColaboratorsIdRepository, ColaboratorsIdRepository>();
builder.Services.AddTransient<IColaboratorIdFactory, ColaboratorIdFactory>();
builder.Services.AddTransient<ColaboratorsIdMapper>();
builder.Services.AddTransient<ColaboratorIdService>();
builder.Services.AddTransient<IRabbitMQConsumerController, RabbitMQColabConsumerController>();

builder.Services.AddSingleton<IRabbitMQConsumerController, RabbitMQHolidayPendingConsumerController>(); 

builder.Services.AddSingleton<IRabbitMQConsumerController, RabbitMQHolidayPendingResponseConsumerController>(); 

builder.Services.AddSingleton<IRabbitMQConsumerController, RabbitMQHolidayCreatedConsumerController>(); 

builder.Services.AddTransient<IHolidayPendingRepository, HolidayPendingRepository>();
builder.Services.AddTransient<HolidayPendingMapper>();
builder.Services.AddTransient<HolidayPendingService>();



var app = builder.Build();

Console.WriteLine("Environment Development? " + app.Environment.IsDevelopment());



var rabbitMQConsumerServices = app.Services.GetServices<IRabbitMQConsumerController>();
foreach (var service in rabbitMQConsumerServices)
{
    service.ConfigQueue(queueName);
    service.StartConsuming();
};

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
*/
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection(); 

app.UseAuthorization();

app.MapControllers();

Console.WriteLine("Running!");

app.Run();
//app.Run($"https://localhost:{port}");
/*
int getPort(string name)
{
    // Implement logic to map queue name to a unique port number
    // Example: Assign a unique port number based on the queue name suffix
    int basePort = 5090; // Start from port 5000
    int queueIndex = int.Parse(name.Substring(2)); // Extract the numeric part of the queue name (assuming it starts with 'Q')
    return basePort + queueIndex;
}*/
public partial class Program{ }