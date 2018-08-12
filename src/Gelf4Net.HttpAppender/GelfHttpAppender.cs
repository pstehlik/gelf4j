using log4net.Appender;
using log4net.Core;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gelf4Net.Appender
{
    public class GelfHttpAppender : AppenderSkeleton
    {
        private readonly HttpClient _httpClient;

        private Uri _baseUrl;

        public string Url { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public GelfHttpAppender()
        {
            _httpClient = new HttpClient();
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            _baseUrl = new Uri(Url);

            _httpClient.DefaultRequestHeaders.ExpectContinue = false;

            if (!string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Password))
            {
                var byteArray = Encoding.ASCII.GetBytes(User + ":" + Password);
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            var payload = RenderLoggingEvent(loggingEvent);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            _httpClient.PostAsync(_baseUrl, content).ContinueWith(CallBackAfterPost);
        }

        private void CallBackAfterPost(Task<HttpResponseMessage> obj)
        {
            if (obj.Exception != null)
              ErrorHandler.Error("Unable to send logging event to remote host " + this.Url, obj.Exception);
        }
    }
}