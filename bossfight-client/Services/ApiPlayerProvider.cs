using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using bossfight_client.Requests;
using bossfight_client.Responses;
using StoreModel = bossfight_client.Store.Store;

namespace bossfight_client.Services;

public class ApiPlayerProvider : IPlayerProvider
{
    public async Task<IReadOnlyList<Player>> GetPlayersAsync(StoreModel store, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(store.ApiKey) || string.IsNullOrWhiteSpace(store.ApiUrl))
        {
            return Array.Empty<Player>();
        }

        try
        {
            var request = new PlayersRequest(store);
            var response = await request.SendAsync(cancellationToken);

            if (response is PlayersResponse playersResponse && playersResponse.Success &&
                playersResponse.Players != null)
            {
                return playersResponse.Players.ToList();
            }

            return Array.Empty<Player>();
        }
        catch
        {
            return Array.Empty<Player>();
        }
    }
}
