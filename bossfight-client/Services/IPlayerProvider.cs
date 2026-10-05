using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using bossfight_client.Responses;
using StoreModel = bossfight_client.Store.Store;

namespace bossfight_client.Services;

/// <summary>
/// Supplies the players offered in the settings dropdown. Backed by sample data for
/// now; swap the implementation for an API call once the endpoint is available.
/// </summary>
public interface IPlayerProvider
{
    /// <param name="store">
    /// The credentials currently typed into the form, not the ones already on disk, so
    /// the dropdown can be tested before the settings are saved.
    /// </param>
    Task<IReadOnlyList<Player>> GetPlayersAsync(StoreModel store, CancellationToken cancellationToken = default);
}

public class SamplePlayerProvider : IPlayerProvider
{
    private static readonly Player[] Samples =
    [
        new Player { Id = 1, Name = "Justin" },
        new Player { Id = 2, Name = "Josh" },
        new Player { Id = 3, Name = "Carter" }
    ];

    public Task<IReadOnlyList<Player>> GetPlayersAsync(StoreModel store, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<Player>>(Samples);
    }
}
