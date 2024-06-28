using System.Text;
using RabbitMQ.Client;

namespace RabbitMQService.Controllers;

public class RabbitMQService : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly HttpClient _httpClient;

    public RabbitMQService(string hostname, string queueName, HttpClient httpClient)
    {
        var factory = new ConnectionFactory() { HostName = hostname };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        _httpClient = httpClient;
    }
    
    public void SendMessage(string queueName, string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
        Console.WriteLine($" [x] Sent {message}");
    }

    public async Task<string> CheckInventoryConnectionAsync()
    {
        var response = await _httpClient.GetAsync("http://localhost:5000/Product/CheckConnection");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}