namespace bossfight_client.Responses;

public abstract class Response
{
    public bool Success { get; set; }
    public string? Status { get; set; }
    public string? Content { get; set; }
}