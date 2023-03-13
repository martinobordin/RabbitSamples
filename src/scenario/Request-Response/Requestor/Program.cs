using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RequestorReplierShared;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

ConcurrentDictionary<string, CalculationRequest> waitingRequests = new ConcurrentDictionary<string, CalculationRequest>();

var factory = new ConnectionFactory
{
    HostName = "localhost",
    VirtualHost = "/",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

IConnection conn = factory.CreateConnection();
IModel channel = conn.CreateModel();

channel.QueueDeclare(RequestorReplierShared.Constants.RequestQueueName, true, false, false);

var responseQueueName = $"res.{Guid.NewGuid()}";
channel.QueueDeclare(responseQueueName);

Console.WriteLine($"Response queue name: {responseQueueName}");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, e) =>
{
    string requestId = Encoding.UTF8.GetString((byte[])e.BasicProperties.Headers[RequestorReplierShared.Constants.RequestIdHeaderKey]);
    CalculationRequest request;

    if (waitingRequests.TryRemove(requestId, out request))
    {
        string messageData = Encoding.UTF8.GetString(e.Body.ToArray());
        CalculationResponse response = JsonSerializer.Deserialize<CalculationResponse>(messageData)!;

        Console.WriteLine($"Calculation result: {request}={response}");
    }
};

channel.BasicConsume(responseQueueName, true, consumer);

//SendRequest(waitingRequests, channel, new CalculationRequest(2, 4, OperationType.Add), responseQueueName);
//SendRequest(waitingRequests, channel, new CalculationRequest(14, 6, OperationType.Subtract), responseQueueName);
//SendRequest(waitingRequests, channel, new CalculationRequest(50, 2, OperationType.Add), responseQueueName);
//SendRequest(waitingRequests, channel, new CalculationRequest(30, 6, OperationType.Subtract), responseQueueName);

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

    var rnd = new Random(Guid.NewGuid().GetHashCode());

    var request = new CalculationRequest(rnd.Next(100), rnd.Next(100), (OperationType)rnd.Next(1, 2));
    SendRequest(waitingRequests, channel, request, responseQueueName);
    Console.WriteLine($"Published: {request}");
}

Console.ReadKey();

channel.Close();
conn.Close();

static void SendRequest(
            ConcurrentDictionary<string, CalculationRequest> waitingRequest,
            IModel channel, CalculationRequest request, string responseQueueName)
{
    var requestId = Guid.NewGuid().ToString();
    var requestData = JsonSerializer.Serialize(request);

    waitingRequest[requestId] = request;

    var basicProperties = channel.CreateBasicProperties();
    basicProperties.ReplyTo = responseQueueName;
    basicProperties.Headers = new Dictionary<string, object>
    {
        { RequestorReplierShared.Constants.RequestIdHeaderKey, Encoding.UTF8.GetBytes(requestId) },
    };

    channel.BasicPublish(
        "",
        RequestorReplierShared.Constants.RequestQueueName,
        basicProperties,
        Encoding.UTF8.GetBytes(requestData));
}