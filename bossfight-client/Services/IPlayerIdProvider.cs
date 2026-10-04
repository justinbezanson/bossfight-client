using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace bossfight_client.Services;

/// <summary>
/// Supplies the player IDs offered in the settings dropdown. Backed by sample data for
/// now; swap the implementation for an API call once the endpoint is available.
/// </summary>
public interface IPlayerIdProvider
{
    Task<IReadOnlyList<string>> GetPlayerIdsAsync(CancellationToken cancellationToken = default);
}

public class SamplePlayerIdProvider : IPlayerIdProvider
{
    private static readonly string[] Samples =
    [
        "player-1",
        "player-2",
        "player-3",
        "player-4"
    ];

    public Task<IReadOnlyList<string>> GetPlayerIdsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<string>>(Samples);
    }
}