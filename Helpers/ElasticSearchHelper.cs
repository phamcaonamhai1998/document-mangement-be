using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Options;
using Nest;

namespace WebApi.Helpers
{
    public class ElasticSearchHelper
    {
        public ElasticsearchClient Client { get; }

        AppSettings _appSettings;
        public ElasticSearchHelper(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            Client = GetClient();
        }

        private ElasticsearchClient GetClient()
        {
            var settings = new ElasticsearchClientSettings(new Uri(_appSettings.Elasticsearch.Url))
                           .CertificateFingerprint(_appSettings.Elasticsearch.FingerPrint)
                           .Authentication(new BasicAuthentication(_appSettings.Elasticsearch.Username, _appSettings.Elasticsearch.Password));
            var client = new ElasticsearchClient(settings);
            return client;
        }

        public ElasticClient GetNESTClient(string index)
        {
            var settings = new ConnectionSettings(new Uri(_appSettings.Elasticsearch.Url))
                           .CertificateFingerprint(_appSettings.Elasticsearch.FingerPrint)
                           .BasicAuthentication(_appSettings.Elasticsearch.Username, _appSettings.Elasticsearch.Password)
                           .DefaultIndex(index);
            var client = new ElasticClient(settings);
            return client;
        }


        public async Task<bool> CreateIndexIfNotExist(string name)
        {
            var getResult = await Client.Indices.GetAsync(name);
            if (!getResult.IsValidResponse)
            {
                var res = await Client.Indices.CreateAsync(name);
                return res.IsSuccess();
            }

            return true;
        }

        public async Task<bool> CreateDoc<E>(E doc, string index)
        {
            var response = await Client.IndexAsync(doc, index);
            return response.IsSuccess();
        }

        public async Task<bool> UpdateDoc<E>(E doc, string id, string index)
        {
            var response = await Client.UpdateAsync<E, E>(index, id, u => u.Doc(doc));
            return response.IsSuccess();
        }

        public async Task<bool> Delete(string id, string index)
        {
            var response = await Client.DeleteAsync(index, id);
            return response.IsSuccess();
        }

        public async Task<E?> GetDoc<E>(string id, string index)
        {
            var response = await Client.GetAsync<E>(1, idx => idx.Index(index));

            if (response.IsValidResponse)
            {
                var source = response.Source;
                return source;
            }

            return default(E);

        }
    }
}
