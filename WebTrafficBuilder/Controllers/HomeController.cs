using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading.Tasks;
using WebTrafficBuilder.Models;
using WebTrafficBuilder.Services;

namespace WebTrafficBuilder.Controllers
{
    //[Controller]
    //[Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MongoDBService _mongoDBService;
        public static List<VisitResult> Tasks = new List<VisitResult>();

        public HomeController(ILogger<HomeController> logger, MongoDBService mongoDBService)
        {
            _logger = logger;
            _mongoDBService = mongoDBService;
        }

        //[Route("Index")]
        //[HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        
        [HttpPost]
        public async Task<Boolean> AddHost(WebSite webSite)       //parametreyle gelen hostu veritabanına ekler
        {
            if(!Contains(webSite))                                //veritabanında bu host kayıtlı değilse çalışır
            {             
                await _mongoDBService.CreatAsync(webSite); 
                return true;
            }            
            return false;
        }
        public async Task<List<WebSite>> GetAllHost()            //Veri tabanından tüm hostları getirir
        {
            return await _mongoDBService.GetAsync();
        }
        public Boolean Contains(WebSite webSite)               //Veritabanında böyle bir kayıt var mı kontrol eder
        {
            if (_mongoDBService.Contains(webSite))
            {
                return true;
            }
            else
            {
                return false;   
            }                      
        }

        //StartVisitte sitelerden dönen cevapları çeker
        public async Task<List<string>> GetVisit()
        {
            List<string> result = new List<string>();
            foreach(var item in Tasks)
            {
                result.Add(JsonConvert.SerializeObject(item)) ;
            }
            return result; 
        }

        [HttpPost]
        //asenkron şekilde veri tabanındaki hostlara sürekli bir şekilde istekte bulunup bilgilerini çeker.
        public async Task<IActionResult> StartVisit(bool randomVisit)
        {           
            List<WebSite> _webSites = new List<WebSite>();
            _webSites = await _mongoDBService.GetAsync();
            if (randomVisit) 
            { 
                _webSites = _webSites.OrderBy(a => System.Guid.NewGuid()).ToList();
            }
            var cancelToken = new CancellationTokenSource(); //task i istediğimiz zaman durdurmya yarıyor. Default olarak false geliyor. true yaparak taskleri durdurabilirsin 
            CancellationToken ct = cancelToken.Token;
            foreach (var website in _webSites)
            {
                await Task.Delay(1000);
                Task task = await Task.Factory.StartNew(async () => await HttpRequest(ct, website.Url));// burası yeni task oluşturmamızı sağlıyor bu task httpRequest metoduna gidiyor. Orda o işlemler devam ederken bu foreach dönmeye devam ediyor ve yeni taskler için aynı fonksiyon dönüyor. Await leri yazmayı unutma çok önemli diğer türlü taskler üst üste biner vs 
                Tasks.Add(new VisitResult { Task = task, Url = website.Url }); // tasklarımın olduğu listeye bu taskimi nesnemin bir property si olarak ekledim. Bu nesnenin diğer propertylerinin değerlerini siteye istek yaptıktan sonra öğreneceğimiz için daha sonra fonksiyonun içinde set edeceğiz
            }
            return NoContent();
        }

        public async Task<bool> HttpRequest(CancellationToken cancellationToken, string website)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            while (true)
            {
                Stopwatch stopwatch = Stopwatch.StartNew(); //kronometre oluşturuyoruz istekten ne kadar sürede cevap aldığımızı gözlemlemek için
                stopwatch.Start();
                await Task.Delay(1000);                            
                var httpClient = new HttpClient(clientHandler);
                var response = await httpClient.GetAsync(website);               
                stopwatch.Stop(); //istek gönderimi bittiği için kronometreyi durdurduk
                if (response.IsSuccessStatusCode) //istek başarılıysa
                {
                    var responseBody = await response.Content.ReadAsStringAsync(); //cevap mesajı
                    foreach (var item in Tasks)
                    {
                        if (item.Url == website) //tasklarin içinde istek gönderdiğimiz taski buluyor propertyleri set ediyoruz
                        {                           
                            item.VisitTime = Convert.ToString (DateTime.Now);
                            item.Size = System.Text.ASCIIEncoding.ASCII.GetByteCount(responseBody);
                            item.RequestTotalDuration = Convert.ToString(stopwatch.Elapsed);
                        }
                    }
                }
                if (cancellationToken.IsCancellationRequested) //task cancel edildiyse döngüyü kırıyor.
                {
                    break;
                }
            }
            return true;
        }
    }
}

