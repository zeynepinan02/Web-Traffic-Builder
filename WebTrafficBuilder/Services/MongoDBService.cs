using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using WebTrafficBuilder.Models;

namespace WebTrafficBuilder.Services
{
    public class MongoDBService
    {
        private readonly IMongoCollection<WebSite> _webSite;
        public MongoDBService(IOptions<MongoDBSetting> mongoDBSettings)
        {
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _webSite = database.GetCollection<WebSite>(mongoDBSettings.Value.CollectionName);
        }

        //database insert yapar
        public async Task CreatAsync(WebSite webSite)
        {
            await _webSite.InsertOneAsync(webSite);
            return;
        }
        
        //databasedeki tüm hostları çeker
        public async Task<List<WebSite>> GetAsync()
        {
            return await _webSite.Find(new BsonDocument()).ToListAsync();
        }

        //databasede böyle bir kayıt var mı kontrol eder
        public Boolean Contains(WebSite webSite)
        {
            FilterDefinition<WebSite> filter = Builders<WebSite>.Filter.Eq("Url", webSite.Url);
            var result = _webSite.Find(filter).FirstOrDefault();
            if (result != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
