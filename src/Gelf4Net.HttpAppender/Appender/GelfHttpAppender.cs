using log4net.Appender;
using log4net.Core;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace gelf4net.Appender
{
    public class GelfHttpAppender : AppenderSkeleton
    {
        private HttpClient _httpClient;

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

            if (!string.IsNullOrEmpty(User) || !string.IsNullOrEmpty(Password))
            {
                var byteArray = Encoding.ASCII.GetBytes(User + ":" + Password);
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    var payload = this.RenderLoggingEvent(loggingEvent);
                    var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                    await _httpClient.PostAsync(_baseUrl, content);
                }
                catch (Exception ex)
                {
                    this.ErrorHandler.Error("Unable to send logging event to remote host " + this.Url, ex);
                }
            });
        }
    }
}
