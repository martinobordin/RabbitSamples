using RabbitMQ.Client;
using System.Text;

var exchangeName = "ex.direct";
var queue1Name = "my.queue1";
var queue2Name = "my.queue2";
var routingKey1 = "commands";
var routingKey2 = "events";
var counterCommands = 0;
var counterEvents = 0;

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

channel.ExchangeDeclare(exchangeName, "direct", true, false);
channel.QueueDeclare(queue1Name, true, false, false);
channel.QueueDeclare(queue2Name, true, false, false);

channel.QueueBind(queue1Name, exchangeName, routingKey1);
channel.QueueBind(queue2Name, exchangeName, routingKey2);

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

    var message1 = $"Command {counterCommands++}";
    channel.BasicPublish(exchangeName, routingKey1, null, Encoding.UTF8.GetBytes(message1));

    Console.WriteLine($"Published: {message1}");

    var message2 = $"Event {counterEvents++}";
    channel.BasicPublish(exchangeName, routingKey2, null, Encoding.UTF8.GetBytes(message2));

    Console.WriteLine($"Published: {message2}");
}