using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Appender;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Esilog.Gelf4net.Transport;
using System.Collections;
using System.Text.RegularExpressions;

namespace Esilog.Gelf4net.Appender
{
	public class Gelf4NetAppender : AppenderSkeleton
	{
		public static int SHORT_MESSAGE_LENGTH = 250;
		public static string UNKNOWN_HOST = "unknown_host";
	    private const string GELF_VERSION = "1.0";

	    private readonly UdpTransport _udpTransport;
		private readonly AmqpTransport _amqpTransport;
		private string _additionalFields;
        private int _maxChunkSize;

		//---------------------------------------
		//configuration settings for the appender
		private Dictionary<string, string> innerAdditionalFields;

		public string AdditionalFields
		{
			get
			{
				return _additionalFields;
			}
			set
			{
				_additionalFields = value;

				if (_additionalFields != null)
					innerAdditionalFields = new Dictionary<string, string>();
				else
					innerAdditionalFields.Clear();
				innerAdditionalFields = _additionalFields.Split(',').ToDictionary(it => it.Split(':')[0], it => it.Split(':')[1]);
			}
		}
		public string Facility { get; set; }
		public string GrayLogServerHost { get; set; }
		public string GrayLogServerHostIpAddress { get; set; }
		public int GrayLogServerPort { get; set; }
		public string Host { get; set; }
		public bool IncludeLocationInformation { get; set; }
		public bool SendAsGelfOrAmqp { get; set; }
		public int MaxChunkSize
		{
			get { return _maxChunkSize; }
			set 
			{ 
				_maxChunkSize = value;
				if(_udpTransport != null)
					_udpTransport.MaxChunkSize = value;
			}
		}
		public int GrayLogServerAmqpPort { get; set; }
		public string GrayLogServerAmqpUser { get; set; }
		public string GrayLogServerAmqpPassword { get; set; }
		public string GrayLogServerAmqpVirtualHost { get; set; }
		public string GrayLogServerAmqpQueue { get; set; }

		public Gelf4NetAppender()
			: base()
		{
			Facility = null;
			GrayLogServerHost = "";
			GrayLogServerHostIpAddress = "";
			GrayLogServerPort = 12201;
			Host = null;
			IncludeLocationInformation = false;
			MaxChunkSize = 1024;
			SendAsGelfOrAmqp = true;
			GrayLogServerAmqpPort = 5672;
			GrayLogServerAmqpUser = "guest";
			GrayLogServerAmqpPassword = "guest";
			GrayLogServerAmqpVirtualHost = "/";
			GrayLogServerAmqpQueue = "queue1";

			_udpTransport = new UdpTransport { MaxChunkSize = MaxChunkSize };
			_amqpTransport = new AmqpTransport
			{
				VirtualHost = GrayLogServerAmqpVirtualHost,
				User = GrayLogServerAmqpUser,
				Password = GrayLogServerAmqpPassword,
				Queue = GrayLogServerAmqpQueue
			};
		}

		public void TestAppend(log4net.Core.LoggingEvent loggingEvent)
		{
			Append(loggingEvent);
		}

		protected override void Append(log4net.Core.LoggingEvent loggingEvent)
		{
			String gelfJsonString = CreateGelfJsonFromLoggingEvent(loggingEvent);
			if (SendAsGelfOrAmqp)
				SendGelfMessageToGrayLog(gelfJsonString);
			else
				SendAmqpMessageToGrayLog(gelfJsonString);
		}

		private String LoggingHostName
		{
			get
			{
				String ret = Host;
				if (ret == null)
				{
					try
					{
						ret = System.Net.Dns.GetHostName();
					}
					catch (SocketException)
					{
						ret = UNKNOWN_HOST;
					}
				}
				return ret;
			}
		}

		private void SendGelfMessageToGrayLog(string message)
		{
			if (GrayLogServerHostIpAddress == string.Empty)
				GrayLogServerHostIpAddress = GetIpAddressFromHostName();
			_udpTransport.Send(LoggingHostName, GrayLogServerHostIpAddress, GrayLogServerPort, message);
		}

		private void SendAmqpMessageToGrayLog(string message)
		{
			if (GrayLogServerHostIpAddress == string.Empty)
			{
				GrayLogServerHostIpAddress = GetIpAddressFromHostName();
			}
			_amqpTransport.Send(LoggingHostName, GrayLogServerHostIpAddress, GrayLogServerPort, message);
			
		}

        private string GetIpAddressFromHostName()
		{
			IPAddress[] addresslist = Dns.GetHostAddresses(GrayLogServerHost);
			return addresslist[0].ToString();
		}

        private string CreateGelfJsonFromLoggingEvent(log4net.Core.LoggingEvent loggingEvent)
		{
			var fullMessage = loggingEvent.RenderedMessage;
			if (loggingEvent.ExceptionObject != null)
				fullMessage = String.Format("{0} - {1}. {2}. {3}.", fullMessage, loggingEvent.ExceptionObject.Source, loggingEvent.ExceptionObject.Message, loggingEvent.ExceptionObject.StackTrace);

			var shortMessage = fullMessage;

			if (shortMessage.Length > SHORT_MESSAGE_LENGTH)
				shortMessage = shortMessage.Substring(0, SHORT_MESSAGE_LENGTH - 1);

			var gelfMessage = new GelfMessage
			{
				Facility = (this.Facility ?? "GELF"),
				File = "",
				FullMesage = fullMessage,
				Host = LoggingHostName,
				Level = GetSyslogSeverity(loggingEvent.Level),
				Line = "",
				ShortMessage = shortMessage,
				TimeStamp = loggingEvent.TimeStamp,
				Version = GELF_VERSION
			};

			//only set location information if configured
			if (IncludeLocationInformation)
			{
				gelfMessage.File = loggingEvent.LocationInformation.FileName;
				gelfMessage.Line = loggingEvent.LocationInformation.LineNumber;
			}

			var gelfJsonMessage = JsonConvert.SerializeObject(gelfMessage);

			var jsonObject = JObject.Parse(gelfJsonMessage);

			//add additional fields and prepend with _ if not present already
			if (innerAdditionalFields != null)
			{
				foreach (var item in innerAdditionalFields)
				{
					AddAdditionalFields(item.Key, item.Value, jsonObject);
				}
			}

			//add additional fields and prepend with _ if not present already
			if (loggingEvent.Properties != null)
			{
				foreach (DictionaryEntry item in loggingEvent.Properties)
				{
					var key = item.Key as string;
					if (key != null)
					{
						AddAdditionalFields(key, item.Value as string, jsonObject);
					}
				}
			}

			return jsonObject.ToString();
		}

        private void AddAdditionalFields(string key, string value, JObject jsonObject)
		{
            if (key == null) return;

            if (!key.StartsWith("_"))
                key = String.Format("_{0}", key);

            if (key != "_id")
            {
                key = Regex.Replace(key, "[\\W]", "");
                jsonObject.Add(key, value);
            }
		}

		private int GetSyslogSeverity(log4net.Core.Level level)
		{
			if(level == log4net.Core.Level.Alert)
				return (int) LocalSyslogAppender.SyslogSeverity.Alert;

			if(level == log4net.Core.Level.Critical || level == log4net.Core.Level.Fatal)
				return (int)LocalSyslogAppender.SyslogSeverity.Critical;

			if(level == log4net.Core.Level.Debug)
				return (int)LocalSyslogAppender.SyslogSeverity.Debug;

			if(level == log4net.Core.Level.Emergency)
				return (int)LocalSyslogAppender.SyslogSeverity.Emergency;

			if(level == log4net.Core.Level.Error)
				return (int)LocalSyslogAppender.SyslogSeverity.Error;

			if(level == log4net.Core.Level.Fine 
				|| level == log4net.Core.Level.Finer 
				|| level ==  log4net.Core.Level.Finest 
				|| level ==  log4net.Core.Level.Info 
				|| level ==  log4net.Core.Level.Off)
				return (int)LocalSyslogAppender.SyslogSeverity.Informational;

			if(level == log4net.Core.Level.Notice 
				|| level == log4net.Core.Level.Verbose 
				|| level == log4net.Core.Level.Trace)
				return (int) LocalSyslogAppender.SyslogSeverity.Notice;

			if(level == log4net.Core.Level.Severe)
				return (int) LocalSyslogAppender.SyslogSeverity.Emergency;
			
			if(level == log4net.Core.Level.Warn)
				return (int) LocalSyslogAppender.SyslogSeverity.Warning;

			return (int) LocalSyslogAppender.SyslogSeverity.Debug;
		}
	}
}
