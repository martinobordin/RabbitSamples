using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RequestorReplierShared;
using System.Text;
using System.Text.Json;

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

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, e) =>
{
    var requestData = Encoding.UTF8.GetString(e.Body.ToArray());
    var requestId = e.BasicProperties.Headers[RequestorReplierShared.Constants.RequestIdHeaderKey];
    var replyTo = e.BasicProperties.ReplyTo;
    CalculationRequest request = JsonSerializer.Deserialize<CalculationRequest>(requestData)!;
    Console.WriteLine($"Request received:{request}. Replying to {replyTo}");

    var response = new CalculationResponse();

    if (request.Operation == OperationType.Add)
    {
        response.Result = request.Number1 + request.Number2;
    }
    else if (request.Operation == OperationType.Subtract)
    {
        response.Result = request.Number1 - request.Number2;
    }

    var responseData = JsonSerializer.Serialize(response);

    var basicProperties = channel.CreateBasicProperties();
    basicProperties.Headers = new Dictionary<string, object>
    {
        { RequestorReplierShared.Constants.RequestIdHeaderKey, requestId }
    };

    var responseQueueName = replyTo;

    channel.BasicPublish(
    "",
    responseQueueName,
    basicProperties,
    Encoding.UTF8.GetBytes(responseData));
};

channel.BasicConsume(RequestorReplierShared.Constants.RequestQueueName, true, consumer);

Console.WriteLine("Press a key to exit.");
Console.ReadKey();

channel.Close();
conn.Close();