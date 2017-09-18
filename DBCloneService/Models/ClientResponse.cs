namespace DBCloning.Models
{
    using RestSharp;

    public class ClientResponse<TPayload>
    {
        public IRestResponse Response { get; set; }

        public TPayload Payload { get; set; }
    }
}