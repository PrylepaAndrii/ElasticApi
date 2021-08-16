using ElasticApi.DTOs;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticApi.Services
{
    public interface ISuggesstionService
    {
        public Task<Tuple<mgmt, Place>> suggest(string line);
        public Task<Place> autocompletePlace(string request);
    }
    public class SuggesstionService : ISuggesstionService
    {
        IElasticClient _client;
        public SuggesstionService(IElasticClient client)
        { _client = client; }
        public async Task<Tuple<mgmt, Place>> suggest(string line)
        {
            var ManagementResponse = await _client.SearchAsync<mgmt>(e =>
                e.Query(f => f
                    .CombinedFields(q => q
                        .Query(line)
                            .Fields(p => p
                                .Field("market")
                                .Field("name"))
                                    .Operator(Operator.Or)))
                .Size(25));
            var PlacesResponse = await _client.SearchAsync<Place>(e => 
                e.Query(f => f
                    .CombinedFields(q => q
                        .Query(line)
                            .Fields(p => p
                                .Field("market")
                                .Field("state")
                                .Field("name")
                                .Field("street"))
                                    .Operator(Operator.Or)))
                .Size(25));
            if (ManagementResponse.MaxScore > PlacesResponse.MaxScore)
                return new Tuple<mgmt, Place>(ManagementResponse.Documents.ElementAtOrDefault(0), null);
            else
                return new Tuple<mgmt, Place>(null, PlacesResponse.Documents.ElementAtOrDefault(0));

        }
        public async Task<Place> autocompletePlace(string request)
        {
            var resp = await _client.SearchAsync<Place>(e => e
            .Query(f => f
                .Bool(b => b
                    .Should(s => s.Prefix(p => p.market, request)).Boost(2)
                    .Should(s => s.Prefix(p => p.state, request)).Boost(3)
                    .Should(s => s.Prefix(p => p.streetAddress, request)).Boost(4)
                    .Should(s=>s.Prefix(p=>p.name,request)).Boost(5))));
            return resp.Documents.FirstOrDefault();
        }
    }
}
