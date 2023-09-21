using MongoDB.Bson;
using Newtonsoft.Json.Bson;

namespace WebTrafficBuilder.Models
{
    public class WebSite
    {
        public ObjectId Id { get; set; }
        public string Url { get; set; }
    }
}
