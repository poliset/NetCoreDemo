﻿namespace ITOps.WarehouseBridge
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Bridge;

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "ITOps.WarehouseBridge";

            var asqConnectionString = Environment.GetEnvironmentVariable("NetCoreDemoAzureStorageQueueTransport");
            if (string.IsNullOrEmpty(asqConnectionString))
            {
                Console.WriteLine("Connection for Azure Storage Queue transport is missing or empty.");
            }

            var rabbitMqConnectionString = Environment.GetEnvironmentVariable("NetCoreDemoRabbitMQTransport");
            if (string.IsNullOrEmpty(asqConnectionString))
            {
                Console.WriteLine("Connection for RabbitMQ transport is missing or empty.");
            }

            var bridgeConfiguration = Bridge
                .Between<AzureStorageQueueTransport>(endpointName: "bridge-warehouse", customization: transport =>
                {
                    transport.ConnectionString(asqConnectionString);
                    transport.SerializeMessageWrapperWith<NewtonsoftSerializer>();
                })
                .And<RabbitMQTransport>(endpointName: "bridge-shipping", customization: transport =>
                {
                    transport.ConnectionString(rabbitMqConnectionString);
                });

            bridgeConfiguration.AutoCreateQueues();
            bridgeConfiguration.UseSubscriptionPersistence(new InMemorySubscriptionStorage());

            var bridge = bridgeConfiguration.Create();

            await bridge.Start()
                .ConfigureAwait(false);

            Console.WriteLine("Press ESC key to exit");
            UILoop();

            await bridge.Stop()
                .ConfigureAwait(false);

        }

        static void UILoop()
        {
            while (true)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                    case ConsoleKey.Q:
                        return;
                }
            }
        }
    }
}
