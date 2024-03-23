using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RabbitMQSender
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();


            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            for(var i = 1; i <= 10; i++)
            {
                var message = $"Message avec {(i % 2 == 0 ? "." : "..")}";
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "messages",
                                     routingKey: string.Empty,
                                     basicProperties: properties,
                                     body: body);
                Console.WriteLine($" [x] Sent {i}");
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
