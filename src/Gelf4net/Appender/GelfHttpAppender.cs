using log4net.Appender;
using log4net.Core;
using System;
using System.Net;
using System.Text;

namespace gelf4net.Appender
{
    public class GelfHttpAppender : AppenderSkeleton
    {
        private Uri _baseUrl;
        private string _credentials;

        public string Url { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public GelfHttpAppender()
        {

        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            _baseUrl = new Uri(Url);
            ServicePointManager.FindServicePoint(_baseUrl).Expect100Continue = false;

            if(!string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Password))
            {
                _credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(User + ":" + Password));
            }
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                var payload = this.RenderLoggingEvent(loggingEvent);
                var webClient = new WebClient();
                if(!string.IsNullOrEmpty(_credentials))
                {
                    webClient.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", _credentials);
                }
                webClient.UploadStringAsync(_baseUrl, payload);
            }
            catch (Exception ex)
            {
                this.ErrorHandler.Error("Unable to send logging event to remote host " + this.Url, ex);
            }
        }
    }
}
