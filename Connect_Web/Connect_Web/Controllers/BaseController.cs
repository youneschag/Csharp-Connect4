using Microsoft.AspNetCore.Mvc;
using Connect_Web.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Connect_Web.Controllers
{
    [ActionOnController]
    public class BaseController : Controller
    {
        public DateTime StartApiTime { get; set; }
        public DateTime StopApiTime { get; set; }
        public TimeSpan TotalApiTime { get; set; }
        public string ApiUrl { get; set; } = "";
        public Dictionary<string, string> SuggeredUrls { get; } = new Dictionary<string, string>();
        protected readonly IConfiguration _configuration;

        public BaseController(IConfiguration configuration)
        {
            _configuration = configuration;
            TotalApiTime = new TimeSpan();
        }

        protected HttpResponseMessage CallApiGet(string url, bool useDefaultApiUrl = true)
        {
            BeforeApiCall(out HttpClient httpClient);

            if (useDefaultApiUrl)
            {
                url = ApiUrl + url;
            }

            var result = httpClient.GetAsync(url);

            // Utiliser _configuration pour obtenir le délai d'attente
            int maxWaitingTime = Convert.ToInt32(_configuration["ApiMaxWaitingTime"]);

            if (result.Wait(maxWaitingTime))
            {
                AfterApiCall();
                return result.Result;
            }
            else
            {
                throw new TimeoutException("L'API a pris trop de temps à répondre");
            }
        }

        protected HttpResponseMessage CallApiPost(string url, HttpContent httpContent = null)
        {
            BeforeApiCall(out HttpClient httpClient);
            url = ApiUrl + url;

            var result = httpClient.PostAsync(url, httpContent);

            int maxWaitingTime = Convert.ToInt32(_configuration["ApiMaxWaitingTime"]);
            if (result.Wait(maxWaitingTime))
            {
                AfterApiCall();
                return result.Result;
            }
            else
            {
                throw new TimeoutException("L'API a pris trop de temps à répondre");
            }
        }

        protected HttpResponseMessage CallApiPut(string url, HttpContent httpContent = null)
        {
            BeforeApiCall(out HttpClient httpClient);
            url = ApiUrl + url;

            var result = httpClient.PutAsync(url, httpContent);

            int maxWaitingTime = Convert.ToInt32(_configuration["ApiMaxWaitingTime"]);
            if (result.Wait(maxWaitingTime))
            {
                AfterApiCall();
                return result.Result;
            }
            else
            {
                throw new TimeoutException("L'API a pris trop de temps à répondre");
            }
        }

        protected HttpResponseMessage CallApiDelete(string url, string content = null)
        {
            BeforeApiCall(out HttpClient httpClient);
            url = ApiUrl + url;

            // Créer le HttpRequestMessage pour DELETE
            var request = new HttpRequestMessage(HttpMethod.Delete, url);

            // Ajouter le contenu si nécessaire
            if (!string.IsNullOrEmpty(content))
            {
                request.Content = new StringContent(content, Encoding.UTF8, "application/json");
            }

            // Envoyer la requête
            var responseTask = httpClient.SendAsync(request);

            int maxWaitingTime = Convert.ToInt32(_configuration["ApiMaxWaitingTime"]);
            if (responseTask.Wait(maxWaitingTime))
            {
                AfterApiCall();
                return responseTask.Result;
            }
            else
            {
                throw new TimeoutException("L'API a pris trop de temps à répondre");
            }
        }

        protected HttpResponseMessage CallApiPatch(string url, HttpContent httpContent = null)
        {
            BeforeApiCall(out HttpClient httpClient);
            url = ApiUrl + url;

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = httpContent
            };

            var result = httpClient.SendAsync(request);

            int maxWaitingTime = Convert.ToInt32(_configuration["ApiMaxWaitingTime"]);
            if (result.Wait(maxWaitingTime))
            {
                AfterApiCall();
                return result.Result;
            }
            else
            {
                throw new TimeoutException("L'API a pris trop de temps à répondre");
            }
        }

        private void BeforeApiCall(out HttpClient httpClient)
        {
            httpClient = new HttpClient();
            StartApiTime = DateTime.Now;
        }

        private void AfterApiCall()
        {
            StopApiTime = DateTime.Now;
            TotalApiTime += StopApiTime - StartApiTime;
        }
    }
}