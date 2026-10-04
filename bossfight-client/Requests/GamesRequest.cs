using System.Net.Http;
using bossfight_client.Responses;
using StoreModel = bossfight_client.Store.Store;

namespace bossfight_client.Requests;

public class GamesRequest : Request
{
    public GamesRequest(StoreModel store) : base(store)
    {
    }

    protected override string Endpoint => "/api/games";
    protected override HttpMethod Method => HttpMethod.Get;

    protected override Response CreateResponse(bool success, string content, string status)
    {
        return GamesResponse.FromContent(content, success, status);
    }
}
