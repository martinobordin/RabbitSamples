using RabbitMQ.Client;
using System.Text;

var exchangeName = "ex.fanout";
var routingKey = string.Empty;
var counter = 0;

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

channel.ExchangeDeclare(exchangeName, "fanout", true, false);

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

    var message = $"Hello, it's {counter++}";
    channel.BasicPublish(exchangeName, routingKey, null, Encoding.UTF8.GetBytes(message));
    Console.WriteLine($"Published: {message}");
}

//channel.QueueDelete(queue1Name);
//channel.QueueDelete(queue2Name);
//channel.ExchangeDelete(exchangeName);
