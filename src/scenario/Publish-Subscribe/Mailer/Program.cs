using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var exchangeName = "ex.fanout";
var queueName = "mailer";
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

channel.QueueDeclare(queueName, true, false, false);
channel.QueueBind(queueName, exchangeName, routingKey);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (object? sender, BasicDeliverEventArgs e) =>
{
    var message = Encoding.UTF8.GetString(e.Body.Span);
    Console.WriteLine($"Received by MAILER: {message}");

    // Manual Ack
    channel.BasicAck(e.DeliveryTag, false);

    //channel.BasicNack(e.DeliveryTag, false, true);
};

channel.BasicConsume(queueName, autoAck: false, consumer);
Console.ReadKey(true);