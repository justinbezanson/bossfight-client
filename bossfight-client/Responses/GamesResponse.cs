using System.Collections.Generic;
using System.Text.Json;

namespace bossfight_client.Responses;

public class Game
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
}

public class GamesResponse : Response
{
    public List<Game>? Games { get; set; }

    public GamesResponse()
    {
        Success = false;
        Status = null;
        Content = null;
        Games = null;
    }

    public static GamesResponse FromContent(string content, bool success, string status)
    {
        var response = new GamesResponse
        {
            Success = success,
            Status = status,
            Content = content
        };

        if (!string.IsNullOrWhiteSpace(content) && success)
        {
            try
            {
                var games = JsonSerializer.Deserialize<List<Game>>(content);
                response.Games = games;
            }
            catch
            {
                // If parsing fails, leave Games as null
            }
        }

        return response;
    }
}
