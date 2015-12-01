using log4net.Appender;
using log4net.Core;
using System;
using System.Net;

namespace gelf4net.Appender
{
    public class GelfHttpAppender : AppenderSkeleton
    {
        private WebClient _webClient;
        private Uri _baseUrl;

        public string Url { get; set; }

        public GelfHttpAppender()
        {
            _webClient = new WebClient();
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            _baseUrl = new Uri(Url);
            ServicePointManager.FindServicePoint(_baseUrl).Expect100Continue = false;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                var payload = this.RenderLoggingEvent(loggingEvent);
                _webClient.UploadStringAsync(_baseUrl, payload);
            }
            catch (Exception ex)
            {
                this.ErrorHandler.Error("Unable to send logging event to remote host " + this.Url, ex);
            }
        }
    }
}
