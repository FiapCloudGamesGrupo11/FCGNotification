using System.Text.Json.Serialization;

namespace NotificationsAPI.Events;

public class UserCreatedEvent
{
	[JsonPropertyName("Id")]
	public Guid UserId { get; set; }

	public string Name { get; set; } = string.Empty;

	public string Email { get; set; } = string.Empty;
}