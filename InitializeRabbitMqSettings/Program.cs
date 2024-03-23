using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;

namespace InitializeRabbitMqSettings
{
    class Program
    {
        static void Main()
        {
            // Permet de créer les exchanges et les queues pour la Démo

            Console.WriteLine("Début de l'initialisation des configurations");

            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            // Queue principale où arrive le message
            // On déclare un DLX en cas d'échec
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("x-dead-letter-exchange", "retry_exchange");
            channel.ExchangeDeclare("messages", ExchangeType.Direct, true, false, null);
            channel.QueueDeclare(queue: "task_queue", durable: true, exclusive: false, autoDelete: false, arguments: args);
            channel.QueueBind("task_queue", "messages", string.Empty);

            // Queue de retry : en cas d'échec avec la première queue. Permet de retry autant de fois que l'on spécifie dans le programme
            // On déclare un DLX en cas d'échec qui sera la vraie corbeille d'échec
            Dictionary<string, object> args_1 = new Dictionary<string, object>();
            args_1.Add("x-dead-letter-exchange", "messages");
            channel.ExchangeDeclare("retry_exchange", ExchangeType.Fanout, true, false, null);
            channel.QueueDeclare(queue: "retry_exchange_queue", durable: true, exclusive: false, autoDelete: false, arguments: args_1);
            channel.QueueBind("retry_exchange_queue", "retry_exchange", string.Empty);

            // Queue finale qui contiendra les échecs
            channel.ExchangeDeclare("dead_messages", ExchangeType.Fanout, true, false, null);
            channel.QueueDeclare(queue: "dead_messages_queue",durable: true,exclusive: false,autoDelete: false,arguments: null);
            channel.QueueBind("dead_messages_queue", "dead_messages", string.Empty);

            Console.WriteLine("Fin de l'initialisation des configurations");
        }
    }
}
