using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using bossfight_client.Requests;
using bossfight_client.Responses;
using StoreModel = bossfight_client.Store.Store;

namespace bossfight_client.Services;

public class ApiPlayerIdProvider : IPlayerIdProvider
{
    private readonly StoreModel _store;

    public ApiPlayerIdProvider(StoreModel store)
    {
        _store = store;
    }

    public async Task<IReadOnlyList<string>> GetPlayerIdsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(_store.ApiKey) || string.IsNullOrWhiteSpace(_store.ApiUrl))
        {
            return Array.Empty<string>();
        }

        try
        {
            var request = new PlayersRequest(_store);
            var response = await request.SendAsync(cancellationToken);

            if (response is PlayersResponse playersResponse && playersResponse.Success &&
                playersResponse.Players != null)
            {
                return playersResponse.Players
                    .Where(p => !string.IsNullOrWhiteSpace(p.Id))
                    .Select(p => p.Id!)
                    .ToList();
            }

            return Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}
