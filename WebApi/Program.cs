using Microsoft.EntityFrameworkCore;
using Application.Services;
using DataModel.Repository;
using DataModel.Mapper;
using Domain.Factory;
using Domain.IRepository;
using WebApi.Controllers;
using Gateway;
using RabbitMQ.Client;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

string replicaNameArg = Array.Find(args, arg => arg.Contains("--replicaName"));
string replicaName;
if (replicaNameArg != null)
    replicaName = replicaNameArg.Split('=')[1];
else
    replicaName = config.GetConnectionString("replicaName");


var queueName = config["Commands:" + replicaName];

//var port = getPort(holidayQueueName);

var port = config["Ports:" + replicaName];

var rabbitMqHost = config["RabbitMq:Host"];
var rabbitMqPort = config["RabbitMq:Port"];
var rabbitMqUser = config["RabbitMq:UserName"];
var rabbitMqPass = config["RabbitMq:Password"];

var DBConnectionString = config.GetConnectionString("DefaultConnection");

// Add services to the container.

builder.Services.AddControllers();

// builder.Services.AddDbContext<AbsanteeContext>(opt =>
//     //opt.UseInMemoryDatabase("AbsanteeList")
//     //opt.UseSqlite("Data Source=AbsanteeDatabase.sqlite")
//     opt.UseSqlite(Host.CreateApplicationBuilder().Configuration.GetConnectionString(queueName))
// );

builder.Services.AddDbContext<AbsanteeContext>(option =>
{
    option.UseNpgsql(DBConnectionString);
}, optionsLifetime: ServiceLifetime.Scoped);

// builder.Services.AddDbContextFactory<AbsanteeContext>(options =>
// {
//     options.UseNpgsql(DBConnectionString);
// });

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
        HostName = rabbitMqHost,
        Port = int.Parse(rabbitMqPort),
        UserName = rabbitMqUser,
        Password = rabbitMqPass
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

var rabbitMQConsumerServices = app.Services.GetServices<IRabbitMQConsumerController>();
foreach (var service in rabbitMQConsumerServices)
{
    service.ConfigQueue(queueName);
    service.StartConsuming();
};

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAllOrigins");

app.UseHttpsRedirection(); 

app.UseAuthorization();

app.MapControllers();

app.Run($"https://localhost:{port}");
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