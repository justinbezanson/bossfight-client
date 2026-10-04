using System.Net.Http;
using bossfight_client.Responses;
using StoreModel = bossfight_client.Store.Store;

namespace bossfight_client.Requests;

public class PlayersRequest : Request
{
    public PlayersRequest(StoreModel store) : base(store)
    {
    }

    protected override string Endpoint => "/api/players";
    protected override HttpMethod Method => HttpMethod.Get;

    protected override Response CreateResponse(bool success, string content, string status)
    {
        return PlayersResponse.FromContent(content, success, status);
    }
}
