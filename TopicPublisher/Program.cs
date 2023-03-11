using RabbitMQ.Client;
using System.Text;

var exchangeName = "ex.topic";
var queue1Name = "my.queue1";
var queue2Name = "my.queue2";
var queue3Name = "my.queue3";

var routingKey1 = "*.image.*";
var routingKey2 = "#.image";
var routingKey3 = "image.#";

var counterCommands = 0;


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

channel.ExchangeDeclare(exchangeName, "topic", true, false);
channel.QueueDeclare(queue1Name, true, false, false);
channel.QueueDeclare(queue2Name, true, false, false);
channel.QueueDeclare(queue3Name, true, false, false);

channel.QueueBind(queue1Name, exchangeName, routingKey1);
channel.QueueBind(queue2Name, exchangeName, routingKey2);
channel.QueueBind(queue3Name, exchangeName, routingKey3);

while (true)
{
    Console.Write("Press any key to publish a message, or 'X' to quit, or ");
    Console.WriteLine("CTRL+C to interrupt the read operation:");

    // Start a console read operation. Do not display the input.
    var cki = Console.ReadKey(true);

    // Announce the name of the key that was pressed .
    Console.WriteLine($"  Key pressed: {cki.Key}\n");

    // Exit if the user pressed the 'X' key.
    if (cki.Key == ConsoleKey.X) break;

    counterCommands++;

    var message1 = $"Message {counterCommands}";
    channel.BasicPublish(exchangeName, "convert.image.jpg", null, Encoding.UTF8.GetBytes(message1));

    Console.WriteLine($"Published: {message1}");
}