using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endeavor.Steps;
using Endeavor.Steps.Core;
using Endeavor.Worker.Messaging;
using Endeavor.Worker.Persistence;
using Endeavor.Worker.Service.Persistence;
using Keryhe.Messaging.RabbitMQ.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Endeavor.Worker.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();

                    services.AddSingleton<IConnectionFactory, ConnectionFactory>();
                    services.AddTransient<IRepository, WorkerRepository>();

                    services.AddRabbitMQListener<TaskToBeWorked>(hostContext.Configuration.GetSection("RabbitMQListener"));
                    
                    services.AddTransient<Func<string, IStep>>(sp => stepType =>
                    {
                        switch (stepType)
                        {
                            case "StartStep":
                                return new StartStep();
                            case "ManualStep":
                                return new ManualStep();
                            case "DecisionStep":
                                return new DecisionStep();
                            case "EndStep":
                                return new EndStep();
                            default:
                                throw new Exception(stepType + " not found");
                        }
                    });

                    services.AddHostedService<Worker>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });
    }
}
