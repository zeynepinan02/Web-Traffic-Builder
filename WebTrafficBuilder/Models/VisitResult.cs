using MongoDB.Bson;
using Newtonsoft.Json;

namespace WebTrafficBuilder.Models
{
    //hostlara yaptığımız isteğin cevapları için
    public class VisitResult
    {
        public string Url { get; set; }

        public int Size { get; set; }

        public string VisitTime { get; set; } 
        
        public string RequestTotalDuration { get; set; }

        [JsonIgnore]
        public Task Task { get; set; }

        public VisitResult() 
        {
            Url = string.Empty;
            Size = 0;
            VisitTime = string.Empty;
            RequestTotalDuration = string.Empty;
            Task = Task.CompletedTask;
        }

    }
}
