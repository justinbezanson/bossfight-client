using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace bossfight_client.Responses;

public class Player
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public class PlayersResponse : Response
{
    public List<Player>? Players { get; set; }

    public PlayersResponse()
    {
        Success = false;
        Status = null;
        Content = null;
        Players = null;
    }

    public static PlayersResponse FromContent(string content, bool success, string status)
    {
        var response = new PlayersResponse
        {
            Success = success,
            Status = status,
            Content = content
        };

        if (!string.IsNullOrWhiteSpace(content) && success)
        {
            try
            {
                var players = JsonSerializer.Deserialize<List<Player>>(content);
                response.Players = players;
            }
            catch
            {
                // If parsing fails, leave Players as null
            }
        }

        return response;
    }
}
