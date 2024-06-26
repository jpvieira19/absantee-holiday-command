﻿using System.Data.Common;
using DataModel.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Controllers;
using Testcontainers.RabbitMq;

namespace WebApi.IntegrationTests;

public class IntegrationTestsWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
{
    private RabbitMqContainer _rabbitMqContainer;
    private string _rabbitHost;
    private int _rabbitPort;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        var configurationValues = new Dictionary<string, string>
        {
            { "replicaName", "repl1" },
            {"RabbitMq:Host", _rabbitHost},
            {"RabbitMq:Port", _rabbitPort.ToString()},
            {"RabbitMq:UserName", "guest"},
            {"RabbitMq:Password", "guest"}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        builder
            // This configuration is used during the creation of the application
            // (e.g. BEFORE WebApplication.CreateBuilder(args) is called in Program.cs).
            .UseConfiguration(configuration)
            .ConfigureAppConfiguration(configurationBuilder =>
            {
                // This overrides configuration settings that were added as part 
                // of building the Host (e.g. calling WebApplication.CreateBuilder(args)).
                configurationBuilder.AddInMemoryCollection(configurationValues);
        });

        builder.ConfigureServices((context, services) =>
        {


            // Remove the AppDbContext registration
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AbsanteeContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Remove the database connection registration
            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbConnection));

            if (dbConnectionDescriptor != null)
            {
                services.Remove(dbConnectionDescriptor);
            }

            // Add SQLite in-memory database
            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();
                return connection;
            });

            services.AddDbContext<AbsanteeContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });

            services.AddSingleton<IRabbitMQConsumerController, RabbitMQAssociationConsumerController>();
            services.AddSingleton<IRabbitMQConsumerController, RabbitMQColabConsumerController>();
            services.AddSingleton<IRabbitMQConsumerController, RabbitMQHolidayCreatedConsumerController>();
            services.AddSingleton<IRabbitMQConsumerController, RabbitMQHolidayPendingConsumerController>();
            services.AddSingleton<IRabbitMQConsumerController, RabbitMQHolidayPendingResponseConsumerController>();

        });

        builder.UseEnvironment("Development");
    }

    public async Task InitializeAsync()
    {
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.13-management")
            .WithPortBinding(5672, true)
            .WithPortBinding(15672, true)
            .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
            .Build();

        await _rabbitMqContainer.StartAsync();

        _rabbitHost = _rabbitMqContainer.Hostname;
        _rabbitPort = _rabbitMqContainer.GetMappedPublicPort(5672);

        await Task.Delay(10000);
    }

    public async Task DisposeAsync()
    {
        if (_rabbitMqContainer != null)
        {
            await _rabbitMqContainer.DisposeAsync();
        }
    }
}
