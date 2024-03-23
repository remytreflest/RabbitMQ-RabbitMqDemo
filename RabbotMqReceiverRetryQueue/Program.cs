using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbotMqReceiverRetryQueue
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

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                IDictionary<string, object> headers = ea.BasicProperties.Headers;
                var xDeathCount = ExtractXDeathCount(headers);

                if(xDeathCount > 2)
                {
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    channel.BasicPublish(exchange: "dead_messages",
                                     routingKey: string.Empty,
                                     basicProperties: properties,
                                     body: body);
                    Console.WriteLine($" [x] Sent {message}");
                }
                else
                {
                    channel.BasicNack(ea.DeliveryTag, false, false);
                }

                Console.WriteLine(" [x] Done");
            };

            channel.BasicConsume(queue: "retry_exchange_queue",
                                 autoAck: false,
                                 consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static long ExtractXDeathCount(IDictionary<string, object> headers)
        {
            long xDeathCount = 0;
            if (headers != null && headers.ContainsKey("x-death"))
            {
                var xDeath = ((List<object>)headers["x-death"])[0];
                IDictionary<string, object> xDeathDictonnary = (IDictionary<string, object>)xDeath;
                xDeathCount = (long)xDeathDictonnary["count"];
            }
            return xDeathCount;
        }
    }
}
