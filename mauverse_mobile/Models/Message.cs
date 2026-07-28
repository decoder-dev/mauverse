using System.Text.Json.Serialization;

namespace mau.Models;

public sealed class Message
{
    public User Sender { get; set; } = new();
    public string Text { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public int Id { get; set; }

    [JsonPropertyName("useridfrom")]
    public int UserIdFrom { get; set; }

    public int UserIdTo { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string SmallMessage { get; set; } = string.Empty;
    public string ContextUrlName { get; set; } = string.Empty;
    public string UserFromFullName { get; set; } = string.Empty;
    public int Notification { get; set; }
    public string ColorName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string ContextUrl { get; set; } = string.Empty;
    public long TimeCreated { get; set; }
    public string TimeCreatedString { get; set; } = string.Empty;
}
