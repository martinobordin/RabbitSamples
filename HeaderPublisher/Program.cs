using RabbitMQ.Client;
using System.Text;

var exchangeName = "ex.header";
var queue1Name = "my.queue1";
var queue2Name = "my.queue2";
var routingKey = string.Empty;
var headers1 = new Dictionary<string, object> { { "x-match", "all" }, { "job", "convert" }, { "format", "jpeg" } };
var headers2 = new Dictionary<string, object> { { "x-match", "any" }, { "job", "convert" }, { "format", "jpeg" } };

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


var props = channel.CreateBasicProperties();
props.Headers = new Dictionary<string, object> { { "job", "convert" }, { "format", "jpeg" } };

channel.ExchangeDeclare(exchangeName, "headers", true, false);
channel.QueueDeclare(queue1Name, true, false, false);
channel.QueueDeclare(queue2Name, true, false, false);

channel.QueueBind(queue1Name, exchangeName, routingKey, headers1);
channel.QueueBind(queue2Name, exchangeName, routingKey, headers2);

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
    channel.BasicPublish(exchangeName, routingKey, props, Encoding.UTF8.GetBytes(message));
    Console.WriteLine($"Published: {message}");
}

//channel.QueueDelete(queue1Name);
//channel.QueueDelete(queue2Name);
//channel.ExchangeDelete(exchangeName);
