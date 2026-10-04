using System.Collections.Generic;
using System.Text.Json;

namespace bossfight_client.Responses;

public class Player
{
    public string? Id { get; set; }
    public string? Name { get; set; }
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
