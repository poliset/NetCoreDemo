﻿using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.MessageMutator;
using Warehouse.Azure;

namespace Warehouse.Subscriber.Azure
{
    class Program
    {
        static ILog log = LogManager.GetLogger(typeof(Program));

        static async Task Main(string[] args)
        {
            var endpointConfiguration = new EndpointConfiguration("Warehouse-Subscriber");
            endpointConfiguration.SendFailedMessagesTo("error");
            var asqConnectionString = Environment.GetEnvironmentVariable("NetCoreDemoAzureStorageQueueTransport");
            if (string.IsNullOrEmpty(asqConnectionString))
            {
                log.Info("Connection for Azure Storage Queue transport is missing or empty.");
            }
            
            log.Info("Using Azure Storage Queue Transport");
            var transport = endpointConfiguration.UseTransport<AzureStorageQueueTransport>()
                .ConnectionString(asqConnectionString);

            transport.Routing().RegisterPublisher(typeof(ItemRestocked), "Warehouse");

            // Persistence Configuration
            endpointConfiguration.UsePersistence<InMemoryPersistence>();

            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.RegisterMessageMutator(new RemoveAssemblyInfoFromMessageMutator());
            endpointConfiguration.EnableInstallers();

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            while (true)
            {
                var key = Console.ReadKey();
                log.Info("Press Esc to exit ...");

                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }
            }

            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}
