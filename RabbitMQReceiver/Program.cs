using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RabbitMQReceiver
{
    class Program
    {
        static void Main()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] Received {message}");

                int dots = message.Split('.').Length - 1;

                if (dots % 2 == 0)
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                else
                    channel.BasicNack(ea.DeliveryTag, false, false);

                Console.WriteLine($"{(dots % 2 == 0 ? "Accepté" : "Refusé")}");
            };

            channel.BasicConsume(queue: "task_queue",
                                 autoAck: false,
                                 consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
