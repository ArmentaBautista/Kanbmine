using System.Text.Json.Serialization;

namespace Kanbmine.Shared.Models;

public class ErrorResponse
{
    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new();
}
