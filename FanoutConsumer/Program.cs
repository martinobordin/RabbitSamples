using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var queue1Name = "my.queue1";
var routingKey = string.Empty;

var connectionFactory = new ConnectionFactory
{
    HostName = "localhost",
    VirtualHost = "/",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

using var connection = connectionFactory.CreateConnection();
using var channel = connection.CreateModel();

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (object? sender, BasicDeliverEventArgs e) =>
{
    var message = Encoding.UTF8.GetString(e.Body.Span);
    Console.WriteLine($"Received: {message}");

    // Manual Ack
    channel.BasicAck(e.DeliveryTag, false);

    //channel.BasicNack(e.DeliveryTag, false, true);
};

channel.BasicConsume(queue1Name, autoAck: false, consumer);
Console.ReadKey(true);